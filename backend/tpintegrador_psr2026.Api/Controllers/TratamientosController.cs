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

    [HttpGet]
    public ActionResult<List<Tratamiento>> ObtenerTodos()
    {
        return Ok(_tratamientoService.ObtenerTratamientos());
    }

    [HttpGet("mascota/{mascotaId}")]
    public ActionResult<List<Tratamiento>> ObtenerPorMascota(int mascotaId)
    {
        return Ok(_tratamientoService.ObtenerTratamientosDeMascota(mascotaId));
    }

    [HttpGet("{id}")]
    public ActionResult<Tratamiento> ObtenerPorId(int id)
    {
        var tratamiento = _tratamientoService.BuscarTratamiento(id);
        if (tratamiento is null) return NotFound();
        return Ok(tratamiento);
    }

    [HttpPost("mascota/{mascotaId}")]
    public ActionResult<Tratamiento> Crear(int mascotaId, [FromBody] Tratamiento tratamiento)
    {
        var creado = _tratamientoService.RegistrarTratamiento(mascotaId, tratamiento);
        if (creado is null) return NotFound("La mascota indicada no existe.");

        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}/estado")]
    public ActionResult CambiarEstado(int id, [FromBody] EstadoTratamiento nuevoEstado)
    {
        var actualizado = _tratamientoService.CambiarEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _tratamientoService.EliminarTratamiento(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}
