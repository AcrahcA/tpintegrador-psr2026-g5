namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ISolicitudAdopcionRepository
{
    List<SolicitudAdopcion> Get();
    SolicitudAdopcion? GetPorId(int id);
    SolicitudAdopcion Post(SolicitudAdopcion entidad);
    bool Put(int id, SolicitudAdopcion entidad);
    bool Delete(int id);
}