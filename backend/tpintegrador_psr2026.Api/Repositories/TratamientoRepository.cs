using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class TratamientoRepository : Repositorio<Tratamiento>, ITratamientoRepository
{
    public TratamientoRepository(RefugioDbContext context) : base(context) { }

    public List<Tratamiento> GetPorMascota(int mascotaId) =>
        Set.Where(t => t.HistorialSanitario!.MascotaId == mascotaId)
           .OrderBy(t => t.FechaInicio).ThenBy(t => t.Id).ToList();

    public List<Tratamiento> GetPorHistorial(int historialSanitarioId) =>
        Set.Where(t => t.HistorialSanitarioId == historialSanitarioId).OrderBy(t => t.FechaInicio).ToList();

    public bool TieneTratamientosActivos(int mascotaId) =>
        Set.Any(t => t.HistorialSanitario!.MascotaId == mascotaId &&
                     (t.Estado == EstadoTratamiento.Pendiente || t.Estado == EstadoTratamiento.EnCurso));
}
