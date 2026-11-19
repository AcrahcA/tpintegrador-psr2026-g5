namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IHistorialSanitarioRepository : IRepositorio<HistorialSanitario>
{
    /// <summary>Historial de la mascota, con sus tratamientos (también para mascotas adoptadas).</summary>
    HistorialSanitario? GetPorMascota(int mascotaId);
}
