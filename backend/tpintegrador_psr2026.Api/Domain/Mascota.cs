using System.Text.Json.Serialization;

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

    [JsonIgnore] // La fecha se asigna automáticamente al crear
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    public EstadoMascota Estado { get; set; } = EstadoMascota.Ingresada;

    // --- Relaciones y Navegación (Se ignoran en el JSON de Swagger) ---
    [JsonIgnore]
    public int? RefugioId { get; set; }

    [JsonIgnore]
    public Refugio? Refugio { get; set; }

    [JsonIgnore]
    public int? CuidadorId { get; set; }

    [JsonIgnore]
    public Cuidador? Cuidador { get; set; }

    [JsonIgnore]
    public HistorialSanitario? HistorialSanitario { get; set; }

    [JsonIgnore]
    public int? AvisoRescateOrigenId { get; set; }
}