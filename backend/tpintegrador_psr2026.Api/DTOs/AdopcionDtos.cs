using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class AdopcionRequestDto
{
    public int SolicitudAdopcionId { get; set; }
    public DateTime? FechaAdopcion { get; set; }
    public string? Observaciones { get; set; }
}

public record AdopcionResponseDto(
    int Id,
    int SolicitudAdopcionId,
    int MascotaId,
    int AdoptanteId,
    DateTime FechaAdopcion,
    string? Observaciones)
{
    public static AdopcionResponseDto Desde(Adopcion a) =>
        new(a.Id, a.SolicitudAdopcionId, a.MascotaId, a.AdoptanteId, a.FechaAdopcion, a.Observaciones);
}
