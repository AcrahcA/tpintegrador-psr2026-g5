namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/historiales-sanitarios")]
public class HistorialSanitarioController : ControllerBase
{
    private readonly IHistorialSanitarioService _historialService;

    public HistorialSanitarioController(IHistorialSanitarioService historialService)
    {
        _historialService = historialService;
    }

    // GET: api/historiales-sanitarios
    [HttpGet]
    public ActionResult<List<HistorialSanitario>> Get()
    {
        return Ok(_historialService.Get());
    }

    // GET: api/historiales-sanitarios/123
    [HttpGet("{id}")]
    public ActionResult<HistorialSanitario> GetById(int id)
    {
        var historial = _historialService.GetById(id);
        if (historial is null) return NotFound();
        return Ok(historial);
    }

    // GET: api/historiales-sanitarios/mascotas/123
    [HttpGet("mascotas/{mascotaId}")]
    public ActionResult<HistorialSanitario> GetByMascota(int mascotaId)
    {
        var historial = _historialService.GetByMascotaId(mascotaId);
        if (historial is null) return NotFound("No se encontró historial sanitario para esa mascota.");
        return Ok(historial);
    }

    // POST: api/historiales-sanitarios
    [HttpPost]
    public ActionResult<HistorialSanitario> Create([FromBody] HistorialSanitario historial)
    {
        var creado = _historialService.Post(historial);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    // PUT: api/historiales-sanitarios/123
    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] HistorialSanitario historial)
    {
        var actualizado = _historialService.Put(id, historial);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // DELETE: api/historiales-sanitarios/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _historialService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}