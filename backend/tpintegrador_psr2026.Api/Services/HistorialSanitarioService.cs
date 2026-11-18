namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface IHistorialSanitarioService
{
    List<HistorialSanitario> Get();
    HistorialSanitario ObtenerPorId(int id);
    /// <summary>Historial de la mascota (se crea si no existiera). Se conserva aunque la mascota sea adoptada.</summary>
    HistorialSanitario ObtenerOCrearPorMascota(int mascotaId);
    HistorialSanitario Crear(int mascotaId, string? observaciones);
    HistorialSanitario ActualizarObservaciones(int id, string? observaciones);
}

public class HistorialSanitarioService : IHistorialSanitarioService
{
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public HistorialSanitarioService(IHistorialSanitarioRepository historialRepository, IMascotaRepository mascotaRepository)
    {
        _historialRepository = historialRepository;
        _mascotaRepository = mascotaRepository;
    }

    public List<HistorialSanitario> Get() => _historialRepository.Get();

    public HistorialSanitario ObtenerPorId(int id) =>
        _historialRepository.GetPorId(id) ?? throw new RecursoNoEncontradoException($"No existe el historial sanitario #{id}.");

    public HistorialSanitario ObtenerOCrearPorMascota(int mascotaId)
    {
        if (_mascotaRepository.GetPorId(mascotaId) is null)
            throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");
        return _historialRepository.GetPorMascota(mascotaId)
               ?? _historialRepository.Post(new HistorialSanitario(mascotaId));
    }

    public HistorialSanitario Crear(int mascotaId, string? observaciones)
    {
        if (_mascotaRepository.GetPorId(mascotaId) is null)
            throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");

        var existente = _historialRepository.GetPorMascota(mascotaId);
        if (existente is not null)
        {
            // Cada mascota tiene un único historial: se actualizan sus observaciones
            existente.Observaciones = observaciones;
            _historialRepository.Put(existente.Id, existente);
            return existente;
        }
        return _historialRepository.Post(new HistorialSanitario(mascotaId, observaciones));
    }

    public HistorialSanitario ActualizarObservaciones(int id, string? observaciones)
    {
        var historial = ObtenerPorId(id);
        historial.Observaciones = observaciones;
        _historialRepository.Put(id, historial);
        return historial;
    }
}
