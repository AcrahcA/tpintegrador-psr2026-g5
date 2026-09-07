namespace tpintegrador_psr2026.Api.Domain;

public class AvisoRescate
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public TipoAnimal TipoAnimal { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public DateTime FechaAviso { get; set; } = DateTime.Now;
    public string ContactoNombre { get; set; } = string.Empty;
    public string? ContactoTelefono { get; set; }
    public EstadoAparente EstadoAparente { get; set; }
    public NivelUrgencia NivelUrgencia { get; set; }
    public EstadoAviso Estado { get; set; } = EstadoAviso.Reportado;

    // Id de la mascota generada a partir de este aviso (0..1), una vez atendido el rescate
    public int? MascotaGeneradaId { get; set; }
}
