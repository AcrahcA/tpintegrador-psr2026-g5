namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ICuidadorRepository
{
    List<Cuidador> ObtenerTodos();
    Cuidador? ObtenerPorId(int id);
    Cuidador Agregar(Cuidador entidad);
    bool Actualizar(int id, Cuidador entidad);
    bool Eliminar(int id);
}
