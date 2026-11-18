namespace tpintegrador_psr2026.Api.Domain;

public class Mascota : IEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoAnimal Especie { get; set; }
    public string? Raza { get; set; }
    public int EdadAproximada { get; set; }
    public Sexo Sexo { get; set; }
    public Tamaño Tamaño { get; set; }
    public DateTime FechaIngreso { get; set; }
    public EstadoMascota Estado { get; set; } = EstadoMascota.Ingresada;
    public string? FotoUrl { get; set; }

    // Relaciones
    public int RefugioId { get; set; }
    public Refugio? Refugio { get; set; }

    public int? CuidadorId { get; set; }
    public Cuidador? Cuidador { get; set; }

    public HistorialSanitario? HistorialSanitario { get; set; }

    // Aviso de rescate que originó el ingreso (0..1)
    public int? AvisoRescateOrigenId { get; set; }

    protected Mascota() { }

    public Mascota(
        string nombre,
        TipoAnimal especie,
        int edadAproximada,
        Sexo sexo,
        Tamaño tamaño,
        string? raza = null)
    {
        Nombre = nombre;
        Especie = especie;
        EdadAproximada = edadAproximada;
        Sexo = sexo;
        Tamaño = tamaño;
        Raza = raza;
        FechaIngreso = DateTime.UtcNow;
        Estado = EstadoMascota.Ingresada;
    }

    /// <summary>Las mascotas adoptadas ya no ocupan plaza dentro del refugio.</summary>
    public bool EstaAlojada => Estado != EstadoMascota.Adoptada;

    public bool FueAdoptada => Estado == EstadoMascota.Adoptada;

    /// <summary>
    /// Estados que el personal puede asignar manualmente. "Ingresada" sólo se da al ingresar,
    /// "Reservada" al aprobar una solicitud y "Adoptada" al registrar la adopción.
    /// </summary>
    public static bool EsEstadoManual(EstadoMascota estado) =>
        estado is EstadoMascota.EnTratamiento or EstadoMascota.EnRecuperacion or EstadoMascota.DisponibleAdopcion;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
            throw new ReglaNegocioException("El nombre de la mascota es obligatorio.");
        if (EdadAproximada < 0 || EdadAproximada > 40)
            throw new ReglaNegocioException("La edad aproximada debe estar entre 0 y 40 años.");
    }
}
