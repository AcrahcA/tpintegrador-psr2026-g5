namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdopcionesController : ControllerBase
{
    private readonly IAdopcionService _adopcionService;

    public AdopcionesController(IAdopcionService adopcionService)
    {
        _adopcionService = adopcionService;
    }

    [HttpGet]
    public ActionResult<List<Adopcion>> ObtenerTodas()
    {
        return Ok(_adopcionService.ObtenerAdopciones());
    }

    [HttpGet("{id}")]
    public ActionResult<Adopcion> ObtenerPorId(int id)
    {
        var adopcion = _adopcionService.BuscarAdopcion(id);
        if (adopcion is null) return NotFound();
        return Ok(adopcion);
    }

    public record NuevaAdopcionRequest(int SolicitudAdopcionId, string? Observaciones);

    [HttpPost]
    public ActionResult<Adopcion> Crear([FromBody] NuevaAdopcionRequest request)
    {
        var creada = _adopcionService.ConfirmarAdopcion(request.SolicitudAdopcionId, request.Observaciones);
        if (creada is null)
            return BadRequest("No se cumplen las condiciones para confirmar la adopcion (solicitud aprobada, mascota disponible y sin tratamientos activos).");

        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }
}
