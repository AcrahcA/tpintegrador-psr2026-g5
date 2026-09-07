namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _mascotaService;

    public MascotasController(IMascotaService mascotaService)
    {
        _mascotaService = mascotaService;
    }

    [HttpGet]
    public ActionResult<List<Mascota>> ObtenerTodas()
    {
        return Ok(_mascotaService.ObtenerMascotas());
    }

    [HttpGet("{id}")]
    public ActionResult<Mascota> ObtenerPorId(int id)
    {
        var mascota = _mascotaService.BuscarMascota(id);
        if (mascota is null) return NotFound();
        return Ok(mascota);
    }

    [HttpPost]
    public ActionResult<Mascota> Crear([FromBody] Mascota mascota)
    {
        var creada = _mascotaService.IngresarMascota(mascota);
        if (creada is null)
            return BadRequest("El refugio alcanzo su capacidad maxima.");

        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}/estado")]
    public ActionResult CambiarEstado(int id, [FromBody] EstadoMascota nuevoEstado)
    {
        var actualizado = _mascotaService.CambiarEstado(id, nuevoEstado);
        if (!actualizado) return BadRequest("No se pudo cambiar el estado, revise las condiciones sanitarias de la mascota.");
        return NoContent();
    }

    [HttpPut("{id}/cuidador/{cuidadorId}")]
    public ActionResult AsignarCuidador(int id, int cuidadorId)
    {
        var asignado = _mascotaService.AsignarCuidador(id, cuidadorId);
        if (!asignado) return BadRequest("El cuidador no existe o alcanzo su capacidad maxima.");
        return NoContent();
    }

    [HttpGet("{id}/disponible-adopcion")]
    public ActionResult<bool> EstaDisponibleParaAdopcion(int id)
    {
        return Ok(_mascotaService.EstaDisponibleParaAdopcion(id));
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminada = _mascotaService.EliminarMascota(id);
        if (!eliminada) return NotFound();
        return NoContent();
    }
}
