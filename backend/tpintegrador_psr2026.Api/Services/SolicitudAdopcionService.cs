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

    public List<SolicitudAdopcion> ObtenerSolicitudes() => _solicitudRepository.ObtenerTodos();

    public List<SolicitudAdopcion> ObtenerPendientes()
    {
        return _solicitudRepository.ObtenerTodos()
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .ToList();
    }

    public SolicitudAdopcion? BuscarSolicitud(int id) => _solicitudRepository.ObtenerPorId(id);

    // Regla: solo podrán realizarse solicitudes sobre mascotas disponibles para adopción.
    public SolicitudAdopcion? RealizarSolicitud(int adoptanteId, int mascotaId)
    {
        var adoptante = _adoptanteRepository.ObtenerPorId(adoptanteId);
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (adoptante is null || mascota is null) return null;

        if (mascota.Estado != EstadoMascota.DisponibleAdopcion)
            return null;

        var solicitud = new SolicitudAdopcion
        {
            AdoptanteId = adoptanteId,
            MascotaId = mascotaId,
            Estado = EstadoSolicitud.Pendiente
        };

        return _solicitudRepository.Agregar(solicitud);
    }

    public bool CambiarEstado(int id, EstadoSolicitud nuevoEstado)
    {
        var solicitud = _solicitudRepository.ObtenerPorId(id);
        if (solicitud is null) return false;

        // Regla: Verificar que la mascota continúe disponible en el momento exacto de aprobar la solicitud
        if (nuevoEstado == EstadoSolicitud.Aprobada)
        {
            var mascota = _mascotaRepository.ObtenerPorId(solicitud.MascotaId);
            if (mascota is null || mascota.Estado != EstadoMascota.DisponibleAdopcion)
            {
                // La mascota ya no está disponible
                return false;
            }
        }

        solicitud.Estado = nuevoEstado;
        var actualizado = _solicitudRepository.Actualizar(id, solicitud);

        // Al aprobarse una solicitud, la mascota queda reservada hasta que se concrete la adopción definitiva.
        if (actualizado && nuevoEstado == EstadoSolicitud.Aprobada)
            _mascotaService.CambiarEstado(solicitud.MascotaId, EstadoMascota.Reservada);

        return actualizado;
    }

    public bool EliminarSolicitud(int id) => _solicitudRepository.Eliminar(id);
}