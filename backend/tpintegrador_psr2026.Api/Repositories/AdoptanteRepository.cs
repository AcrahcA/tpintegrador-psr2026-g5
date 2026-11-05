using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AdoptanteRepository : IAdoptanteRepository
{
    private readonly RefugioDbContext _context;

    public AdoptanteRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<Adoptante> Get() 
    {
        return _context.Adoptantes.ToList();
    }

    public Adoptante? GetPorId(int id) 
    {
        return _context.Adoptantes.Find(id);
    }

    public Adoptante Post(Adoptante entidad)
    {
        _context.Adoptantes.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, Adoptante entidad)
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

        _context.Adoptantes.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}