using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class CuidadorRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Tipos de animal que atiende, p. ej. "Perros", "Gatos y aves". Vacío = generalista.</summary>
    public string? Especializacion { get; set; }
    public int CapacidadMaxima { get; set; }
    /// <summary>Si se omite: true al registrar y sin cambios al actualizar.</summary>
    public bool? Disponible { get; set; }
}

public record CuidadorResponseDto(
    int Id,
    string Nombre,
    string? Especializacion,
    int CapacidadMaxima,
    bool Disponible,
    int MascotasAsignadas)
{
    public static CuidadorResponseDto Desde(Cuidador c, int asignadas) =>
        new(c.Id, c.Nombre, c.Especializacion, c.CapacidadMaxima, c.Disponible, asignadas);
}
