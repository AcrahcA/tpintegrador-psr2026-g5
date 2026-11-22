using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Services.Seguridad;

public static class Roles
{
    public const string Administrador = nameof(RolUsuario.Administrador);
    public const string Usuario = nameof(RolUsuario.Usuario);
}

/// <summary>Inicio y cierre de sesión con una cookie HttpOnly (el navegador no la expone a JavaScript).</summary>
public static class Sesion
{
    public static ClaimsPrincipal CrearPrincipal(Usuario u) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
        new Claim(ClaimTypes.Name, u.Nombre),
        new Claim(ClaimTypes.Email, u.Email),
        new Claim(ClaimTypes.Role, u.Rol.ToString())
    }, CookieAuthenticationDefaults.AuthenticationScheme));

    public static Task IniciarAsync(HttpContext http, Usuario u) =>
        http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, CrearPrincipal(u),
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });

    public static Task CerrarAsync(HttpContext http) => http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    /// <summary>Id del usuario de la sesión actual, o null si no inició sesión.</summary>
    public static int? UsuarioId(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true && int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
