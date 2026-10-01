namespace tpintegrador_psr2026.Api.Domain;

public class SolicitudAdopcion
{
    public int Id { get; set; }
    
    public int AdoptanteId { get; set; }
    public Adoptante? Adoptante { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public DateTime FechaSolicitud { get; set; }
    public EstadoSolicitud Estado { get; set; }

    // Relación opcional con la Adopción concretada (0..1)
    public Adopcion? Adopcion { get; set; }

    // CONSTRUCTOR (kas iti estruktura ti Adopcion.cs ken AvisoRescate.cs)
    public SolicitudAdopcion(
        int adoptanteId,
        int mascotaId,
        DateTime fechaSolicitud = default,
        EstadoSolicitud estado = EstadoSolicitud.Pendiente,
        Adopcion? adopcion = null)
    {
        this.AdoptanteId = adoptanteId;
        this.MascotaId = mascotaId;
        this.FechaSolicitud = fechaSolicitud != default ? fechaSolicitud : DateTime.Now;
        this.Estado = estado;
        this.Adopcion = adopcion;
    }
}