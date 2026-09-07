namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;
public interface IMascotaRepository
{
    List<Mascota> ObtenerTodos();
    Mascota? ObtenerPorId(int id);
    Mascota Agregar(Mascota entidad);
    bool Actualizar(int id, Mascota entidad);
    bool Eliminar(int id);
}
