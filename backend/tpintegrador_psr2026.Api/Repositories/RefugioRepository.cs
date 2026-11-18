using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class RefugioRepository : Repositorio<Refugio>, IRefugioRepository
{
    public RefugioRepository(RefugioDbContext context) : base(context) { }

    public Refugio? GetActual() => Set.OrderBy(r => r.Id).FirstOrDefault();
}
