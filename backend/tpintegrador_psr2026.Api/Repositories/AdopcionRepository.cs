using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AdopcionRepository : IAdopcionRepository
{
    private readonly RefugioDbContext _context;

    public AdopcionRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<Adopcion> Get() 
    {
        return _context.Adopciones.ToList();
    }

    public Adopcion? GetPorId(int id) 
    {
        return _context.Adopciones.Find(id);
    }

    public Adopcion Post(Adopcion entidad)
    {
        _context.Adopciones.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, Adopcion entidad)
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

        _context.Adopciones.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}