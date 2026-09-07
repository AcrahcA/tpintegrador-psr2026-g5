namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAvisoRescateRepository
{
    List<AvisoRescate> ObtenerTodos();
    AvisoRescate? ObtenerPorId(int id);
    AvisoRescate Agregar(AvisoRescate entidad);
    bool Actualizar(int id, AvisoRescate entidad);
    bool Eliminar(int id);
}
