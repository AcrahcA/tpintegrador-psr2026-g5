namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ISolicitudAdopcionService
{
    List<SolicitudAdopcion> ObtenerSolicitudes();
    List<SolicitudAdopcion> ObtenerPendientes(); // <-- Método agregado
    SolicitudAdopcion? BuscarSolicitud(int id);
    SolicitudAdopcion? RealizarSolicitud(int adoptanteId, int mascotaId);
    bool CambiarEstado(int id, EstadoSolicitud nuevoEstado);
    bool EliminarSolicitud(int id);
}