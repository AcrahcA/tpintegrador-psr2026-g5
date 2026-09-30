namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdoptantesController : ControllerBase
{
    private readonly IAdoptanteService _adoptanteService;

    public AdoptantesController(IAdoptanteService adoptanteService)
    {
        _adoptanteService = adoptanteService;
    }

    // GET: api/adoptantes
    [HttpGet]
    public ActionResult<List<Adoptante>> Get()
    {
        return Ok(_adoptanteService.Get());
    }

    // GET: api/adoptantes/123
    [HttpGet("{id}")]
    public ActionResult<Adoptante> GetById(int id)
    {
        var adoptante = _adoptanteService.GetById(id);
        if (adoptante is null) return NotFound();
        return Ok(adoptante);
    }

    // POST: api/adoptantes
    [HttpPost]
    public ActionResult<Adoptante> Create([FromBody] Adoptante adoptante)
    {
        var creado = _adoptanteService.Post(adoptante);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    // DELETE: api/adoptantes/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _adoptanteService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}