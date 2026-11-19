using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class HistorialSanitarioRepository : Repositorio<HistorialSanitario>, IHistorialSanitarioRepository
{
    public HistorialSanitarioRepository(RefugioDbContext context) : base(context) { }

    public HistorialSanitario? GetPorMascota(int mascotaId) =>
        Set.Include(h => h.Tratamientos).FirstOrDefault(h => h.MascotaId == mascotaId);
}
