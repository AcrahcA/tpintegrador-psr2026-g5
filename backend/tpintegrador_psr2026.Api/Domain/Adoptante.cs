namespace tpintegrador_psr2026.Api.Domain;

public class Adoptante : IEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }

    public ICollection<SolicitudAdopcion> Solicitudes { get; set; } = new List<SolicitudAdopcion>();

    protected Adoptante() { }

    public Adoptante(string nombre, string dni, string? telefono = null, string? direccion = null, string? email = null)
    {
        Nombre = nombre;
        Dni = dni;
        Telefono = telefono;
        Direccion = direccion;
        Email = email;
    }

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
            throw new ReglaNegocioException("El nombre del adoptante es obligatorio.");
        Dni = new string((Dni ?? string.Empty).Where(char.IsDigit).ToArray());
        if (Dni.Length is < 6 or > 9)
            throw new ReglaNegocioException("El DNI del adoptante debe tener entre 6 y 9 dígitos.");
    }
}
