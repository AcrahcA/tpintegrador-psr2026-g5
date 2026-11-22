namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;
using tpintegrador_psr2026.Api.Services.Seguridad;

/// <summary>Buzón de mensajes de la persona que inició sesión (cada uno ve sólo los suyos).</summary>
[ApiController]
[Route("api/buzon")]
[Authorize]
public class BuzonController : ControllerBase
{
    private readonly INotificacionService _notificaciones;

    public BuzonController(INotificacionService notificaciones) => _notificaciones = notificaciones;

    private int UsuarioId => User.UsuarioId()!.Value;

    // GET: api/buzon
    [HttpGet]
    public ActionResult<List<NotificacionDto>> Get() =>
        Ok(_notificaciones.Buzon(UsuarioId).Select(NotificacionDto.Desde).ToList());

    // GET: api/buzon/no-leidas
    [HttpGet("no-leidas")]
    public ActionResult<object> NoLeidas() => Ok(new { cantidad = _notificaciones.ContarNoLeidas(UsuarioId) });

    // PUT: api/buzon/5/leida
    [HttpPut("{id:int}/leida")]
    public ActionResult<NotificacionDto> MarcarLeida(int id) =>
        Ok(NotificacionDto.Desde(_notificaciones.MarcarLeida(UsuarioId, id)));

    // PUT: api/buzon/leer-todas
    [HttpPut("leer-todas")]
    public ActionResult LeerTodas()
    {
        _notificaciones.MarcarTodasLeidas(UsuarioId);
        return NoContent();
    }
}
