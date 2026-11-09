using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class SolicitudAdopcionRequestDto
{
    public int AdoptanteId { get; set; }
    public int MascotaId { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
}