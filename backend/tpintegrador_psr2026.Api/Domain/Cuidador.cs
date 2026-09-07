namespace tpintegrador_psr2026.Api.Domain;

public class Cuidador
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Especializacion { get; set; }
    public int CapacidadMaxima { get; set; }

    // Agregación con Refugio (Un cuidador pertenece a un Refugio)
    public int? RefugioId { get; set; }
    public Refugio? Refugio { get; set; }
}