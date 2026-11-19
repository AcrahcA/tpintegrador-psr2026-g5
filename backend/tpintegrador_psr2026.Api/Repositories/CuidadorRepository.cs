using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class CuidadorRepository : Repositorio<Cuidador>, ICuidadorRepository
{
    public CuidadorRepository(RefugioDbContext context) : base(context) { }

    public List<Cuidador> GetDisponibles() => Set.Where(c => c.Disponible).OrderBy(c => c.Id).ToList();
}
