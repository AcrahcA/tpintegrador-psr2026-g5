namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ITratamientoRepository : IRepositorio<Tratamiento>
{
    List<Tratamiento> GetPorMascota(int mascotaId);
    List<Tratamiento> GetPorHistorial(int historialSanitarioId);
    bool TieneTratamientosActivos(int mascotaId);
}
