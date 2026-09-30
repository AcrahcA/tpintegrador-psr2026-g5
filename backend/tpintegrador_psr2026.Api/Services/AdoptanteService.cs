namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AdoptanteService : IAdoptanteService
{
    private readonly IAdoptanteRepository _adoptanteRepository;

    public AdoptanteService(IAdoptanteRepository adoptanteRepository)
    {
        _adoptanteRepository = adoptanteRepository;
    }

    public List<Adoptante> Get() => _adoptanteRepository.Get();

    public Adoptante? GetById(int id) => _adoptanteRepository.Get().FirstOrDefault(a => a.Id == id);

    public Adoptante Post(Adoptante adoptante) => _adoptanteRepository.Post(adoptante);

    public bool Delete(int id) => _adoptanteRepository.Delete(id);
}