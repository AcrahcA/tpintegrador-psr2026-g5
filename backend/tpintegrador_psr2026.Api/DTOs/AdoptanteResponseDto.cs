namespace tpintegrador_psr2026.Api.DTOs;

public record AdoptanteResponseDto(
    int Id,
    string Nombre,
    string Dni,
    string? Telefono,
    string? Direccion
);