namespace tpintegrador_psr2026.Api.Domain;

public class Refugio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }

    // Colecciones de Agregación
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    public ICollection<Cuidador> Cuidadores { get; set; } = new List<Cuidador>();
}