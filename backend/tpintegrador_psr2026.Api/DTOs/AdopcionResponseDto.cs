namespace tpintegrador_psr2026.Api.DTOs;

public record AdopcionResponseDto(
    int Id,
    int SolicitudAdopcionId,
    DateTime FechaAdopcion,
    string? Observaciones
);