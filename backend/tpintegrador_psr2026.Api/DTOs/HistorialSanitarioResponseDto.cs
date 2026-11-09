namespace tpintegrador_psr2026.Api.DTOs;

public record HistorialSanitarioResponseDto(
    int Id,
    int MascotaId,
    string? Observaciones
);