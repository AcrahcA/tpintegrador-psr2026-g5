namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AvisosRescateController : ControllerBase
{
    private readonly IAvisoRescateService _avisoService;

    public AvisosRescateController(IAvisoRescateService avisoService)
    {
        _avisoService = avisoService;
    }

    [HttpGet]
    public ActionResult<List<AvisoRescate>> ObtenerTodos()
    {
        return Ok(_avisoService.ObtenerAvisos());
    }

    [HttpGet("pendientes")]
    public ActionResult<List<AvisoRescate>> ObtenerPendientes()
    {
        return Ok(_avisoService.ObtenerPendientes());
    }

    [HttpGet("{id}")]
    public ActionResult<AvisoRescate> ObtenerPorId(int id)
    {
        var aviso = _avisoService.BuscarAviso(id);
        if (aviso is null) return NotFound();
        return Ok(aviso);
    }

    [HttpPost]
    public ActionResult<AvisoRescate> Crear([FromBody] AvisoRescate aviso)
    {
        var creado = _avisoService.RegistrarAviso(aviso);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    // Endpoint genérico para la transición de cualquier EstadoAviso ("EnEvaluacion", "Aceptado", "Descartado", etc.)
    [HttpPut("{id}/estado")]
    public ActionResult CambiarEstado(int id, [FromBody] EstadoAviso nuevoEstado)
    {
        var actualizado = _avisoService.CambiarEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // Atiende el rescate y da de alta la mascota, verificando que esté en 'Aceptado' y la capacidad del refugio
    [HttpPost("{id}/atender")]
    public ActionResult<Mascota> AtenderYGenerarMascota(int id, [FromBody] Mascota datosMascota)
    {
        var mascota = _avisoService.AtenderAvisoYGenerarMascota(id, datosMascota);
        if (mascota is null)
            return BadRequest("No fue posible atender el aviso: asegúrese de que el aviso esté en estado 'Aceptado' y que el refugio tenga capacidad disponible.");

        return Ok(mascota);
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _avisoService.EliminarAviso(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}