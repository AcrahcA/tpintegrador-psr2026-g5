namespace tpintegrador_psr2026.Api.DTOs;

public class AdopcionRequestDto
{
    public int SolicitudAdopcionId { get; set; }
    public DateTime FechaAdopcion { get; set; }
    public string? Observaciones { get; set; }
}