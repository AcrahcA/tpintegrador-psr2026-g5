namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface IAdopcionService
{
    List<Adopcion> Get();
    Adopcion? GetById(int id);
    Adopcion? GetPorMascota(int mascotaId);
    Adopcion Registrar(int solicitudId, string? observaciones, DateTime? fechaAdopcion = null);
}

public class AdopcionService : IAdopcionService
{
    private readonly IAdopcionRepository _adopcionRepository;
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IMascotaService _mascotaService;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IAdoptanteRepository _adoptanteRepository;
    private readonly INotificacionService _notificaciones;

    public AdopcionService(
        IAdopcionRepository adopcionRepository,
        ISolicitudAdopcionRepository solicitudRepository,
        IMascotaRepository mascotaRepository,
        IMascotaService mascotaService,
        IUnidadDeTrabajo unidadDeTrabajo,
        IAdoptanteRepository adoptanteRepository,
        INotificacionService notificaciones)
    {
        _adoptanteRepository = adoptanteRepository;
        _notificaciones = notificaciones;
        _adopcionRepository = adopcionRepository;
        _solicitudRepository = solicitudRepository;
        _mascotaRepository = mascotaRepository;
        _mascotaService = mascotaService;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<Adopcion> Get() => _adopcionRepository.Get();

    public Adopcion? GetById(int id) => _adopcionRepository.GetPorId(id);

    public Adopcion? GetPorMascota(int mascotaId) => _adopcionRepository.GetPorMascota(mascotaId);

    public Adopcion Registrar(int solicitudId, string? observaciones, DateTime? fechaAdopcion = null)
    {
        var solicitud = _solicitudRepository.GetPorId(solicitudId)
                        ?? throw new RecursoNoEncontradoException($"No existe la solicitud de adopción #{solicitudId}.");

        // Regla: la solicitud debe encontrarse aprobada antes de completar la adopción
        if (solicitud.Estado != EstadoSolicitud.Aprobada)
            throw new ReglaNegocioException($"La solicitud debe estar aprobada para completar la adopción (estado actual: {solicitud.Estado}).");

        // Regla: no pueden existir dos adopciones definitivas sobre una misma mascota
        if (_adopcionRepository.ExisteAdopcionDeSolicitud(solicitudId) || _adopcionRepository.ExisteAdopcionDeMascota(solicitud.MascotaId))
            throw new ReglaNegocioException("La mascota ya tiene una adopción definitiva registrada.");

        var mascota = _mascotaRepository.GetPorId(solicitud.MascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{solicitud.MascotaId}.");

        // Reglas: la mascota continúa disponible (reservada para esta solicitud), sin tratamientos
        // que impidan la adopción y con las vacunas / controles requeridos
        _mascotaService.ValidarAptaParaAdopcion(mascota, permitirReservada: true);

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            var adopcion = _adopcionRepository.Post(new Adopcion(solicitud, observaciones, fechaAdopcion));

            // La mascota pasa automáticamente a Adoptada (se conserva en el historial)
            mascota.Estado = EstadoMascota.Adoptada;
            _mascotaRepository.Put(mascota.Id, mascota);

            // Buzón: felicitaciones a quien adoptó
            _notificaciones.Notificar(_adoptanteRepository.GetPorId(solicitud.AdoptanteId)?.UsuarioId,
                TipoNotificacion.AdopcionConcretada, $"¡{mascota.Nombre} ya es parte de tu familia!",
                $"Registramos la adopción (solicitud #{solicitud.Id}). Gracias por darle un hogar.",
                solicitudAdopcionId: solicitud.Id);

            foreach (var otra in _solicitudRepository.GetPorMascota(mascota.Id).Where(s => s.Id != solicitud.Id && s.EstaPendiente))
            {
                otra.Estado = EstadoSolicitud.Rechazada;
                _solicitudRepository.Put(otra.Id, otra);
                _notificaciones.Notificar(_adoptanteRepository.GetPorId(otra.AdoptanteId)?.UsuarioId,
                    TipoNotificacion.SolicitudRechazada, $"Tu solicitud para adoptar a {mascota.Nombre} fue rechazada",
                    $"Solicitud #{otra.Id}. Podés conocer a otras mascotas disponibles y enviar una nueva solicitud.",
                    $"{mascota.Nombre} ya fue adoptada por otra familia.", solicitudAdopcionId: otra.Id);
            }
            return adopcion;
        });
    }
}
