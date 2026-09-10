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

    [HttpGet]
    public ActionResult<List<Adoptante>> ObtenerTodos()
    {
        return Ok(_adoptanteService.ObtenerAdoptantes());
    }

    [HttpGet("{id}")]
    public ActionResult<Adoptante> ObtenerPorId(int id)
    {
        var adoptante = _adoptanteService.BuscarAdoptante(id);
        if (adoptante is null) return NotFound();
        return Ok(adoptante);
    }

    [HttpPost]
    public ActionResult<Adoptante> Crear([FromBody] Adoptante adoptante)
    {
        var creado = _adoptanteService.RegistrarAdoptante(adoptante);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _adoptanteService.EliminarAdoptante(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}