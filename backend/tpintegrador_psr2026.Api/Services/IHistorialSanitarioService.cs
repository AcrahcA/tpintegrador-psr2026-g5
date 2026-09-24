namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IHistorialSanitarioService
{
    List<HistorialSanitario> Get();
    HistorialSanitario? GetById(int id);
    HistorialSanitario? GetByMascotaId(int mascotaId);
    HistorialSanitario Post(HistorialSanitario historial);
    bool Put(int id, HistorialSanitario historial);
    bool Delete(int id);
}