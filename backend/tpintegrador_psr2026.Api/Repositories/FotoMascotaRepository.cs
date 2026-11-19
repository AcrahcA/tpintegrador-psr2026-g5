using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class FotoMascotaRepository : Repositorio<FotoMascota>, IFotoMascotaRepository
{
    public FotoMascotaRepository(RefugioDbContext context) : base(context) { }

    public List<FotoMascota> GetPorMascota(int mascotaId) =>
        Set.Where(f => f.MascotaId == mascotaId).OrderBy(f => f.Orden).ThenBy(f => f.Id).ToList();

    public Dictionary<int, List<FotoMascota>> GetPorMascotas(IEnumerable<int> mascotaIds)
    {
        var ids = mascotaIds.Distinct().ToList();
        return Set.Where(f => ids.Contains(f.MascotaId))
                  .OrderBy(f => f.Orden).ThenBy(f => f.Id)
                  .AsEnumerable()
                  .GroupBy(f => f.MascotaId)
                  .ToDictionary(g => g.Key, g => g.ToList());
    }

    public void GuardarCambios() => Context.SaveChanges();
}
