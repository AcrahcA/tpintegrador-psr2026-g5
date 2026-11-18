using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

/// <summary>Implementación base con Entity Framework Core.</summary>
public abstract class Repositorio<T> : IRepositorio<T> where T : class, IEntidad
{
    protected readonly RefugioDbContext Context;
    protected DbSet<T> Set => Context.Set<T>();

    protected Repositorio(RefugioDbContext context) => Context = context;

    public virtual List<T> Get() => Set.OrderBy(e => e.Id).ToList();

    public virtual T? GetPorId(int id) => Set.Find(id);

    public virtual T Post(T entidad)
    {
        Set.Add(entidad);
        Context.SaveChanges();
        return entidad;
    }

    public virtual bool Put(int id, T entidad)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        entidad.Id = id;
        if (!ReferenceEquals(existente, entidad))
            Context.Entry(existente).CurrentValues.SetValues(entidad);
        Context.SaveChanges();
        return true;
    }

    public virtual bool Delete(int id)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        Set.Remove(existente);
        Context.SaveChanges();
        return true;
    }
}
