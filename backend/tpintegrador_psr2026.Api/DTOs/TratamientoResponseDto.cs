using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public record TratamientoResponseDto(
    int Id,
    int HistorialSanitarioId,
    TipoTratamiento Tipo,
    string? Descripcion,
    DateTime FechaInicio,
    DateTime? FechaFin,
    EstadoTratamiento Estado
);