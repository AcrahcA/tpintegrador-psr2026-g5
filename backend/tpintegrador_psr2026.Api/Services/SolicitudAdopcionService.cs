namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class SolicitudAdopcionService : ISolicitudAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IAdoptanteRepository _adoptanteRepository;
    private readonly IMascotaService _mascotaService;
    private readonly IMascotaRepository _mascotaRepository;

    public SolicitudAdopcionService(
        ISolicitudAdopcionRepository solicitudRepository,
        IAdoptanteRepository adoptanteRepository,
        IMascotaService mascotaService,
        IMascotaRepository mascotaRepository)
    {
        _solicitudRepository = solicitudRepository;
        _adoptanteRepository = adoptanteRepository;
        _mascotaService = mascotaService;
        _mascotaRepository = mascotaRepository;
    }

    public List<SolicitudAdopcion> Get() => _solicitudRepository.Get();

    public List<SolicitudAdopcion> GetPendientes()
    {
        return _solicitudRepository.Get()
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .ToList();
    }

    public SolicitudAdopcion? GetById(int id) => _solicitudRepository.Get(id);

    // Regla: Solo podrán realizarse solicitudes sobre mascotas disponibles para adopción.
    public SolicitudAdopcion? Post(int adoptanteId, int mascotaId)
    {
        var adoptante = _adoptanteRepository.Get(adoptanteId);
        var mascota = _mascotaRepository.Get(mascotaId);
        if (adoptante is null || mascota is null) return null;

        if (mascota.Estado != EstadoMascota.DisponibleAdopcion)
            return null;

        var solicitud = new SolicitudAdopcion
        {
            AdoptanteId = adoptanteId,
            MascotaId = mascotaId,
            Estado = EstadoSolicitud.Pendiente
        };

        return _solicitudRepository.Post(solicitud);
    }

    public bool PutEstado(int id, EstadoSolicitud nuevoEstado)
    {
        var solicitud = _solicitudRepository.Get(id);
        if (solicitud is null) return false;

        // Regla: Verificar que la mascota continúe disponible en el momento exacto de aprobar la solicitud
        if (nuevoEstado == EstadoSolicitud.Aprobada)
        {
            var mascota = _mascotaRepository.Get(solicitud.MascotaId);
            if (mascota is null || mascota.Estado != EstadoMascota.DisponibleAdopcion)
            {
                // La mascota ya no está disponible
                return false;
            }
        }

        solicitud.Estado = nuevoEstado;
        var actualizado = _solicitudRepository.Put(id, solicitud);

        if (actualizado && nuevoEstado == EstadoSolicitud.Aprobada)
        {
            // 1. Al aprobarse, la mascota pasa a estar Reservada
            _mascotaService.PutEstado(solicitud.MascotaId, EstadoMascota.Reservada);

            // 2. Rechazar automáticamente cualquier otra solicitud pendiente sobre la misma mascota
            var otrasSolicitudes = _solicitudRepository.Get()
                .Where(s => s.MascotaId == solicitud.MascotaId && s.Id != id && s.Estado == EstadoSolicitud.Pendiente);

            foreach (var otra in otrasSolicitudes)
            {
                otra.Estado = EstadoSolicitud.Rechazada;
                _solicitudRepository.Put(otra.Id, otra);
            }
        }

        return actualizado;
    }

    public bool Delete(int id) => _solicitudRepository.Delete(id);
}