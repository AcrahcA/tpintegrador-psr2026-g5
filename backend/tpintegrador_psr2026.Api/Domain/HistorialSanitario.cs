namespace tpintegrador_psr2026.Api.Domain;

/// <summary>
/// Registro sanitario de una mascota (composición 1 a 1). Nunca se elimina, aunque la mascota sea adoptada.
/// </summary>
public class HistorialSanitario : IEntidad
{
    public int Id { get; set; }
    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    protected HistorialSanitario() { }

    public HistorialSanitario(int mascotaId, string? observaciones = null)
    {
        MascotaId = mascotaId;
        Observaciones = observaciones;
    }
}
