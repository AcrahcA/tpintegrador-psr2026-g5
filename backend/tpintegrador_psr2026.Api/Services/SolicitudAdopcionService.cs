namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class SolicitudAdopcionService : ISolicitudAdopcionService
{
    private readonly ISolicitudAdopcionRepository _solicitudes;
    private readonly IAdoptanteRepository _adoptantes;
    private readonly IMascotaRepository _mascotas;
    private readonly ITratamientoRepository _tratamientos;
    private readonly IHistorialSanitarioRepository _historiales;
    private readonly IOperacionAtomica? _atomica;

    public SolicitudAdopcionService(ISolicitudAdopcionRepository solicitudRepository,
        IAdoptanteRepository adoptanteRepository, IMascotaRepository mascotaRepository,
        ITratamientoRepository tratamientoRepository, IHistorialSanitarioRepository historialRepository,
        IOperacionAtomica? operacionAtomica = null)
    {
        _solicitudes = solicitudRepository; _adoptantes = adoptanteRepository;
        _mascotas = mascotaRepository; _tratamientos = tratamientoRepository;
        _historiales = historialRepository; _atomica = operacionAtomica;
    }
    public List<SolicitudAdopcion> Get() => _solicitudes.Get();
    public List<SolicitudAdopcion> GetPendientes() =>
        _solicitudes.Get().Where(s => s.Estado is EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion).ToList();
    public SolicitudAdopcion? GetById(int id) => _solicitudes.GetPorId(id);
    public bool EsMascotaAptaParaAdopcion(int mascotaId)
    {
        var mascota = _mascotas.GetPorId(mascotaId);
        return mascota is not null && mascota.Estado == EstadoMascota.DisponibleAdopcion &&
            ReglasSanitarias.Cumple(mascotaId, _historiales.Get(), _tratamientos.Get());
    }
    public SolicitudAdopcion? Post(int adoptanteId, int mascotaId)
    {
        if (_adoptantes.GetPorId(adoptanteId) is null || !EsMascotaAptaParaAdopcion(mascotaId)) return null;
        if (_solicitudes.Get().Any(s => s.MascotaId == mascotaId && s.AdoptanteId == adoptanteId &&
            s.Estado is EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion or EstadoSolicitud.Aprobada)) return null;
        return _solicitudes.Post(new SolicitudAdopcion(adoptanteId, mascotaId));
    }
    public bool PutEstado(int id, EstadoSolicitud estado)
    {
        using var transaccion = _atomica?.Iniciar();
        var actual = GetById(id);
        if (actual is null) return false;
        if (actual.Estado == estado) return true;
        if (actual.Estado is EstadoSolicitud.Rechazada or EstadoSolicitud.Cancelada) return false;
        // Ya aprobada solo puede cancelarse, mientras no exista una adopción definitiva.
        if (actual.Estado == EstadoSolicitud.Aprobada && estado != EstadoSolicitud.Cancelada) return false;
        if (actual.Estado != EstadoSolicitud.Aprobada && estado is not
            (EstadoSolicitud.EnEvaluacion or EstadoSolicitud.Aprobada or EstadoSolicitud.Rechazada or EstadoSolicitud.Cancelada))
            return false;

        if (estado == EstadoSolicitud.Aprobada)
        {
            var mascota = _mascotas.GetPorId(actual.MascotaId);
            if (mascota is null || !EsMascotaAptaParaAdopcion(actual.MascotaId)) return false;
            if (_solicitudes.Get().Any(s => s.MascotaId == mascota.Id && s.Id != id && s.Estado == EstadoSolicitud.Aprobada)) return false;
            mascota.Estado = EstadoMascota.Reservada;
            if (!_mascotas.Put(mascota.Id, mascota)) throw new OperacionNoPermitidaException("No se pudo reservar la mascota.");
            foreach (var otra in _solicitudes.Get().Where(s => s.MascotaId == mascota.Id && s.Id != id &&
                s.Estado is EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion).ToList())
            {
                otra.Estado = EstadoSolicitud.Rechazada;
                if (!_solicitudes.Put(otra.Id, otra)) throw new OperacionNoPermitidaException("No se pudo cerrar otra solicitud.");
            }
        }
        if (actual.Estado == EstadoSolicitud.Aprobada && estado == EstadoSolicitud.Cancelada)
        {
            var mascota = _mascotas.GetPorId(actual.MascotaId);
            if (mascota is null || mascota.Estado is not (EstadoMascota.Reservada or EstadoMascota.DisponibleAdopcion)) return false;
            if (mascota.Estado == EstadoMascota.Reservada)
            {
                mascota.Estado = EstadoMascota.DisponibleAdopcion;
                if (!_mascotas.Put(mascota.Id, mascota)) throw new OperacionNoPermitidaException("No se pudo liberar la reserva.");
            }
        }
        actual.Estado = estado;
        if (!_solicitudes.Put(id, actual)) throw new OperacionNoPermitidaException("No se pudo actualizar la solicitud.");
        transaccion?.Confirmar();
        return true;
    }
    public bool Delete(int id)
    {
        var solicitud = GetById(id);
        if (solicitud is null || solicitud.Estado == EstadoSolicitud.Aprobada) return false;
        // Conservar aprobaciones y negativas para auditar el proceso; sólo cancelar mediante estado.
        return solicitud.Estado == EstadoSolicitud.Pendiente && _solicitudes.Delete(id);
    }
}
