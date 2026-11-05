using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class HistorialSanitarioRepository : IHistorialSanitarioRepository
{
    private readonly RefugioDbContext _context;

    public HistorialSanitarioRepository(RefugioDbContext context)
    {
        _context = context;
    }

    public List<HistorialSanitario> Get() 
    {
        return _context.HistorialesSanitarios.ToList();
    }

    public HistorialSanitario? GetPorId(int id) 
    {
        return _context.HistorialesSanitarios.Find(id);
    }

    public HistorialSanitario Post(HistorialSanitario entidad)
    {
        _context.HistorialesSanitarios.Add(entidad);
        _context.SaveChanges();
        return entidad;
    }

    public bool Put(int id, HistorialSanitario entidad)
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

        _context.HistorialesSanitarios.Remove(existente);
        _context.SaveChanges();
        return true;
    }
}