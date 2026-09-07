namespace tpintegrador_psr2026.Api.Domain;

public class SolicitudAdopcion
{
    public int Id { get; set; }
    public int AdoptanteId { get; set; }
    public int MascotaId { get; set; }
    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
}
