namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AdopcionService : IAdopcionService
{
    private readonly IAdopcionRepository _adopciones;
    private readonly ISolicitudAdopcionRepository _solicitudes;
    private readonly IMascotaRepository _mascotas;
    private readonly IHistorialSanitarioRepository? _historiales;
    private readonly ITratamientoRepository? _tratamientos;
    private readonly IOperacionAtomica? _atomica;
    public AdopcionService(IAdopcionRepository adopcionRepository, ISolicitudAdopcionRepository solicitudRepository,
        IMascotaRepository mascotaRepository,
        IHistorialSanitarioRepository? historialRepository = null,
        ITratamientoRepository? tratamientoRepository = null,
        IOperacionAtomica? operacionAtomica = null)
    {
        _adopciones = adopcionRepository; _solicitudes = solicitudRepository; _mascotas = mascotaRepository;
        _historiales = historialRepository; _tratamientos = tratamientoRepository; _atomica = operacionAtomica;
    }
    public List<Adopcion> Get() => _adopciones.Get();
    public Adopcion? GetById(int id) => _adopciones.GetPorId(id);
    public Adopcion? Post(int solicitudId, string? observaciones)
    {
        using var transaccion = _atomica?.Iniciar();
        var solicitud = _solicitudes.GetPorId(solicitudId);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.Aprobada) return null;
        var mascota = _mascotas.GetPorId(solicitud.MascotaId);
        // Incluye Disponibles solo por compatibilidad con solicitudes aprobadas ANTES del cambio.
        if (mascota is null || mascota.Estado is not (EstadoMascota.Reservada or EstadoMascota.DisponibleAdopcion)) return null;
        var solicitudesDeMascota = _solicitudes.Get().Where(s => s.MascotaId == mascota.Id).ToList();
        if (_adopciones.Get().Any(a => solicitudesDeMascota.Any(s => s.Id == a.SolicitudAdopcionId))) return null;
        // Nunca completar si se activó un tratamiento o faltan controles documentados.
        if (_historiales is null || _tratamientos is null ||
            !ReglasSanitarias.Cumple(mascota.Id, _historiales.Get(), _tratamientos.Get())) return null;
        var creada = _adopciones.Post(new Adopcion(solicitud.Id, observaciones));
        mascota.Estado = EstadoMascota.Adoptada;
        if (!_mascotas.Put(mascota.Id, mascota)) throw new OperacionNoPermitidaException("No se pudo finalizar la adopción.");
        foreach (var otra in solicitudesDeMascota.Where(s => s.Id != solicitudId &&
            s.Estado is EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion))
        {
            otra.Estado = EstadoSolicitud.Rechazada;
            if (!_solicitudes.Put(otra.Id, otra)) throw new OperacionNoPermitidaException("No se pudo cerrar otra solicitud.");
        }
        transaccion?.Confirmar();
        return creada;
    }
}
