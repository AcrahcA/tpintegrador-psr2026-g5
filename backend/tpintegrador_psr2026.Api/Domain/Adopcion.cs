namespace tpintegrador_psr2026.Api.Domain;

public class Adopcion
{
    public int Id { get; set; }
    public int SolicitudAdopcionId { get; set; }
    public DateTime FechaAdopcion { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }
}
