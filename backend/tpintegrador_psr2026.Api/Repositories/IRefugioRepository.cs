namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IRefugioRepository
{
    List<Refugio> ObtenerTodos();
    Refugio? ObtenerPorId(int id);
    Refugio Agregar(Refugio entidad);
    bool Actualizar(int id, Refugio entidad);
    bool Eliminar(int id);
}
