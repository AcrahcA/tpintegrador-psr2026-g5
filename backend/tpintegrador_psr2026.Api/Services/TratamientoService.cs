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
        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == mascotaId);
        if (mascota is null) return null;

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
        _mascotaRepository.Put(mascota.Id, mascota);

        return tratamientoCreado;
    }

    public bool PutEstado(int id, EstadoTratamiento nuevoEstado)
    {
        var tratamiento = _tratamientoRepository.Get().FirstOrDefault(t => t.Id == id);
        if (tratamiento is null) return false;

        tratamiento.Estado = nuevoEstado;
        if (nuevoEstado is EstadoTratamiento.Finalizado or EstadoTratamiento.Suspendido)
            tratamiento.FechaFin = DateTime.Now;

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

                    // Si ya no tiene tratamientos activos y estaba en tratamiento, pasa a estar disponible para adopción
                    if (!tieneOtrosTratamientosActivos && mascota.Estado == EstadoMascota.EnTratamiento)
                    {
                        mascota.Estado = EstadoMascota.DisponibleAdopcion;
                        _mascotaRepository.Put(mascota.Id, mascota);
                    }
                }
            }
        }

        return actualizado;
    }

    public bool Delete(int id) => _tratamientoRepository.Delete(id);
}