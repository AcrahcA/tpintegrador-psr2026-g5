using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class MascotaRepository : Repositorio<Mascota>, IMascotaRepository
{
    public MascotaRepository(RefugioDbContext context) : base(context) { }

    public List<Mascota> GetPorEstado(EstadoMascota estado) =>
        Set.Where(m => m.Estado == estado).OrderBy(m => m.Id).ToList();

    public List<Mascota> GetAlojadas() =>
        Set.Where(m => m.Estado != EstadoMascota.Adoptada).OrderBy(m => m.Id).ToList();

    public int ContarAlojadas() => Set.Count(m => m.Estado != EstadoMascota.Adoptada);

    public List<Mascota> GetPorCuidador(int cuidadorId, bool incluirAdoptadas = false) =>
        Set.Where(m => m.CuidadorId == cuidadorId && (incluirAdoptadas || m.Estado != EstadoMascota.Adoptada))
           .OrderBy(m => m.Id).ToList();

    public int ContarAsignadasACuidador(int cuidadorId) =>
        Set.Count(m => m.CuidadorId == cuidadorId && m.Estado != EstadoMascota.Adoptada);
}
