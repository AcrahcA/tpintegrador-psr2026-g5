namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotaRepository;
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IRefugioService _refugioService;

    public MascotaService(
        IMascotaRepository mascotaRepository,
        ICuidadorRepository cuidadorRepository,
        IHistorialSanitarioRepository historialRepository,
        ITratamientoRepository tratamientoRepository,
        IRefugioService refugioService)
    {
        _mascotaRepository = mascotaRepository;
        _cuidadorRepository = cuidadorRepository;
        _historialRepository = historialRepository;
        _tratamientoRepository = tratamientoRepository;
        _refugioService = refugioService;
    }

    public List<Mascota> ObtenerMascotas() => _mascotaRepository.ObtenerTodos();

    public Mascota? BuscarMascota(int id) => _mascotaRepository.ObtenerPorId(id);

    // Regla: no podran ingresar nuevas mascotas cuando el refugio haya alcanzado su capacidad maxima.
    public Mascota? IngresarMascota(Mascota mascota)
    {
        if (!_refugioService.TieneLugarDisponible())
            return null;

        mascota.Estado = EstadoMascota.Ingresada;
        mascota.FechaIngreso = DateTime.Now;

        var mascotaCreada = _mascotaRepository.Agregar(mascota);

        // Toda mascota nace con su propio historial sanitario (composicion)
        var historial = _historialRepository.Agregar(new HistorialSanitario { MascotaId = mascotaCreada.Id });
        mascotaCreada.HistorialSanitarioId = historial.Id;
        _mascotaRepository.Actualizar(mascotaCreada.Id, mascotaCreada);

        return mascotaCreada;
    }

    public bool CambiarEstado(int id, EstadoMascota nuevoEstado)
    {
        var mascota = _mascotaRepository.ObtenerPorId(id);
        if (mascota is null) return false;

        if (nuevoEstado == EstadoMascota.DisponibleAdopcion && !EstaDisponibleParaAdopcion(id))
            return false;

        mascota.Estado = nuevoEstado;
        return _mascotaRepository.Actualizar(id, mascota);
    }

    // Regla: un cuidador no podra tener asignadas mas mascotas que su capacidad maxima.
    public bool AsignarCuidador(int mascotaId, int cuidadorId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        var cuidador = _cuidadorRepository.ObtenerPorId(cuidadorId);
        if (mascota is null || cuidador is null) return false;

        var cantidadAsignadas = _mascotaRepository.ObtenerTodos()
            .Count(m => m.CuidadorId == cuidadorId);

        if (cantidadAsignadas >= cuidador.CapacidadMaxima)
            return false;

        mascota.CuidadorId = cuidadorId;
        return _mascotaRepository.Actualizar(mascotaId, mascota);
    }

    // Regla: mientras una mascota tenga tratamientos activos, no podra encontrarse
    // disponible para adopcion.
    public bool EstaDisponibleParaAdopcion(int mascotaId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return false;

        if (mascota.Estado is EstadoMascota.Reservada or EstadoMascota.Adoptada)
            return false;

        return CumpleCondicionesSanitarias(mascotaId);
    }

    // Regla: una mascota debe cumplir las condiciones sanitarias establecidas antes de
    // ser habilitada para adopcion (no puede tener tratamientos pendientes o en curso).
    public bool CumpleCondicionesSanitarias(int mascotaId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return false;

        var tratamientosActivos = _tratamientoRepository.ObtenerTodos()
            .Where(t => t.HistorialSanitarioId == mascota.HistorialSanitarioId)
            .Any(t => t.Estado is EstadoTratamiento.Pendiente or EstadoTratamiento.EnCurso);

        return !tratamientosActivos;
    }

    public bool EliminarMascota(int id) => _mascotaRepository.Eliminar(id);
}
