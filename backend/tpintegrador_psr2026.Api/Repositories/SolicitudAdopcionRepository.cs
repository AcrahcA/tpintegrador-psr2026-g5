using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class SolicitudAdopcionRepository : Repositorio<SolicitudAdopcion>, ISolicitudAdopcionRepository
{
    public SolicitudAdopcionRepository(RefugioDbContext context) : base(context) { }

    public List<SolicitudAdopcion> GetPorEstado(params EstadoSolicitud[] estados) =>
        Set.Where(s => estados.Contains(s.Estado)).OrderBy(s => s.FechaSolicitud).ToList();

    public List<SolicitudAdopcion> GetPorMascota(int mascotaId) =>
        Set.Where(s => s.MascotaId == mascotaId).OrderBy(s => s.FechaSolicitud).ToList();

    public List<SolicitudAdopcion> GetPorAdoptante(int adoptanteId) =>
        Set.Where(s => s.AdoptanteId == adoptanteId).OrderBy(s => s.FechaSolicitud).ToList();
}
