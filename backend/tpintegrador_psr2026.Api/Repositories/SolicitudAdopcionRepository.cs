using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class SolicitudAdopcionRepository : ISolicitudAdopcionRepository
{
    private readonly RefugioDbContext _context;

    public SolicitudAdopcionRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<SolicitudAdopcion> Get() 
    {
        return _context.SolicitudesAdopcion.ToList();
    }

    public SolicitudAdopcion? GetPorId(int id) 
    {
        return _context.SolicitudesAdopcion.Find(id);
    }

    public SolicitudAdopcion Post(SolicitudAdopcion entidad)
    {
        _context.SolicitudesAdopcion.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, SolicitudAdopcion entidad)
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

        _context.SolicitudesAdopcion.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}