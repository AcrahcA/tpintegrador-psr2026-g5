namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesAdopcionController : ControllerBase
{
    private readonly ISolicitudAdopcionService _solicitudService;

    public SolicitudesAdopcionController(ISolicitudAdopcionService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpGet]
    public ActionResult<List<SolicitudAdopcion>> ObtenerTodas()
    {
        return Ok(_solicitudService.ObtenerSolicitudes());
    }

    [HttpGet("pendientes")]
    public ActionResult<List<SolicitudAdopcion>> ObtenerPendientes()
    {
        return Ok(_solicitudService.ObtenerPendientes());
    }

    [HttpGet("{id}")]
    public ActionResult<SolicitudAdopcion> ObtenerPorId(int id)
    {
        var solicitud = _solicitudService.BuscarSolicitud(id);
        if (solicitud is null) return NotFound();
        return Ok(solicitud);
    }

    public record NuevaSolicitudRequest(int AdoptanteId, int MascotaId);

    [HttpPost]
    public ActionResult<SolicitudAdopcion> Crear([FromBody] NuevaSolicitudRequest request)
    {
        var creada = _solicitudService.RealizarSolicitud(request.AdoptanteId, request.MascotaId);
        if (creada is null)
            return BadRequest("La mascota no existe o no se encuentra disponible para adopcion.");

        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}/estado")]
    public ActionResult CambiarEstado(int id, [FromBody] EstadoSolicitud nuevoEstado)
    {
        var actualizado = _solicitudService.CambiarEstado(id, nuevoEstado);
        if (!actualizado) 
            return BadRequest("No se pudo cambiar el estado de la solicitud. Verifique que la solicitud exista y que la mascota continue disponible.");
            
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _solicitudService.EliminarSolicitud(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}