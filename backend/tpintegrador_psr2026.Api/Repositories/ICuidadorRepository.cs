namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ICuidadorRepository
{
    List<Cuidador> Get();
    Cuidador? GetPorId(int id);
    Cuidador Post(Cuidador entidad);
    bool Put(int id, Cuidador entidad);
    bool Delete(int id);
}