namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAdoptanteRepository
{
    List<Adoptante> ObtenerTodos();
    Adoptante? ObtenerPorId(int id);
    Adoptante Agregar(Adoptante entidad);
    bool Actualizar(int id, Adoptante entidad);
    bool Eliminar(int id);
}
