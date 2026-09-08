using System.Text.Json.Serialization;

namespace tpintegrador_psr2026.Api.Domain;

public class Refugio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }

    // Colecciones de Agregación (Se ignoran en el JSON para que Swagger no las pida)
    [JsonIgnore]
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();

    [JsonIgnore]
    public ICollection<Cuidador> Cuidadores { get; set; } = new List<Cuidador>();
}