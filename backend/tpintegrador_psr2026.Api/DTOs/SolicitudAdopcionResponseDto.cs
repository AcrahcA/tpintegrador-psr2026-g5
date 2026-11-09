using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public record SolicitudAdopcionResponseDto(
    int Id,
    int AdoptanteId,
    int MascotaId,
    DateTime FechaSolicitud,
    EstadoSolicitud Estado
);