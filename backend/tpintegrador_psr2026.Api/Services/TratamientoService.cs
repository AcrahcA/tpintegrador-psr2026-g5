namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class TratamientoService : ITratamientoService
{
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IOperacionAtomica? _atomica;

    public TratamientoService(
        ITratamientoRepository tratamientoRepository,
        IHistorialSanitarioRepository historialRepository,
        IMascotaRepository mascotaRepository,
        IOperacionAtomica? operacionAtomica = null)
    {
        _tratamientoRepository = tratamientoRepository;
        _historialRepository = historialRepository;
        _mascotaRepository = mascotaRepository;
        _atomica = operacionAtomica;
    }

    public List<Tratamiento> Get() => _tratamientoRepository.Get();

    public List<Tratamiento> GetByMascotaId(int mascotaId)
    {
        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == mascotaId);
        if (mascota is null) return new List<Tratamiento>();

        var historial = _historialRepository.Get()
            .FirstOrDefault(h => h.MascotaId == mascotaId);

        if (historial is null) return new List<Tratamiento>();

        return _tratamientoRepository.Get()
            .Where(t => t.HistorialSanitarioId == historial.Id)
            .ToList();
    }

    public Tratamiento? GetById(int id) => _tratamientoRepository.Get().FirstOrDefault(t => t.Id == id);

    public Tratamiento? Post(int mascotaId, Tratamiento tratamiento)
    {
        using var transaccion = _atomica?.Iniciar();
        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == mascotaId);
        if (mascota is null || mascota.Estado is EstadoMascota.Adoptada or EstadoMascota.Reservada) return null;

        var historial = _historialRepository.Get()
            .FirstOrDefault(h => h.MascotaId == mascotaId);

        // Si la mascota no tiene un historial sanitario creado, se genera uno automáticamente usando el constructor
        if (historial is null)
        {
            var nuevoHistorial = new HistorialSanitario(mascotaId);
            historial = _historialRepository.Post(nuevoHistorial);
        }

        tratamiento.HistorialSanitarioId = historial.Id;
        tratamiento.Estado = EstadoTratamiento.Pendiente;
        var tratamientoCreado = _tratamientoRepository.Post(tratamiento);

        // La mascota pasa a estado EnTratamiento
        mascota.Estado = EstadoMascota.EnTratamiento;
        if (!_mascotaRepository.Put(mascota.Id, mascota))
            throw new OperacionNoPermitidaException("No se pudo asignar el estado EnTratamiento. Se revirtió el alta.");
        transaccion?.Confirmar();
        return tratamientoCreado;
    }

    public bool Actualizar(int id, Tratamiento datos)
    {
        var actual = GetById(id);
        if (actual is null || actual.Estado == EstadoTratamiento.Finalizado ||
            _historialRepository.Get().FirstOrDefault(h => h.Id == actual.HistorialSanitarioId) is not { } historial ||
            _mascotaRepository.GetPorId(historial.MascotaId)?.Estado == EstadoMascota.Adoptada) return false;
        actual.Tipo = datos.Tipo;
        actual.Descripcion = datos.Descripcion;
        actual.FechaInicio = datos.FechaInicio;
        return _tratamientoRepository.Put(id, actual);
    }

    public bool PutEstado(int id, EstadoTratamiento nuevoEstado)
    {
        var tratamiento = _tratamientoRepository.Get().FirstOrDefault(t => t.Id == id);
        if (tratamiento is null) return false;
        if (tratamiento.Estado == nuevoEstado) return true;
        if (tratamiento.Estado == EstadoTratamiento.Finalizado) return false;
        if (tratamiento.Estado == EstadoTratamiento.Suspendido && nuevoEstado != EstadoTratamiento.EnCurso) return false;
        if (nuevoEstado == EstadoTratamiento.Pendiente) return false;

        tratamiento.Estado = nuevoEstado;
        if (nuevoEstado == EstadoTratamiento.EnCurso) tratamiento.FechaFin = null;
        if (nuevoEstado is EstadoTratamiento.Finalizado or EstadoTratamiento.Suspendido)
            tratamiento.FechaFin = DateTime.UtcNow;

        var actualizado = _tratamientoRepository.Put(id, tratamiento);

        if (actualizado)
        {
            // Verificar el estado general de tratamientos de la mascota asociada
            var historial = _historialRepository.Get().FirstOrDefault(h => h.Id == tratamiento.HistorialSanitarioId);
            if (historial is not null)
            {
                var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == historial.MascotaId);
                if (mascota is not null)
                {
                    // Comprobar si todavía le quedan tratamientos pendientes o en curso
                    var tieneOtrosTratamientosActivos = _tratamientoRepository.Get()
                        .Any(t => t.HistorialSanitarioId == historial.Id &&
                                 (t.Estado == EstadoTratamiento.Pendiente || t.Estado == EstadoTratamiento.EnCurso));

                    // No habilitar adopción automáticamente: debe pasar por recuperación y revisión sanitaria.
                    if (!tieneOtrosTratamientosActivos && mascota.Estado == EstadoMascota.EnTratamiento)
                    {
                        mascota.Estado = EstadoMascota.EnRecuperacion;
                        _mascotaRepository.Put(mascota.Id, mascota);
                    }
                }
            }
        }

        return actualizado;
    }

    public bool Delete(int id)
    {
        var tratamiento = GetById(id);
        if (tratamiento is null) return false;
        var historial = _historialRepository.GetPorId(tratamiento.HistorialSanitarioId);
        if (historial is null || _mascotaRepository.GetPorId(historial.MascotaId)?.Estado == EstadoMascota.Adoptada)
            return false;
        // Historial clínico: no borrar atenciones finalizadas.
        return tratamiento.Estado == EstadoTratamiento.Pendiente && _tratamientoRepository.Delete(id);
    }
}