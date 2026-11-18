namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ICuidadorRepository : IRepositorio<Cuidador>
{
    List<Cuidador> GetDisponibles();
}
