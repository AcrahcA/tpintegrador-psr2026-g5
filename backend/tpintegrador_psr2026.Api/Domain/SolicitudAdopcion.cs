namespace tpintegrador_psr2026.Api.Domain;

public class SolicitudAdopcion : IEntidad
{
    public int Id { get; set; }

    public int AdoptanteId { get; set; }
    public Adoptante? Adoptante { get; set; }

    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public DateTime FechaSolicitud { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    public Adopcion? Adopcion { get; set; }

    protected SolicitudAdopcion() { }

    public SolicitudAdopcion(int adoptanteId, int mascotaId)
    {
        AdoptanteId = adoptanteId;
        MascotaId = mascotaId;
        FechaSolicitud = DateTime.UtcNow;
        Estado = EstadoSolicitud.Pendiente;
    }

    /// <summary>Pendiente o en evaluación: todavía no fue resuelta.</summary>
    public bool EstaPendiente => Estado is EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion;

    /// <summary>Solicitud vigente (no rechazada ni cancelada).</summary>
    public bool EstaVigente => EstaPendiente || Estado == EstadoSolicitud.Aprobada;

    public bool PuedeCambiarA(EstadoSolicitud nuevo) => (Estado, nuevo) switch
    {
        (EstadoSolicitud.Pendiente, EstadoSolicitud.EnEvaluacion) => true,
        (EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion, EstadoSolicitud.Aprobada) => true,
        (EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion, EstadoSolicitud.Rechazada) => true,
        (EstadoSolicitud.Pendiente or EstadoSolicitud.EnEvaluacion or EstadoSolicitud.Aprobada, EstadoSolicitud.Cancelada) => true,
        _ => false
    };
}
