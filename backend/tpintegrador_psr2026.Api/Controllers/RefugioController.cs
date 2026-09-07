namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class RefugioController : ControllerBase
{
    private readonly IRefugioService _refugioService;

    public RefugioController(IRefugioService refugioService)
    {
        _refugioService = refugioService;
    }

    [HttpGet]
    public ActionResult<Refugio> Obtener()
    {
        return Ok(_refugioService.ObtenerConfiguracion());
    }

    [HttpPut]
    public ActionResult<Refugio> Configurar([FromBody] Refugio refugio)
    {
        return Ok(_refugioService.ConfigurarRefugio(refugio));
    }

    [HttpGet("disponibilidad")]
    public ActionResult<object> ObtenerDisponibilidad()
    {
        return Ok(new
        {
            capacidadMaxima = _refugioService.ObtenerConfiguracion().CapacidadMaxima,
            ocupacionActual = _refugioService.ContarMascotasActuales(),
            tieneLugarDisponible = _refugioService.TieneLugarDisponible()
        });
    }
}
