namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAdoptanteRepository : IRepositorio<Adoptante>
{
    Adoptante? GetPorDni(string dni);
    Adoptante? GetPorUsuario(int usuarioId);
}
