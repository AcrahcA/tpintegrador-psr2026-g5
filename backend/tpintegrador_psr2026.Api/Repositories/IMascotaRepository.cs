namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IMascotaRepository
{
    List<Mascota> Get();
    Mascota? GetPorId(int id);
    Mascota Post(Mascota entidad);
    bool Put(int id, Mascota entidad);
    bool Delete(int id);
}