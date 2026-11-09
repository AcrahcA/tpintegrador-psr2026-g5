using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public record AvisoRescateResponseDto(
    int Id,
    string Descripcion,
    TipoAnimal TipoAnimal,
    double Latitud,
    double Longitud,
    DateTime FechaAviso,
    string ContactoNombre,
    string? ContactoTelefono,
    EstadoAparente EstadoAparente,
    NivelUrgencia NivelUrgencia,
    EstadoAviso Estado,
    int? MascotaGeneradaId
);