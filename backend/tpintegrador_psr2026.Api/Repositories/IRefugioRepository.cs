namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IRefugioRepository : IRepositorio<Refugio>
{
    Refugio? GetActual();
}
