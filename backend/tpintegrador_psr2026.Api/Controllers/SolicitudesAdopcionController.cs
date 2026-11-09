namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
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
    public ActionResult<List<SolicitudAdopcionResponseDto>> Get()
    {
        var solicitudes = _solicitudService.Get();

        var response = solicitudes.Select(s => new SolicitudAdopcionResponseDto(
            Id: s.Id,
            AdoptanteId: s.AdoptanteId,
            MascotaId: s.MascotaId,
            FechaSolicitud: s.FechaSolicitud,
            Estado: s.Estado
        )).ToList();

        return Ok(response);
    }

    // GET: api/solicitudes-adopcion/pendientes
    [HttpGet("pendientes")]
    public ActionResult<List<SolicitudAdopcionResponseDto>> GetPendientes()
    {
        var solicitudes = _solicitudService.GetPendientes();

        var response = solicitudes.Select(s => new SolicitudAdopcionResponseDto(
            Id: s.Id,
            AdoptanteId: s.AdoptanteId,
            MascotaId: s.MascotaId,
            FechaSolicitud: s.FechaSolicitud,
            Estado: s.Estado
        )).ToList();

        return Ok(response);
    }

    // GET: api/solicitudes-adopcion/123
    [HttpGet("{id}")]
    public ActionResult<SolicitudAdopcionResponseDto> GetById(int id)
    {
        var s = _solicitudService.GetById(id);
        if (s is null) return NotFound();

        var response = new SolicitudAdopcionResponseDto(
            Id: s.Id,
            AdoptanteId: s.AdoptanteId,
            MascotaId: s.MascotaId,
            FechaSolicitud: s.FechaSolicitud,
            Estado: s.Estado
        );

        return Ok(response);
    }

    // POST: api/solicitudes-adopcion
    [HttpPost]
    public ActionResult<SolicitudAdopcionResponseDto> Create([FromBody] SolicitudAdopcionRequestDto request)
    {
        // Llamamos al servicio usando los datos del DTO recibido
        var creada = _solicitudService.Post(request.AdoptanteId, request.MascotaId);
        if (creada is null)
            return BadRequest("No se pudo crear la solicitud. Verifique que el adoptante y la mascota existan y que la mascota esté disponible para adopción.");

        var response = new SolicitudAdopcionResponseDto(
            Id: creada.Id,
            AdoptanteId: creada.AdoptanteId,
            MascotaId: creada.MascotaId,
            FechaSolicitud: creada.FechaSolicitud,
            Estado: creada.Estado
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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