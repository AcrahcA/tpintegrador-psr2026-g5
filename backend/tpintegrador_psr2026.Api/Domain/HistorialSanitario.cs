namespace tpintegrador_psr2026.Api.Domain;

public class HistorialSanitario
{
    public int Id { get; set; }
    
    // Composición: Pertenece a una única Mascota (FK explícita)
    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public string? Observaciones { get; set; }

    // Composición 1 a N con Tratamientos
    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}