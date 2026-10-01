namespace tpintegrador_psr2026.Api.Domain;

public class AvisoRescate
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public TipoAnimal TipoAnimal { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public DateTime FechaAviso { get; set; }
    public string ContactoNombre { get; set; } = string.Empty;
    public string? ContactoTelefono { get; set; }
    public EstadoAparente EstadoAparente { get; set; }
    public NivelUrgencia NivelUrgencia { get; set; }
    public EstadoAviso Estado { get; set; }

    // Id de la mascota generada a partir de este aviso (0..1), una vez atendido el rescate
    public int? MascotaGeneradaId { get; set; }

    // CONSTRUCTOR (igual a la estructura de Adopcion.cs)
    public AvisoRescate(
        string descripcion,
        TipoAnimal tipoAnimal,
        double latitud,
        double longitud,
        string contactoNombre,
        EstadoAparente estadoAparente,
        NivelUrgencia nivelUrgencia,
        string? contactoTelefono = null,
        DateTime fechaAviso = default,
        EstadoAviso estado = EstadoAviso.Reportado,
        int? mascotaGeneradaId = null)
    {
        this.Descripcion = descripcion;
        this.TipoAnimal = tipoAnimal;
        this.Latitud = latitud;
        this.Longitud = longitud;
        this.ContactoNombre = contactoNombre;
        this.EstadoAparente = estadoAparente;
        this.NivelUrgencia = nivelUrgencia;
        this.ContactoTelefono = contactoTelefono;
        this.FechaAviso = fechaAviso != default ? fechaAviso : DateTime.Now;
        this.Estado = estado;
        this.MascotaGeneradaId = mascotaGeneradaId;
    }
}