namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ISolicitudAdopcionRepository
{
    List<SolicitudAdopcion> ObtenerTodos();
    SolicitudAdopcion? ObtenerPorId(int id);
    SolicitudAdopcion Agregar(SolicitudAdopcion entidad);
    bool Actualizar(int id, SolicitudAdopcion entidad);
    bool Eliminar(int id);
}
