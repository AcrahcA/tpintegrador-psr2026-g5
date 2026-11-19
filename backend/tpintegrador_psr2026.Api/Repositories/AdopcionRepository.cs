using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class AdopcionRepository : IAdopcionRepository
{
    private readonly RefugioDbContext _context;

    public AdopcionRepository(RefugioDbContext context) => _context = context;

    public List<Adopcion> Get() => _context.Adopciones.OrderBy(a => a.FechaAdopcion).ToList();

    public Adopcion? GetPorId(int id) => _context.Adopciones.Find(id);

    public Adopcion Post(Adopcion adopcion)
    {
        _context.Adopciones.Add(adopcion);
        _context.SaveChanges();
        return adopcion;
    }

    public Adopcion? GetPorMascota(int mascotaId) => _context.Adopciones.FirstOrDefault(a => a.MascotaId == mascotaId);

    public List<Adopcion> GetPorAdoptante(int adoptanteId) =>
        _context.Adopciones.Where(a => a.AdoptanteId == adoptanteId).OrderBy(a => a.FechaAdopcion).ToList();

    public bool ExisteAdopcionDeMascota(int mascotaId) => _context.Adopciones.Any(a => a.MascotaId == mascotaId);

    public bool ExisteAdopcionDeSolicitud(int solicitudId) => _context.Adopciones.Any(a => a.SolicitudAdopcionId == solicitudId);
}
