namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IFotoMascotaRepository : IRepositorio<FotoMascota>
{
    /// <summary>Fotos de la mascota ordenadas (la primera es la principal).</summary>
    List<FotoMascota> GetPorMascota(int mascotaId);
    /// <summary>Fotos de varias mascotas a la vez, agrupadas por mascota (evita una consulta por mascota).</summary>
    Dictionary<int, List<FotoMascota>> GetPorMascotas(IEnumerable<int> mascotaIds);
    void GuardarCambios();
}
