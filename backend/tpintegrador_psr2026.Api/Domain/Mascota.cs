namespace tpintegrador_psr2026.Api.Domain;

public class Mascota
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public int EdadAproximada { get; set; }
    public Sexo Sexo { get; set; }
    public Tamaño Tamaño { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public EstadoMascota Estado { get; set; } = EstadoMascota.Ingresada;

    // Agregación con Refugio (1 Refugio a N Mascotas)
    public int? RefugioId { get; set; }
    public Refugio? Refugio { get; set; }

    // Asociación con Cuidador (0..1)
    public int? CuidadorId { get; set; }
    public Cuidador? Cuidador { get; set; }

    // Composición 1 a 1 con HistorialSanitario
    public HistorialSanitario? HistorialSanitario { get; set; }

    // Referencia al AvisoRescate que le dio origen
    public int? AvisoRescateOrigenId { get; set; }
}
