using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class TratamientoRequestDto
{
    public TipoTratamiento Tipo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    /// <summary>Estado inicial (por defecto Pendiente). Permite registrar, p. ej., una vacuna ya aplicada como Finalizado.</summary>
    public EstadoTratamiento Estado { get; set; } = EstadoTratamiento.Pendiente;

    public Tratamiento ADominio() => new(Tipo, Descripcion, FechaInicio, FechaFin, Estado);
}

public record TratamientoResponseDto(
    int Id,
    int HistorialSanitarioId,
    int MascotaId,
    TipoTratamiento Tipo,
    string? Descripcion,
    DateTime FechaInicio,
    DateTime? FechaFin,
    EstadoTratamiento Estado)
{
    public static TratamientoResponseDto Desde(Tratamiento t, int mascotaId) =>
        new(t.Id, t.HistorialSanitarioId, mascotaId, t.Tipo, t.Descripcion, t.FechaInicio, t.FechaFin, t.Estado);
}
