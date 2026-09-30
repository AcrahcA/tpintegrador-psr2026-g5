namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class TratamientosController : ControllerBase
{
    private readonly ITratamientoService _tratamientoService;

    public TratamientosController(ITratamientoService tratamientoService)
    {
        _tratamientoService = tratamientoService;
    }

    // GET: api/tratamientos
    [HttpGet]
    public ActionResult<List<Tratamiento>> Get()
    {
        return Ok(_tratamientoService.Get());
    }

    // GET: api/tratamientos/mascotas/123
    [HttpGet("mascotas/{mascotaId}")]
    public ActionResult<List<Tratamiento>> GetByMascota(int mascotaId)
    {
        return Ok(_tratamientoService.GetByMascotaId(mascotaId));
    }

    // GET: api/tratamientos/123
    [HttpGet("{id}")]
    public ActionResult<Tratamiento> GetById(int id)
    {
        var tratamiento = _tratamientoService.GetById(id);
        if (tratamiento is null) return NotFound();
        return Ok(tratamiento);
    }

    // POST: api/tratamientos/mascotas/123
    [HttpPost("mascotas/{mascotaId}")]
    public ActionResult<Tratamiento> Create(int mascotaId, [FromBody] Tratamiento tratamiento)
    {
        var creado = _tratamientoService.Post(mascotaId, tratamiento);
        if (creado is null)
            return BadRequest("No se pudo registrar el tratamiento. Verifique que la mascota y su historial sanitario existan.");

        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    // PUT: api/tratamientos/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoTratamiento nuevoEstado)
    {
        var actualizado = _tratamientoService.PutEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // DELETE: api/tratamientos/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _tratamientoService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}