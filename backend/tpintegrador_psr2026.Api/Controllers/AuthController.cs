namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;
using tpintegrador_psr2026.Api.Services.Seguridad;

/// <summary>Cuentas del sitio: registro, inicio y cierre de sesión.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IAdoptanteService _adoptanteService;
    private readonly INotificacionService _notificaciones;

    public AuthController(IUsuarioService usuarioService, IAdoptanteService adoptanteService, INotificacionService notificaciones)
    {
        _usuarioService = usuarioService;
        _adoptanteService = adoptanteService;
        _notificaciones = notificaciones;
    }

    private SesionDto Dto(Domain.Usuario u)
    {
        var adoptante = _adoptanteService.GetPorUsuario(u.Id);
        return new SesionDto(UsuarioDto.Desde(u), adoptante is null ? null : AdoptanteResponseDto.Desde(adoptante), _notificaciones.ContarNoLeidas(u.Id));
    }

    // POST: api/auth/registro  (las cuentas nuevas son siempre "Usuario")
    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<SesionDto>> Registro([FromBody] RegistroRequestDto request)
    {
        var usuario = _usuarioService.Registrar(request.Nombre, request.Email, request.Contraseña, request.Telefono);
        await Sesion.IniciarAsync(HttpContext, usuario);
        return StatusCode(StatusCodes.Status201Created, Dto(usuario));
    }

    // POST: api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<SesionDto>> Login([FromBody] LoginRequestDto request)
    {
        var usuario = _usuarioService.Autenticar(request.Email, request.Contraseña);
        await Sesion.IniciarAsync(HttpContext, usuario);
        return Ok(Dto(usuario));
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<ActionResult> Logout()
    {
        await Sesion.CerrarAsync(HttpContext);
        return NoContent();
    }

    // GET: api/auth/yo  (sesión actual; 401 si no inició sesión)
    [HttpGet("yo")]
    [Authorize]
    public ActionResult<SesionDto> Yo()
    {
        var usuario = _usuarioService.GetById(User.UsuarioId()!.Value);
        if (usuario is null) return Unauthorized();
        return Ok(Dto(usuario));
    }
}
