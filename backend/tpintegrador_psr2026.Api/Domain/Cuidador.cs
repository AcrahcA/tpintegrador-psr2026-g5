using System.Text.Json.Serialization;

namespace tpintegrador_psr2026.Api.Domain;

public class Cuidador
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Especializacion { get; set; }
    public int CapacidadMaxima { get; set; }

    // Al asignarle 1 por defecto y usar JsonIgnore en RefugioId y Refugio, 
    // Swagger no los solicitará en el cuerpo del JSON.
    [JsonIgnore]
    public int? RefugioId { get; set; } = 1;

    [JsonIgnore]
    public Refugio? Refugio { get; set; }
}