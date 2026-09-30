namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/avisos-rescate")]
public class AvisosRescateController : ControllerBase
{
    private readonly IAvisoRescateService _avisoService;

    public AvisosRescateController(IAvisoRescateService avisoService)
    {
        _avisoService = avisoService;
    }

    // GET: api/avisos-rescate
    [HttpGet]
    public ActionResult<List<AvisoRescate>> Get()
    {
        return Ok(_avisoService.Get());
    }

    // GET: api/avisos-rescate/pendientes
    [HttpGet("pendientes")]
    public ActionResult<List<AvisoRescate>> GetPendientes()
    {
        return Ok(_avisoService.GetPendientes());
    }

    // GET: api/avisos-rescate/123
    [HttpGet("{id}")]
    public ActionResult<AvisoRescate> GetById(int id)
    {
        var aviso = _avisoService.GetById(id);
        if (aviso is null) return NotFound();
        return Ok(aviso);
    }

    // POST: api/avisos-rescate
    [HttpPost]
    public ActionResult<AvisoRescate> Create([FromBody] AvisoRescate aviso)
    {
        var creado = _avisoService.Post(aviso);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    // PUT: api/avisos-rescate/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoAviso nuevoEstado)
    {
        var actualizado = _avisoService.PutEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // POST: api/avisos-rescate/123/mascotas
    [HttpPost("{id}/mascotas")]
    public ActionResult<Mascota> AtenderYGenerarMascota(int id, [FromBody] Mascota datosMascota)
    {
        var mascota = _avisoService.AtenderAvisoYGenerarMascota(id, datosMascota);
        if (mascota is null)
            return BadRequest("No fue posible atender el aviso: asegúrese de que el aviso esté en estado 'Aceptado' y que el refugio tenga capacidad disponible.");

        return Ok(mascota);
    }

    // DELETE: api/avisos-rescate/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _avisoService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}