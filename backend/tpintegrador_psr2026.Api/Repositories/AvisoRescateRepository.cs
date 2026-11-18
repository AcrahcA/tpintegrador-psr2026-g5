using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AvisoRescateRepository : Repositorio<AvisoRescate>, IAvisoRescateRepository
{
    public AvisoRescateRepository(RefugioDbContext context) : base(context) { }

    public List<AvisoRescate> GetPorEstado(EstadoAviso estado) =>
        Set.Where(a => a.Estado == estado).OrderBy(a => a.FechaAviso).ToList();

    public List<AvisoRescate> GetPendientes() =>
        Set.Where(a => a.Estado == EstadoAviso.Reportado || a.Estado == EstadoAviso.EnEvaluacion)
           .OrderByDescending(a => a.NivelUrgencia).ThenBy(a => a.FechaAviso).ToList();
}
