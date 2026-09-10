namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAvisoRescateRepository
{
    List<AvisoRescate> Get();
    AvisoRescate? GetPorId(int id);
    AvisoRescate Post(AvisoRescate entidad);
    bool Put(int id, AvisoRescate entidad);
    bool Delete(int id);
}