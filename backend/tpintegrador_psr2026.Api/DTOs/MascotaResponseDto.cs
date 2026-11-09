using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public record MascotaResponseDto(
    int Id,
    string Nombre,
    string Especie,
    string? Raza,
    int EdadAproximada,
    Sexo Sexo,
    Tamaño Tamaño,
    DateTime FechaIngreso,
    EstadoMascota Estado
);