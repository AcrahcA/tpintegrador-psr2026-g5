namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ITratamientoService
{
   List<Tratamiento> Get();
    List<Tratamiento> GetByMascotaId(int mascotaId);
    Tratamiento? GetById(int id);
    Tratamiento? Post(int mascotaId, Tratamiento tratamiento);
    bool PutEstado(int id, EstadoTratamiento nuevoEstado);
    bool Delete(int id);
}
