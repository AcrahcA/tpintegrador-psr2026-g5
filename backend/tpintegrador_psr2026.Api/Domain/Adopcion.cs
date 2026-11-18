namespace tpintegrador_psr2026.Api.Domain;

/// <summary>Adopción definitiva. Es permanente: no se modifica ni se elimina.</summary>
public class Adopcion : IEntidad
{
    public int Id { get; set; }

    public int SolicitudAdopcionId { get; set; }
    public SolicitudAdopcion? SolicitudAdopcion { get; set; }

    // Se registran explícitamente la mascota y su adoptante (la base impide dos adopciones de la misma mascota)
    public int MascotaId { get; set; }
    public int AdoptanteId { get; set; }

    public DateTime FechaAdopcion { get; set; }
    public string? Observaciones { get; set; }

    protected Adopcion() { }

    public Adopcion(SolicitudAdopcion solicitud, string? observaciones = null, DateTime? fechaAdopcion = null)
    {
        SolicitudAdopcionId = solicitud.Id;
        MascotaId = solicitud.MascotaId;
        AdoptanteId = solicitud.AdoptanteId;
        Observaciones = observaciones;
        FechaAdopcion = fechaAdopcion is { } f && f != default ? Fechas.AUtc(f) : DateTime.UtcNow;
    }
}
