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

    // Regla: No podrán ingresar nuevas mascotas cuando el refugio haya alcanzado su capacidad máxima.
    public Mascota? IngresarMascota(Mascota mascota)
    {
        if (!_refugioService.TieneLugarDisponible())
            return null;

        mascota.Estado = EstadoMascota.Ingresada;
        mascota.FechaIngreso = DateTime.Now;

        var mascotaCreada = _mascotaRepository.Agregar(mascota);

        // Toda mascota nace con su propio historial sanitario (Composición UML: MascotaId asignado)
        var historial = _historialRepository.Agregar(new HistorialSanitario { MascotaId = mascotaCreada.Id });
        mascotaCreada.HistorialSanitario = historial;

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

    // Regla: Un cuidador no podrá tener asignadas más mascotas que su capacidad máxima.
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

    // Regla: Mientras una mascota tenga tratamientos activos, no podrá encontrarse disponible para adopción.
    public bool EstaDisponibleParaAdopcion(int mascotaId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return false;

        if (mascota.Estado is EstadoMascota.Reservada or EstadoMascota.Adoptada)
            return false;

        return CumpleCondicionesSanitarias(mascotaId);
    }

    // Regla: Una mascota debe cumplir las condiciones sanitarias antes de ser habilitada para adopción.
    public bool CumpleCondicionesSanitarias(int mascotaId)
    {
        var mascota = _mascotaRepository.ObtenerPorId(mascotaId);
        if (mascota is null) return false;

        // Buscar el historial sanitario que le pertenece a la mascota por MascotaId
        var historial = _historialRepository.ObtenerTodos()
            .FirstOrDefault(h => h.MascotaId == mascotaId);

        if (historial is null) return true;

        var tratamientosActivos = _tratamientoRepository.ObtenerTodos()
            .Where(t => t.HistorialSanitarioId == historial.Id)
            .Any(t => t.Estado is EstadoTratamiento.Pendiente or EstadoTratamiento.EnCurso);

        return !tratamientosActivos;
    }

    public bool EliminarMascota(int id) => _mascotaRepository.Eliminar(id);
}