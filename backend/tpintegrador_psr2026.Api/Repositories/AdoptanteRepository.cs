using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AdoptanteRepository : Repositorio<Adoptante>, IAdoptanteRepository
{
    public AdoptanteRepository(RefugioDbContext context) : base(context) { }

    public Adoptante? GetPorDni(string dni) => Set.FirstOrDefault(a => a.Dni == dni);

    public Adoptante? GetPorUsuario(int usuarioId) => Set.FirstOrDefault(a => a.UsuarioId == usuarioId);
}
