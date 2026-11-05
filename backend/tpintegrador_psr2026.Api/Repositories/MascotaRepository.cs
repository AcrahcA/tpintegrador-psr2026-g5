using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class MascotaRepository : IMascotaRepository
{
    private readonly RefugioDbContext _context;

    public MascotaRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<Mascota> Get() 
    {
        return _context.Mascotas.ToList();
    }

    public Mascota? GetPorId(int id) 
    {
        return _context.Mascotas.Find(id);
    }

    public Mascota Post(Mascota entidad)
    {
        _context.Mascotas.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, Mascota entidad)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        entidad.Id = id; // Aseguramos que mantenga el mismo ID
        _context.Entry(existente).CurrentValues.SetValues(entidad);
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        _context.Mascotas.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}