namespace tpintegrador_psr2026.Api.Domain;

public class Adopcion
{
    public int Id { get; set; }
    
    // Nace a partir de una SolicitudAdopcion aprobada
    public int SolicitudAdopcionId { get; set; }
    public SolicitudAdopcion? SolicitudAdopcion { get; set; }

    public DateTime FechaAdopcion { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }
}