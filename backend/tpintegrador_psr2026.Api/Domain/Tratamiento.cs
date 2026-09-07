namespace tpintegrador_psr2026.Api.Domain;

public class Tratamiento
{
    public int Id { get; set; }
    public int HistorialSanitarioId { get; set; }
    public TipoTratamiento Tipo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime? FechaFin { get; set; }
    public EstadoTratamiento Estado { get; set; } = EstadoTratamiento.Pendiente;
}
