namespace tpintegrador_psr2026.Api.DTOs;

public record CuidadorResponseDto(
    int Id,
    string Nombre,
    string? Especializacion,
    int CapacidadMaxima
);