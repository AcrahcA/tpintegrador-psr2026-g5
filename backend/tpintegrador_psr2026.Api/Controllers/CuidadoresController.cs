namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly ICuidadorService _cuidadorService;

    public CuidadoresController(ICuidadorService cuidadorService)
    {
        _cuidadorService = cuidadorService;
    }

    // GET: api/cuidadores
    [HttpGet]
    public ActionResult<List<Cuidador>> Get()
    {
        return Ok(_cuidadorService.Get());
    }

    // GET: api/cuidadores/123
    [HttpGet("{id}")]
    public ActionResult<Cuidador> GetById(int id)
    {
        var cuidador = _cuidadorService.GetById(id);
        if (cuidador is null) return NotFound();
        return Ok(cuidador);
    }

    // POST: api/cuidadores
    [HttpPost]
    public ActionResult<Cuidador> Create([FromBody] Cuidador cuidador)
    {
        var creado = _cuidadorService.Post(cuidador);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    // PUT: api/cuidadores/123
    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] Cuidador cuidador)
    {
        var actualizado = _cuidadorService.Put(id, cuidador);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // GET: api/cuidadores/123/disponibilidad
    [HttpGet("{id}/disponibilidad")]
    public ActionResult<object> GetDisponibilidad(int id)
    {
        var cuidador = _cuidadorService.GetById(id);
        if (cuidador is null) return NotFound();

        return Ok(new
        {
            capacidadMaxima = cuidador.CapacidadMaxima,
            asignadasActualmente = _cuidadorService.ObtenerCantidadAsignadas(id),
            tieneDisponibilidad = _cuidadorService.TieneDisponibilidad(id)
        });
    }

    // DELETE: api/cuidadores/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _cuidadorService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}