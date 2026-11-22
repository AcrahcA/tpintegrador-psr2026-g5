namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IUsuarioRepository : IRepositorio<Usuario>
{
    Usuario? GetPorEmail(string email);
}
