using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AvisoRescateRepository : IAvisoRescateRepository
{
    private readonly RefugioDbContext _context;

    public AvisoRescateRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<AvisoRescate> Get() 
    {
        return _context.AvisosRescate.ToList();
    }

    public AvisoRescate? GetPorId(int id) 
    {
        return _context.AvisosRescate.Find(id);
    }

    public AvisoRescate Post(AvisoRescate entidad)
    {
        _context.AvisosRescate.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, AvisoRescate entidad)
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

        _context.AvisosRescate.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}