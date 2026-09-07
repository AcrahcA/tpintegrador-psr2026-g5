namespace tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Domain;

public interface IHistorialSanitarioRepository
{
    List<HistorialSanitario> ObtenerTodos();
    HistorialSanitario? ObtenerPorId(int id);
    HistorialSanitario Agregar(HistorialSanitario entidad);
    bool Actualizar(int id, HistorialSanitario entidad);
    bool Eliminar(int id);
}
