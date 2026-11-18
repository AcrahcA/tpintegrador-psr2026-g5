namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

/// <summary>Operaciones comunes a todos los repositorios.</summary>
public interface IRepositorio<T> where T : class, IEntidad
{
    List<T> Get();
    T? GetPorId(int id);
    T Post(T entidad);
    bool Put(int id, T entidad);
    bool Delete(int id);
}
