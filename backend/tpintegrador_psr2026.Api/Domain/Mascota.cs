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

    // Relacion con Cuidador (0..1 del lado de la mascota)
    public int? CuidadorId { get; set; }

    // Composicion 1 a 1 con su historial sanitario
    public int HistorialSanitarioId { get; set; }

    // Referencia al aviso de rescate que le dio origen, si existe
    public int? AvisoRescateOrigenId { get; set; }
}
