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

        if (historial is null) return null;

        tratamiento.HistorialSanitarioId = historial.Id;
        tratamiento.Estado = EstadoTratamiento.Pendiente;
        var tratamientoCreado = _tratamientoRepository.Post(tratamiento);

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

        return _tratamientoRepository.Put(id, tratamiento);
    }

    public bool Delete(int id) => _tratamientoRepository.Delete(id);
}