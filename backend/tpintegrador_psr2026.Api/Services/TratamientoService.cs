namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class TratamientoService : ITratamientoService
{
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public TratamientoService(
        ITratamientoRepository tratamientoRepository,
        IHistorialSanitarioRepository historialRepository,
        IMascotaRepository mascotaRepository)
    {
        _tratamientoRepository = tratamientoRepository;
        _historialRepository = historialRepository;
        _mascotaRepository = mascotaRepository;
    }

    public List<Tratamiento> ObtenerTratamientos() => _tratamientoRepository.ObtenerTodos();

    public List<Tratamiento> ObtenerTratamientosDeMascota(int mascotaId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return new List<Tratamiento>();

        var historial = _historialRepository.ObtenerTodos()
            .FirstOrDefault(h => h.MascotaId == mascotaId);

        if (historial is null) return new List<Tratamiento>();

        return _tratamientoRepository.ObtenerTodos()
            .Where(t => t.HistorialSanitarioId == historial.Id)
            .ToList();
    }

    public Tratamiento? BuscarTratamiento(int id) => _tratamientoRepository.ObtenerPorId(id);

    // Un tratamiento nuevo deja a la mascota fuera de disponibilidad para adopción
    public Tratamiento? RegistrarTratamiento(int mascotaId, Tratamiento tratamiento)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return null;

        var historial = _historialRepository.ObtenerTodos()
            .FirstOrDefault(h => h.MascotaId == mascotaId);

        if (historial is null) return null;

        tratamiento.HistorialSanitarioId = historial.Id;
        tratamiento.Estado = EstadoTratamiento.Pendiente;
        var tratamientoCreado = _tratamientoRepository.Agregar(tratamiento);

        mascota.Estado = EstadoMascota.EnTratamiento;
        _mascotaRepository.Actualizar(mascota.Id, mascota);

        return tratamientoCreado;
    }

    public bool CambiarEstado(int id, EstadoTratamiento nuevoEstado)
    {
        var tratamiento = _tratamientoRepository.ObtenerPorId(id);
        if (tratamiento is null) return false;

        tratamiento.Estado = nuevoEstado;
        if (nuevoEstado is EstadoTratamiento.Finalizado or EstadoTratamiento.Suspendido)
            tratamiento.FechaFin = DateTime.Now;

        return _tratamientoRepository.Actualizar(id, tratamiento);
    }

    public bool EliminarTratamiento(int id) => _tratamientoRepository.Eliminar(id);
}