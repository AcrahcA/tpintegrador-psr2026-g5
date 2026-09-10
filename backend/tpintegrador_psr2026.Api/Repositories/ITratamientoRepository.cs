namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ITratamientoRepository
{
    List<Tratamiento> Get();
    Tratamiento? GetPorId(int id);
    Tratamiento Post(Tratamiento entidad);
    bool Put(int id, Tratamiento entidad);
    bool Delete(int id);
}