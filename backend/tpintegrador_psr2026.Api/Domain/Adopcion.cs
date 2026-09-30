namespace tpintegrador_psr2026.Api.Domain;

public class Adopcion
{
    public int Id { get; set; }
    
    // Nace a partir de una SolicitudAdopcion aprobada
    public int SolicitudAdopcionId { get; set; }
    public SolicitudAdopcion? SolicitudAdopcion { get; set; }

    public DateTime FechaAdopcion { get; set; }
    public string? Observaciones { get; set; }

    public Adopcion(int solicitudAdopcionId, string? observaciones = null, DateTime fechaAdopcion = default)
    {
        this.SolicitudAdopcionId = solicitudAdopcionId;
        this.Observaciones = observaciones;
        this.FechaAdopcion = fechaAdopcion != default ? fechaAdopcion : DateTime.Now;
    }
}