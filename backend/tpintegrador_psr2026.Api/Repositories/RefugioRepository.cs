using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class RefugioRepository : IRefugioRepository
{
    private readonly RefugioDbContext _context;

    public RefugioRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<Refugio> Get() 
    {
        return _context.Refugios.ToList();
    }

    public Refugio? GetPorId(int id) 
    {
        return _context.Refugios.Find(id);
    }

    public Refugio Post(Refugio entidad)
    {
        _context.Refugios.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, Refugio entidad)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        entidad.Id = id;
        _context.Entry(existente).CurrentValues.SetValues(entidad);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        _context.Refugios.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}