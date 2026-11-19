namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IMascotaRepository : IRepositorio<Mascota>
{
    List<Mascota> GetPorEstado(EstadoMascota estado);
    /// <summary>Mascotas que ocupan plaza (todas menos las adoptadas).</summary>
    List<Mascota> GetAlojadas();
    int ContarAlojadas();
    /// <summary>Mascotas asignadas a un cuidador (por defecto sólo las que siguen alojadas).</summary>
    List<Mascota> GetPorCuidador(int cuidadorId, bool incluirAdoptadas = false);
    int ContarAsignadasACuidador(int cuidadorId);
}
