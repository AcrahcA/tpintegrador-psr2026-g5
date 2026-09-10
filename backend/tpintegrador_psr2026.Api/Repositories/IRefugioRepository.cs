namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IRefugioRepository
{
    List<Refugio> Get();
    Refugio? GetPorId(int id);
    Refugio Post(Refugio entidad);
    bool Put(int id, Refugio entidad);
    bool Delete(int id);
}