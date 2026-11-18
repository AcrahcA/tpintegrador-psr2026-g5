using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class HistorialSanitarioRequestDto
{
    public int MascotaId { get; set; }
    public string? Observaciones { get; set; }
}

public record HistorialSanitarioResponseDto(int Id, int MascotaId, string? Observaciones)
{
    public static HistorialSanitarioResponseDto Desde(HistorialSanitario h) => new(h.Id, h.MascotaId, h.Observaciones);
}

/// <summary>Historial completo: atenciones recibidas y si cumple las condiciones sanitarias para adopción.</summary>
public record HistorialSanitarioDetalleDto(
    int Id,
    int MascotaId,
    string? Observaciones,
    bool CumpleCondicionesSanitarias,
    List<string> Motivos,
    List<TratamientoResponseDto> Tratamientos);
