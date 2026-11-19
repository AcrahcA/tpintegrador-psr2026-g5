namespace tpintegrador_psr2026.Api.Domain;

public class AvisoRescate : IEntidad
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public TipoAnimal TipoAnimal { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaAviso { get; set; }
    public string ContactoNombre { get; set; } = string.Empty;
    public string? ContactoTelefono { get; set; }
    public EstadoAparente EstadoAparente { get; set; }
    public NivelUrgencia NivelUrgencia { get; set; }
    public EstadoAviso Estado { get; set; } = EstadoAviso.Reportado;

    // Mascota generada a partir de este aviso (0..1), una vez atendido el rescate
    public int? MascotaGeneradaId { get; set; }

    protected AvisoRescate() { }

    public AvisoRescate(
        string descripcion,
        TipoAnimal tipoAnimal,
        double latitud,
        double longitud,
        string contactoNombre,
        EstadoAparente estadoAparente,
        NivelUrgencia nivelUrgencia,
        string? contactoTelefono = null,
        string? direccion = null)
    {
        Descripcion = descripcion;
        TipoAnimal = tipoAnimal;
        Latitud = latitud;
        Longitud = longitud;
        ContactoNombre = contactoNombre;
        EstadoAparente = estadoAparente;
        NivelUrgencia = nivelUrgencia;
        ContactoTelefono = contactoTelefono;
        Direccion = direccion;
        FechaAviso = DateTime.UtcNow;
        Estado = EstadoAviso.Reportado;
    }

    /// <summary>Un aviso está pendiente mientras todavía no se resolvió (reportado o en evaluación).</summary>
    public bool EstaPendiente => Estado is EstadoAviso.Reportado or EstadoAviso.EnEvaluacion;

    public bool EstaCerrado => Estado is EstadoAviso.Atendido or EstadoAviso.Descartado;

    /// <summary>
    /// Transiciones válidas. "Atendido" sólo se alcanza al generar el ingreso de la mascota.
    /// </summary>
    public bool PuedeCambiarA(EstadoAviso nuevo) => (Estado, nuevo) switch
    {
        (EstadoAviso.Reportado, EstadoAviso.EnEvaluacion) => true,
        (EstadoAviso.Reportado, EstadoAviso.Aceptado) => true,
        (EstadoAviso.Reportado, EstadoAviso.Descartado) => true,
        (EstadoAviso.EnEvaluacion, EstadoAviso.Aceptado) => true,
        (EstadoAviso.EnEvaluacion, EstadoAviso.Descartado) => true,
        (EstadoAviso.Aceptado, EstadoAviso.Descartado) => true,
        _ => false
    };

    public static void ValidarCoordenadas(double latitud, double longitud)
    {
        if (double.IsNaN(latitud) || latitud < -90 || latitud > 90 ||
            double.IsNaN(longitud) || longitud < -180 || longitud > 180)
            throw new ReglaNegocioException("Las coordenadas del aviso no son válidas (latitud -90..90, longitud -180..180).");
    }
}
