namespace tpintegrador_psr2026.Api.Domain;

public class SolicitudAdopcion
{
    public int Id { get; set; }
    
    public int AdoptanteId { get; set; }
    public Adoptante? Adoptante { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    // Relación opcional con la Adopción concretada (0..1)
    public Adopcion? Adopcion { get; set; }
}