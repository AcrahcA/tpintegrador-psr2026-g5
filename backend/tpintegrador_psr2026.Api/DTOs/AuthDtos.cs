using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class RegistroRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Contraseña { get; set; } = string.Empty;
}

public record UsuarioDto(int Id, string Nombre, string Email, RolUsuario Rol, string? Telefono)
{
    public static UsuarioDto Desde(Usuario u) => new(u.Id, u.Nombre, u.Email, u.Rol, u.Telefono);
}

/// <summary>Sesión actual: la cuenta y, si ya pidió adoptar, sus datos de adoptante (para completar formularios).</summary>
public record SesionDto(UsuarioDto Usuario, AdoptanteResponseDto? Adoptante, int NotificacionesNoLeidas);

public record NotificacionDto(int Id, TipoNotificacion Tipo, string Titulo, string Mensaje, string? Motivo, DateTime Fecha,
    bool Leida, int? SolicitudAdopcionId, int? AvisoRescateId)
{
    public static NotificacionDto Desde(Notificacion n) =>
        new(n.Id, n.Tipo, n.Titulo, n.Mensaje, n.Motivo, n.Fecha, n.Leida, n.SolicitudAdopcionId, n.AvisoRescateId);
}

/// <summary>Motivo opcional que el administrador puede escribir al rechazar, cancelar o descartar.</summary>
public class MotivoDto
{
    public string? Motivo { get; set; }
}
