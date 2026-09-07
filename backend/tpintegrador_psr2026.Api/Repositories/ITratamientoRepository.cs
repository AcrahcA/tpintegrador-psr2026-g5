namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ITratamientoRepository
{
    List<Tratamiento> ObtenerTodos();
    Tratamiento? ObtenerPorId(int id);
    Tratamiento Agregar(Tratamiento entidad);
    bool Actualizar(int id, Tratamiento entidad);
    bool Eliminar(int id);
}
