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

    // GET: api/refugio
    [HttpGet]
    public ActionResult<Refugio> Get()
    {
        return Ok(_refugioService.Get());
    }

    // POST: api/refugio
    [HttpPost]
    public ActionResult<Refugio> Post([FromBody] Refugio refugio)
    {
        var configurado = _refugioService.Post(refugio);
        return Ok(configurado);
    }

    // GET: api/refugio/capacidad
    [HttpGet("capacidad")]
    public ActionResult<object> GetCapacidad()
    {
        var refugio = _refugioService.Get();
        var ocupadas = _refugioService.ContarMascotasActuales();

        return Ok(new
        {
            capacidadMaxima = refugio.CapacidadMaxima,
            mascotasActuales = ocupadas,
            plazasDisponibles = refugio.CapacidadMaxima - ocupadas,
            tieneLugarDisponible = _refugioService.TieneLugarDisponible()
        });
    }
}