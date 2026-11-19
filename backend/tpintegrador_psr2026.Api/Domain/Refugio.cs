namespace tpintegrador_psr2026.Api.Domain;

public class Refugio : IEntidad
{
    public const string NombrePorDefecto = "Refugio de Mascotas";
    public const int CapacidadPorDefecto = 50;

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    public ICollection<Cuidador> Cuidadores { get; set; } = new List<Cuidador>();

    protected Refugio() { }

    public Refugio(string nombre, int capacidadMaxima)
    {
        Nombre = nombre;
        CapacidadMaxima = capacidadMaxima;
    }

    /// <summary>Indica si hay plazas libres, dado el número de mascotas alojadas (las adoptadas no ocupan plaza).</summary>
    public bool TieneLugarDisponible(int mascotasAlojadas) => mascotasAlojadas < CapacidadMaxima;

    public int PlazasDisponibles(int mascotasAlojadas) => Math.Max(0, CapacidadMaxima - mascotasAlojadas);
}
