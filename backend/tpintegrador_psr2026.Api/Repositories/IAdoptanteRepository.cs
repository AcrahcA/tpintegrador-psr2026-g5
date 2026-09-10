namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAdoptanteRepository
{
    List<Adoptante> Get();
    Adoptante? GetPorId(int id);
    Adoptante Post(Adoptante entidad);
    bool Put(int id, Adoptante entidad);
    bool Delete(int id);
}