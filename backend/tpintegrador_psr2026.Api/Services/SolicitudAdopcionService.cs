namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class SolicitudAdopcionService : ISolicitudAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IAdoptanteRepository _adoptanteRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;

    public SolicitudAdopcionService(
        ISolicitudAdopcionRepository solicitudRepository,
        IAdoptanteRepository adoptanteRepository,
        IMascotaRepository mascotaRepository,
        ITratamientoRepository tratamientoRepository,
        IHistorialSanitarioRepository historialRepository)
    {
        _solicitudRepository = solicitudRepository;
        _adoptanteRepository = adoptanteRepository;
        _mascotaRepository = mascotaRepository;
        _tratamientoRepository = tratamientoRepository;
        _historialRepository = historialRepository;
    }

    public List<SolicitudAdopcion> Get() => _solicitudRepository.Get();

    public List<SolicitudAdopcion> GetPendientes() =>
        _solicitudRepository.Get()
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .ToList();

    public SolicitudAdopcion? GetById(int id) => _solicitudRepository.Get().FirstOrDefault(s => s.Id == id);

    public SolicitudAdopcion? Post(int adoptanteId, int mascotaId)
    {
        var adoptante = _adoptanteRepository.Get().FirstOrDefault(a => a.Id == adoptanteId);
        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == mascotaId);

        if (adoptante is null || mascota is null) return null;
        if (mascota.Estado != EstadoMascota.DisponibleAdopcion) return null;

        var nuevaSolicitud = new SolicitudAdopcion
        {
            AdoptanteId = adoptanteId,
            MascotaId = mascotaId,
            FechaSolicitud = DateTime.Now,
            Estado = EstadoSolicitud.Pendiente
        };

        return _solicitudRepository.Post(nuevaSolicitud);
    }

    public bool PutEstado(int id, EstadoSolicitud nuevoEstado)
    {
        var solicitud = _solicitudRepository.Get().FirstOrDefault(s => s.Id == id);
        if (solicitud is null) return false;

        if (nuevoEstado == EstadoSolicitud.Aprobada)
        {
            var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == solicitud.MascotaId);
            if (mascota is null || mascota.Estado != EstadoMascota.DisponibleAdopcion)
                return false;
        }

        solicitud.Estado = nuevoEstado;
        return _solicitudRepository.Put(id, solicitud);
    }

    public bool EsMascotaAptaParaAdopcion(int mascotaId)
    {
        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == mascotaId);
        if (mascota is null || mascota.Estado != EstadoMascota.DisponibleAdopcion) return false;

        var historial = _historialRepository.Get().FirstOrDefault(h => h.MascotaId == mascotaId);
        if (historial is null) return false;

        var tratamientosActivos = _tratamientoRepository.Get()
            .Any(t => t.HistorialSanitarioId == historial.Id && t.Estado == EstadoTratamiento.EnCurso);

        return !tratamientosActivos;
    }

    public bool Delete(int id) => _solicitudRepository.Delete(id);
}