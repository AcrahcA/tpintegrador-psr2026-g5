using System.Text.RegularExpressions;

namespace tpintegrador_psr2026.Api.Domain;

/// <summary>
/// Cuenta de una persona en el sitio. Las cuentas nuevas son siempre "Usuario";
/// el rol "Administrador" (acceso al panel del refugio) se asigna desde la base de datos.
/// </summary>
public class Usuario : IEntidad
{
    public const int LargoMinimoContraseña = 8;

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Usuario;
    public string? Telefono { get; set; }
    public DateTime FechaRegistro { get; set; }

    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

    protected Usuario() { }

    public Usuario(string nombre, string email, string passwordHash, string? telefono = null)
    {
        Nombre = nombre.Trim();
        Email = NormalizarEmail(email);
        PasswordHash = passwordHash;
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        Rol = RolUsuario.Usuario;
        FechaRegistro = DateTime.UtcNow;
    }

    public bool EsAdministrador => Rol == RolUsuario.Administrador;

    public static string NormalizarEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    public static void ValidarDatos(string? nombre, string? email)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 2)
            throw new ReglaNegocioException("Ingresá tu nombre.");
        if (!Regex.IsMatch(NormalizarEmail(email), @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            throw new ReglaNegocioException("El correo electrónico no tiene un formato válido.");
    }

    /// <summary>Al menos 8 caracteres, con letras y números.</summary>
    public static void ValidarContraseña(string? contraseña)
    {
        if (string.IsNullOrEmpty(contraseña) || contraseña.Length < LargoMinimoContraseña
            || !contraseña.Any(char.IsLetter) || !contraseña.Any(char.IsDigit))
            throw new ReglaNegocioException($"La contraseña debe tener al menos {LargoMinimoContraseña} caracteres, con letras y números.");
    }
}
