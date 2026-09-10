namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IHistorialSanitarioRepository
{
    List<HistorialSanitario> Get();
    HistorialSanitario? GetPorId(int id);
    HistorialSanitario Post(HistorialSanitario entidad);
    bool Put(int id, HistorialSanitario entidad);
    bool Delete(int id);
}