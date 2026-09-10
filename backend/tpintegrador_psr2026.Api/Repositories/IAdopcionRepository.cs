namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAdopcionRepository
{
    List<Adopcion> Get();
    Adopcion? GetPorId(int id);
    Adopcion Post(Adopcion entidad);
    bool Put(int id, Adopcion entidad);
    bool Delete(int id);
}