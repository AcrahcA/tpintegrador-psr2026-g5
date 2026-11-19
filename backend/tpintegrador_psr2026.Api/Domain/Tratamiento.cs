namespace tpintegrador_psr2026.Api.Domain;

public class Tratamiento : IEntidad
{
    public int Id { get; set; }
    public int HistorialSanitarioId { get; set; }
    public HistorialSanitario? HistorialSanitario { get; set; }
    public TipoTratamiento Tipo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public EstadoTratamiento Estado { get; set; } = EstadoTratamiento.Pendiente;

    protected Tratamiento() { }

    public Tratamiento(
        TipoTratamiento tipo,
        string? descripcion = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        EstadoTratamiento estado = EstadoTratamiento.Pendiente)
    {
        Tipo = tipo;
        Descripcion = descripcion;
        FechaInicio = fechaInicio is { } f && f != default ? Fechas.AUtc(f) : DateTime.UtcNow;
        FechaFin = fechaFin is { } fin ? Fechas.AUtc(fin) : null;
        Estado = estado;
    }

    /// <summary>Un tratamiento pendiente o en curso impide que la mascota esté disponible para adopción.</summary>
    public bool EstaActivo => Estado is EstadoTratamiento.Pendiente or EstadoTratamiento.EnCurso;

    public bool PuedeCambiarA(EstadoTratamiento nuevo) => (Estado, nuevo) switch
    {
        (EstadoTratamiento.Pendiente, EstadoTratamiento.EnCurso) => true,
        (EstadoTratamiento.Pendiente, EstadoTratamiento.Finalizado) => true,
        (EstadoTratamiento.Pendiente, EstadoTratamiento.Suspendido) => true,
        (EstadoTratamiento.EnCurso, EstadoTratamiento.Finalizado) => true,
        (EstadoTratamiento.EnCurso, EstadoTratamiento.Suspendido) => true,
        (EstadoTratamiento.Suspendido, EstadoTratamiento.EnCurso) => true,
        (EstadoTratamiento.Suspendido, EstadoTratamiento.Pendiente) => true,
        _ => false
    };

    public void CambiarEstado(EstadoTratamiento nuevo)
    {
        if (!PuedeCambiarA(nuevo))
            throw new ReglaNegocioException($"Un tratamiento en estado {Estado} no puede pasar a {nuevo}.");
        Estado = nuevo;
        FechaFin = nuevo is EstadoTratamiento.Finalizado or EstadoTratamiento.Suspendido ? DateTime.UtcNow : null;
    }

    public void Validar()
    {
        if (FechaFin is { } fin && fin < FechaInicio)
            throw new ReglaNegocioException("La fecha de finalización no puede ser anterior a la fecha de inicio.");
    }
}
