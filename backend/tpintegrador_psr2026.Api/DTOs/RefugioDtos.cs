using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class RefugioRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }
}

public record RefugioResponseDto(int Id, string Nombre, int CapacidadMaxima)
{
    public static RefugioResponseDto Desde(Refugio r) => new(r.Id, r.Nombre, r.CapacidadMaxima);
}

public record CapacidadRefugioDto(int CapacidadMaxima, int MascotasActuales, int PlazasDisponibles, bool TieneLugarDisponible);
