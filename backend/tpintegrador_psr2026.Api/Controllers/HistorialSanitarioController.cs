namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class HistorialSanitarioController : ControllerBase
{
    private readonly IHistorialSanitarioService _historialService;

    public HistorialSanitarioController(IHistorialSanitarioService historialService)
    {
        _historialService = historialService;
    }

    [HttpGet]
    public ActionResult<List<HistorialSanitario>> ObtenerTodos()
    {
        return Ok(_historialService.ObtenerHistoriales());
    }

    [HttpGet("{id}")]
    public ActionResult<HistorialSanitario> ObtenerPorId(int id)
    {
        var historial = _historialService.BuscarPorId(id);
        if (historial is null) return NotFound();
        return Ok(historial);
    }

    [HttpGet("mascota/{mascotaId}")]
    public ActionResult<HistorialSanitario> ObtenerPorMascota(int mascotaId)
    {
        var historial = _historialService.BuscarPorMascotaId(mascotaId);
        if (historial is null) return NotFound("No se encontró historial sanitario para esa mascota.");
        return Ok(historial);
    }

    [HttpPost]
    public ActionResult<HistorialSanitario> Crear([FromBody] HistorialSanitario historial)
    {
        var creado = _historialService.CrearHistorial(historial);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public ActionResult Actualizar(int id, [FromBody] HistorialSanitario historial)
    {
        var actualizado = _historialService.ActualizarHistorial(id, historial);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _historialService.EliminarHistorial(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}