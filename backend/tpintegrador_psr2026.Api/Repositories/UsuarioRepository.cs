using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class UsuarioRepository : Repositorio<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(RefugioDbContext context) : base(context) { }

    public Usuario? GetPorEmail(string email)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        return Set.FirstOrDefault(u => u.Email == normalizado);
    }
}
