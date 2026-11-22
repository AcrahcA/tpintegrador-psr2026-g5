namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface ISolicitudAdopcionService
{
    List<SolicitudAdopcion> Get();
    List<SolicitudAdopcion> GetPorEstado(EstadoSolicitud estado);
    /// <summary>Solicitudes todavía no resueltas: Pendiente o EnEvaluacion.</summary>
    List<SolicitudAdopcion> GetPendientes();
    List<SolicitudAdopcion> GetPorMascota(int mascotaId);
    SolicitudAdopcion? GetById(int id);
    SolicitudAdopcion ObtenerPorId(int id);
    SolicitudAdopcion Crear(int adoptanteId, int mascotaId);
    /// <summary>Solicitud enviada desde el sitio por una persona con cuenta: queda vinculada para avisarle por el buzón.</summary>
    SolicitudAdopcion CrearPublica(int usuarioId, Adoptante datosAdoptante, int mascotaId);
    /// <summary>Cambia el estado. El motivo es opcional y le llega a la persona en su buzón.</summary>
    SolicitudAdopcion CambiarEstado(int id, EstadoSolicitud nuevoEstado, string? motivo = null);
    SolicitudAdopcion Aprobar(int id);
    SolicitudAdopcion Rechazar(int id, string? motivo = null);
    SolicitudAdopcion Cancelar(int id, string? motivo = null);
    void Eliminar(int id);
}

public class SolicitudAdopcionService : ISolicitudAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IAdoptanteRepository _adoptanteRepository;
    private readonly IAdoptanteService _adoptanteService;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IMascotaService _mascotaService;
    private readonly IAdopcionRepository _adopcionRepository;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly INotificacionService _notificaciones;

    public SolicitudAdopcionService(
        ISolicitudAdopcionRepository solicitudRepository,
        IAdoptanteRepository adoptanteRepository,
        IAdoptanteService adoptanteService,
        IMascotaRepository mascotaRepository,
        IMascotaService mascotaService,
        IAdopcionRepository adopcionRepository,
        IUnidadDeTrabajo unidadDeTrabajo,
        INotificacionService notificaciones)
    {
        _notificaciones = notificaciones;
        _solicitudRepository = solicitudRepository;
        _adoptanteRepository = adoptanteRepository;
        _adoptanteService = adoptanteService;
        _mascotaRepository = mascotaRepository;
        _mascotaService = mascotaService;
        _adopcionRepository = adopcionRepository;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<SolicitudAdopcion> Get() => _solicitudRepository.Get();

    public List<SolicitudAdopcion> GetPorEstado(EstadoSolicitud estado) => _solicitudRepository.GetPorEstado(estado);

    public List<SolicitudAdopcion> GetPendientes() =>
        _solicitudRepository.GetPorEstado(EstadoSolicitud.Pendiente, EstadoSolicitud.EnEvaluacion);

    public List<SolicitudAdopcion> GetPorMascota(int mascotaId) => _solicitudRepository.GetPorMascota(mascotaId);

    public SolicitudAdopcion? GetById(int id) => _solicitudRepository.GetPorId(id);

    public SolicitudAdopcion ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe la solicitud de adopción #{id}.");

    public SolicitudAdopcion Crear(int adoptanteId, int mascotaId)
    {
        if (_adoptanteRepository.GetPorId(adoptanteId) is null)
            throw new RecursoNoEncontradoException($"No existe el adoptante #{adoptanteId}.");
        var mascota = _mascotaRepository.GetPorId(mascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");

        // Regla: sólo pueden realizarse solicitudes sobre mascotas disponibles para adopción
        _mascotaService.ValidarAptaParaAdopcion(mascota, permitirReservada: false);
        if (_adopcionRepository.ExisteAdopcionDeMascota(mascotaId))
            throw new ReglaNegocioException("La mascota ya fue adoptada y no admite nuevas solicitudes.");

        var duplicada = _solicitudRepository.GetPorMascota(mascotaId)
            .Any(s => s.AdoptanteId == adoptanteId && s.EstaVigente);
        if (duplicada)
            throw new ReglaNegocioException("El adoptante ya tiene una solicitud vigente para esta mascota.");

        return _solicitudRepository.Post(new SolicitudAdopcion(adoptanteId, mascotaId));
    }

    public SolicitudAdopcion CrearPublica(int usuarioId, Adoptante datosAdoptante, int mascotaId) =>
        _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            var adoptante = _adoptanteService.ObtenerORegistrarParaUsuario(usuarioId, datosAdoptante);
            return Crear(adoptante.Id, mascotaId);
        });

    public SolicitudAdopcion CambiarEstado(int id, EstadoSolicitud nuevoEstado, string? motivo = null)
    {
        var solicitud = ObtenerPorId(id);
        if (solicitud.Estado == nuevoEstado) return solicitud;
        motivo = Notificacion.NormalizarMotivo(motivo);

        if (_adopcionRepository.ExisteAdopcionDeSolicitud(id))
            throw new ReglaNegocioException("La solicitud ya finalizó en una adopción y no puede cambiar de estado.");
        if (!solicitud.PuedeCambiarA(nuevoEstado))
            throw new ReglaNegocioException($"Una solicitud en estado {solicitud.Estado} no puede pasar a {nuevoEstado}.");

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            var resultado = nuevoEstado switch
            {
                EstadoSolicitud.Aprobada => AprobarInterno(solicitud),
                EstadoSolicitud.Cancelada => CancelarInterno(solicitud),
                _ => Guardar(solicitud, nuevoEstado)
            };

            // Buzón: la persona se entera del resultado (el motivo es opcional)
            var tipo = nuevoEstado switch
            {
                EstadoSolicitud.Aprobada => TipoNotificacion.SolicitudAprobada,
                EstadoSolicitud.Rechazada => TipoNotificacion.SolicitudRechazada,
                EstadoSolicitud.Cancelada => TipoNotificacion.SolicitudCancelada,
                _ => (TipoNotificacion?)null
            };
            if (tipo is not null) AvisarAlAdoptante(resultado, tipo.Value, motivo);
            return resultado;
        });
    }

    public SolicitudAdopcion Aprobar(int id) => CambiarEstado(id, EstadoSolicitud.Aprobada);

    public SolicitudAdopcion Rechazar(int id, string? motivo = null) => CambiarEstado(id, EstadoSolicitud.Rechazada, motivo);

    public SolicitudAdopcion Cancelar(int id, string? motivo = null) => CambiarEstado(id, EstadoSolicitud.Cancelada, motivo);

    private void AvisarAlAdoptante(SolicitudAdopcion s, TipoNotificacion tipo, string? motivo)
    {
        var usuarioId = _adoptanteRepository.GetPorId(s.AdoptanteId)?.UsuarioId;
        if (usuarioId is null) return;
        var mascota = _mascotaRepository.GetPorId(s.MascotaId)?.Nombre ?? $"la mascota #{s.MascotaId}";
        var (titulo, mensaje) = tipo switch
        {
            TipoNotificacion.SolicitudAprobada => ($"¡Tu solicitud para adoptar a {mascota} fue aprobada!",
                $"Solicitud #{s.Id}. El equipo del refugio se va a comunicar con vos para coordinar la entrega."),
            TipoNotificacion.SolicitudRechazada => ($"Tu solicitud para adoptar a {mascota} fue rechazada",
                $"Solicitud #{s.Id}. Podés conocer a otras mascotas disponibles y enviar una nueva solicitud."),
            _ => ($"Tu solicitud para adoptar a {mascota} fue cancelada",
                $"Solicitud #{s.Id}. Si tenés dudas, comunicate con el refugio.")
        };
        _notificaciones.Notificar(usuarioId, tipo, titulo, mensaje, motivo, solicitudAdopcionId: s.Id);
    }

    private SolicitudAdopcion Guardar(SolicitudAdopcion solicitud, EstadoSolicitud estado)
    {
        solicitud.Estado = estado;
        _solicitudRepository.Put(solicitud.Id, solicitud);
        return solicitud;
    }

    private SolicitudAdopcion AprobarInterno(SolicitudAdopcion solicitud)
    {
        var mascota = _mascotaRepository.GetPorId(solicitud.MascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{solicitud.MascotaId}.");

        // Regla: verificar que la mascota continúe disponible antes de aprobar
        _mascotaService.ValidarAptaParaAdopcion(mascota, permitirReservada: false);
        if (_solicitudRepository.GetPorMascota(mascota.Id).Any(s => s.Id != solicitud.Id && s.Estado == EstadoSolicitud.Aprobada))
            throw new ReglaNegocioException("La mascota ya tiene otra solicitud aprobada.");

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            Guardar(solicitud, EstadoSolicitud.Aprobada);

            // La mascota queda reservada para este adoptante hasta finalizar la adopción
            mascota.Estado = EstadoMascota.Reservada;
            _mascotaRepository.Put(mascota.Id, mascota);

            // Sólo una solicitud puede finalizar en adopción: las demás pendientes se rechazan (y se les avisa)
            foreach (var otra in _solicitudRepository.GetPorMascota(mascota.Id).Where(s => s.Id != solicitud.Id && s.EstaPendiente))
            {
                Guardar(otra, EstadoSolicitud.Rechazada);
                AvisarAlAdoptante(otra, TipoNotificacion.SolicitudRechazada, $"{mascota.Nombre} fue reservada para otra familia.");
            }

            return solicitud;
        });
    }

    private SolicitudAdopcion CancelarInterno(SolicitudAdopcion solicitud)
    {
        var estabaAprobada = solicitud.Estado == EstadoSolicitud.Aprobada;
        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            Guardar(solicitud, EstadoSolicitud.Cancelada);

            // Si se cancela una aprobación, la mascota deja de estar reservada
            var mascota = _mascotaRepository.GetPorId(solicitud.MascotaId);
            if (estabaAprobada && mascota is { Estado: EstadoMascota.Reservada })
            {
                mascota.Estado = _mascotaService.CumpleCondicionesSanitarias(mascota.Id)
                    ? EstadoMascota.DisponibleAdopcion
                    : EstadoMascota.EnRecuperacion;
                _mascotaRepository.Put(mascota.Id, mascota);
            }
            return solicitud;
        });
    }

    public void Eliminar(int id)
    {
        var solicitud = ObtenerPorId(id);
        if (solicitud.Estado == EstadoSolicitud.Aprobada || _adopcionRepository.ExisteAdopcionDeSolicitud(id))
            throw new ReglaNegocioException("No se puede eliminar una solicitud aprobada o que finalizó en adopción. Cancelala en su lugar.");
        _solicitudRepository.Delete(id);
    }
}
