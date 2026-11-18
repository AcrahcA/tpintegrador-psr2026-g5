namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface ITratamientoService
{
    List<Tratamiento> Get();
    List<Tratamiento> GetByMascotaId(int mascotaId);
    Tratamiento? GetById(int id);
    Tratamiento ObtenerPorId(int id);
    Tratamiento Registrar(int mascotaId, Tratamiento tratamiento);
    Tratamiento Actualizar(int id, TipoTratamiento tipo, string? descripcion, DateTime? fechaInicio);
    Tratamiento CambiarEstado(int id, EstadoTratamiento nuevoEstado);
    void Eliminar(int id);
    int ObtenerMascotaId(Tratamiento tratamiento);
}

public class TratamientoService : ITratamientoService
{
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IHistorialSanitarioService _historialService;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public TratamientoService(
        ITratamientoRepository tratamientoRepository,
        IHistorialSanitarioService historialService,
        IHistorialSanitarioRepository historialRepository,
        IMascotaRepository mascotaRepository,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _tratamientoRepository = tratamientoRepository;
        _historialService = historialService;
        _historialRepository = historialRepository;
        _mascotaRepository = mascotaRepository;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<Tratamiento> Get() => _tratamientoRepository.Get();

    public List<Tratamiento> GetByMascotaId(int mascotaId)
    {
        if (_mascotaRepository.GetPorId(mascotaId) is null)
            throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");
        return _tratamientoRepository.GetPorMascota(mascotaId);
    }

    public Tratamiento? GetById(int id) => _tratamientoRepository.GetPorId(id);

    public Tratamiento ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe el tratamiento #{id}.");

    public int ObtenerMascotaId(Tratamiento tratamiento) =>
        _historialRepository.GetPorId(tratamiento.HistorialSanitarioId)?.MascotaId ?? 0;

    public Tratamiento Registrar(int mascotaId, Tratamiento tratamiento)
    {
        var mascota = _mascotaRepository.GetPorId(mascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");
        if (mascota.FueAdoptada)
            throw new ReglaNegocioException("La mascota ya fue adoptada: su historial se conserva pero no admite nuevos tratamientos.");

        tratamiento.Validar();
        if (!tratamiento.EstaActivo && tratamiento.FechaFin is null)
            tratamiento.FechaFin = DateTime.UtcNow;
        if (tratamiento.EstaActivo)
            tratamiento.FechaFin = null;

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            var historial = _historialService.ObtenerOCrearPorMascota(mascotaId);
            tratamiento.HistorialSanitarioId = historial.Id;
            var creado = _tratamientoRepository.Post(tratamiento);
            AjustarEstadoMascota(mascota);
            return creado;
        });
    }

    public Tratamiento Actualizar(int id, TipoTratamiento tipo, string? descripcion, DateTime? fechaInicio)
    {
        var tratamiento = ObtenerPorId(id);
        if (tratamiento.Estado == EstadoTratamiento.Finalizado)
            throw new ReglaNegocioException("Un tratamiento finalizado forma parte del historial y no puede modificarse.");

        tratamiento.Tipo = tipo;
        tratamiento.Descripcion = descripcion;
        if (fechaInicio is { } f && f != default) tratamiento.FechaInicio = Fechas.AUtc(f);
        tratamiento.Validar();

        _tratamientoRepository.Put(id, tratamiento);
        return tratamiento;
    }

    public Tratamiento CambiarEstado(int id, EstadoTratamiento nuevoEstado)
    {
        var tratamiento = ObtenerPorId(id);
        if (tratamiento.Estado == nuevoEstado) return tratamiento;

        var mascota = MascotaDe(tratamiento);
        if (mascota.FueAdoptada)
            throw new ReglaNegocioException("La mascota ya fue adoptada: su historial sanitario no puede modificarse.");

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            tratamiento.CambiarEstado(nuevoEstado);
            _tratamientoRepository.Put(id, tratamiento);
            AjustarEstadoMascota(mascota);
            return tratamiento;
        });
    }

    public void Eliminar(int id)
    {
        var tratamiento = ObtenerPorId(id);
        if (tratamiento.Estado != EstadoTratamiento.Pendiente)
            throw new ReglaNegocioException("Sólo pueden eliminarse tratamientos pendientes (cargados por error). Los demás forman parte del historial sanitario.");

        var mascota = MascotaDe(tratamiento);
        _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            _tratamientoRepository.Delete(id);
            AjustarEstadoMascota(mascota);
            return true;
        });
    }

    private Mascota MascotaDe(Tratamiento tratamiento) =>
        _mascotaRepository.GetPorId(ObtenerMascotaId(tratamiento))
        ?? throw new RecursoNoEncontradoException("No se encontró la mascota asociada al tratamiento.");

    /// <summary>
    /// Sincroniza el estado de la mascota con sus tratamientos:
    /// con tratamientos activos pasa a "En tratamiento"; al finalizarlos todos pasa a "En recuperación".
    /// </summary>
    private void AjustarEstadoMascota(Mascota mascota)
    {
        var tieneActivos = _tratamientoRepository.TieneTratamientosActivos(mascota.Id);
        var nuevo = mascota.Estado;

        if (tieneActivos && mascota.Estado is EstadoMascota.Ingresada or EstadoMascota.EnRecuperacion or EstadoMascota.DisponibleAdopcion)
            nuevo = EstadoMascota.EnTratamiento;
        else if (!tieneActivos && mascota.Estado == EstadoMascota.EnTratamiento)
            nuevo = EstadoMascota.EnRecuperacion;

        if (nuevo != mascota.Estado)
        {
            mascota.Estado = nuevo;
            _mascotaRepository.Put(mascota.Id, mascota);
        }
    }
}
