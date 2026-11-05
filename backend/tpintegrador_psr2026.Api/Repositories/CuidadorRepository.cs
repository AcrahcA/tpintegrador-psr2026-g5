using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class CuidadorRepository : ICuidadorRepository
{
    private readonly RefugioDbContext _context;

    public CuidadorRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<Cuidador> Get() 
    {
        return _context.Cuidadores.ToList();
    }

    public Cuidador? GetPorId(int id) 
    {
        return _context.Cuidadores.Find(id);
    }

    public Cuidador Post(Cuidador entidad)
    {
        _context.Cuidadores.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, Cuidador entidad)
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

        _context.Cuidadores.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}