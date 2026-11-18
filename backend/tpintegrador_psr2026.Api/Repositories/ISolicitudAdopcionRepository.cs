namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface ISolicitudAdopcionRepository : IRepositorio<SolicitudAdopcion>
{
    List<SolicitudAdopcion> GetPorEstado(params EstadoSolicitud[] estados);
    List<SolicitudAdopcion> GetPorMascota(int mascotaId);
    List<SolicitudAdopcion> GetPorAdoptante(int adoptanteId);
}
