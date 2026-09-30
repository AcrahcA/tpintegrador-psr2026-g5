namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class HistorialSanitarioService : IHistorialSanitarioService
{
    private readonly IHistorialSanitarioRepository _historialRepository;

    public HistorialSanitarioService(IHistorialSanitarioRepository historialRepository)
    {
        _historialRepository = historialRepository;
    }

    public List<HistorialSanitario> Get() => _historialRepository.Get();

    public HistorialSanitario? GetById(int id) => _historialRepository.Get().FirstOrDefault(h => h.Id == id);

    public HistorialSanitario? GetByMascotaId(int mascotaId) =>
        _historialRepository.Get().FirstOrDefault(h => h.MascotaId == mascotaId);

    public HistorialSanitario Post(HistorialSanitario historial) => _historialRepository.Post(historial);

    public bool Put(int id, HistorialSanitario historial) => _historialRepository.Put(id, historial);

    public bool Delete(int id) => _historialRepository.Delete(id);
}