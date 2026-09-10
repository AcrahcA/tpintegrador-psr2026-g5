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

    [HttpGet]
    public ActionResult<List<Cuidador>> ObtenerTodos()
    {
        return Ok(_cuidadorService.ObtenerCuidadores());
    }

    [HttpGet("{id}")]
    public ActionResult<AvisoRescate> ObtenerPorId(int id)
    {
        var cuidador = _cuidadorService.BuscarCuidador(id);
        if (cuidador is null) return NotFound();
        return Ok(cuidador);
    }

    [HttpPost]
    public ActionResult<Cuidador> Crear([FromBody] Cuidador cuidador)
    {
        var creado = _cuidadorService.RegistrarCuidador(cuidador);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public ActionResult Actualizar(int id, [FromBody] Cuidador cuidador)
    {
        var actualizado = _cuidadorService.ActualizarCuidador(id, cuidador);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/disponibilidad")]
    public ActionResult<object> ObtenerDisponibilidad(int id)
    {
        var cuidador = _cuidadorService.BuscarCuidador(id);
        if (cuidador is null) return NotFound();

        return Ok(new
        {
            capacidadMaxima = cuidador.CapacidadMaxima,
            asignadasActualmente = _cuidadorService.ObtenerCantidadAsignadas(id),
            tieneDisponibilidad = _cuidadorService.TieneDisponibilidad(id)
        });
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _cuidadorService.EliminarCuidador(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}