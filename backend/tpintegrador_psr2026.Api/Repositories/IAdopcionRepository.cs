namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

/// <summary>Las adopciones son definitivas: no se exponen operaciones de modificación ni de borrado.</summary>
public interface IAdopcionRepository
{
    List<Adopcion> Get();
    Adopcion? GetPorId(int id);
    Adopcion Post(Adopcion adopcion);
    Adopcion? GetPorMascota(int mascotaId);
    List<Adopcion> GetPorAdoptante(int adoptanteId);
    bool ExisteAdopcionDeMascota(int mascotaId);
    bool ExisteAdopcionDeSolicitud(int solicitudId);
}
