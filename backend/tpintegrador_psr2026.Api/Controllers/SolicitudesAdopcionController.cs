namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/solicitudes-adopcion")]
public class SolicitudesAdopcionController : ControllerBase
{
    private readonly ISolicitudAdopcionService _solicitudService;

    public SolicitudesAdopcionController(ISolicitudAdopcionService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    // GET: api/solicitudes-adopcion
    [HttpGet]
    public ActionResult<List<SolicitudAdopcion>> Get()
    {
        return Ok(_solicitudService.Get());
    }

    // GET: api/solicitudes-adopcion/pendientes
    [HttpGet("pendientes")]
    public ActionResult<List<SolicitudAdopcion>> GetPendientes()
    {
        return Ok(_solicitudService.GetPendientes());
    }

    // GET: api/solicitudes-adopcion/123
    [HttpGet("{id}")]
    public ActionResult<SolicitudAdopcion> GetById(int id)
    {
        var solicitud = _solicitudService.GetById(id);
        if (solicitud is null) return NotFound();
        return Ok(solicitud);
    }

    public record CreateSolicitudRequest(int AdoptanteId, int MascotaId);

    // POST: api/solicitudes-adopcion
    [HttpPost]
    public ActionResult<SolicitudAdopcion> Create([FromBody] CreateSolicitudRequest request)
    {
        var creada = _solicitudService.Post(request.AdoptanteId, request.MascotaId);
        if (creada is null)
            return BadRequest("No se pudo crear la solicitud. Verifique que el adoptante y la mascota existan y que la mascota esté disponible para adopción.");

        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    // PUT: api/solicitudes-adopcion/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoSolicitud nuevoEstado)
    {
        var actualizado = _solicitudService.PutEstado(id, nuevoEstado);
        if (!actualizado)
            return BadRequest("No se pudo cambiar el estado. Verifique que la solicitud exista y que la mascota continúe disponible si está aprobando la solicitud.");

        return NoContent();
    }

    // DELETE: api/solicitudes-adopcion/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _solicitudService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}