using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class SolicitudAdopcionRequestDto
{
    public int AdoptanteId { get; set; }
    public int MascotaId { get; set; }
}

/// <summary>Solicitud enviada desde el sitio público: registra (o reutiliza por DNI) al adoptante.</summary>
public class SolicitudPublicaRequestDto : AdoptanteRequestDto
{
    public int MascotaId { get; set; }
}

public record SolicitudAdopcionResponseDto(int Id, int AdoptanteId, int MascotaId, DateTime FechaSolicitud, EstadoSolicitud Estado)
{
    public static SolicitudAdopcionResponseDto Desde(SolicitudAdopcion s) =>
        new(s.Id, s.AdoptanteId, s.MascotaId, s.FechaSolicitud, s.Estado);
}
