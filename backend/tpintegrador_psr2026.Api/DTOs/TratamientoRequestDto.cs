using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class TratamientoRequestDto
{
    public int HistorialSanitarioId { get; set; }
    public TipoTratamiento Tipo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public EstadoTratamiento Estado { get; set; } = EstadoTratamiento.Pendiente;
}