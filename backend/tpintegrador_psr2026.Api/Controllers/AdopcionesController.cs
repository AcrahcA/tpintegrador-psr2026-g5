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

    // GET: api/adopciones
    [HttpGet]
    public ActionResult<List<Adopcion>> Get()
    {
        return Ok(_adopcionService.Get());
    }

    // GET: api/adopciones/123
    [HttpGet("{id}")]
    public ActionResult<Adopcion> GetById(int id)
    {
        var adopcion = _adopcionService.GetById(id);
        if (adopcion is null) return NotFound();
        return Ok(adopcion);
    }

    public record CreateAdopcionRequest(int SolicitudId, string? Observaciones);

    // POST: api/adopciones
    [HttpPost]
    public ActionResult<Adopcion> Create([FromBody] CreateAdopcionRequest request)
    {
        var creada = _adopcionService.Post(request.SolicitudId, request.Observaciones);
        if (creada is null)
            return BadRequest("No se pudo registrar la adopción. Verifique que la solicitud esté aprobada y la mascota disponible.");

        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }
}