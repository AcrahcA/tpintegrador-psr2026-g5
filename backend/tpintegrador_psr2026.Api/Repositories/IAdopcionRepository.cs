namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAdopcionRepository
{
    List<Adopcion> ObtenerTodos();
    Adopcion? ObtenerPorId(int id);
    Adopcion Agregar(Adopcion entidad);
    bool Actualizar(int id, Adopcion entidad);
    bool Eliminar(int id);
}
