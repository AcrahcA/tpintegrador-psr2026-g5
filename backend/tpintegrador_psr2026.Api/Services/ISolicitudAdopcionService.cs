namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ISolicitudAdopcionService
{
    List<SolicitudAdopcion> Get();
    List<SolicitudAdopcion> GetPendientes();
    SolicitudAdopcion? GetById(int id);
    SolicitudAdopcion? Post(int adoptanteId, int mascotaId);
    bool PutEstado(int id, EstadoSolicitud nuevoEstado);
    bool Delete(int id);
}