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

    public SolicitudesAdopcionController(ISolicitudAdopcionService solicitudService) => _solicitudService = solicitudService;

    private static List<SolicitudAdopcionResponseDto> Lista(IEnumerable<SolicitudAdopcion> s) =>
        s.Select(SolicitudAdopcionResponseDto.Desde).ToList();

    // GET: api/solicitudes-adopcion  |  ?estado=Aprobada
    [HttpGet]
    public ActionResult<List<SolicitudAdopcionResponseDto>> Get([FromQuery] EstadoSolicitud? estado) =>
        Ok(Lista(estado is null ? _solicitudService.Get() : _solicitudService.GetPorEstado(estado.Value)));

    // GET: api/solicitudes-adopcion/pendientes  (Pendiente o EnEvaluacion)
    [HttpGet("pendientes")]
    public ActionResult<List<SolicitudAdopcionResponseDto>> GetPendientes() => Ok(Lista(_solicitudService.GetPendientes()));

    [HttpGet("mascotas/{mascotaId:int}")]
    public ActionResult<List<SolicitudAdopcionResponseDto>> GetPorMascota(int mascotaId) =>
        Ok(Lista(_solicitudService.GetPorMascota(mascotaId)));

    [HttpGet("{id:int}")]
    public ActionResult<SolicitudAdopcionResponseDto> GetById(int id) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.ObtenerPorId(id)));

    // POST: api/solicitudes-adopcion  (sólo sobre mascotas disponibles para adopción)
    [HttpPost]
    public ActionResult<SolicitudAdopcionResponseDto> Create([FromBody] SolicitudAdopcionRequestDto request)
    {
        var creada = _solicitudService.Crear(request.AdoptanteId, request.MascotaId);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, SolicitudAdopcionResponseDto.Desde(creada));
    }

    // POST: api/solicitudes-adopcion/publica  (desde el sitio público: datos del adoptante + mascota)
    [HttpPost("publica")]
    public ActionResult<SolicitudAdopcionResponseDto> CreatePublica([FromBody] SolicitudPublicaRequestDto request)
    {
        var creada = _solicitudService.CrearPublica(request.ADominio(), request.MascotaId);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, SolicitudAdopcionResponseDto.Desde(creada));
    }

    // PUT: api/solicitudes-adopcion/3/estado   body: "EnEvaluacion" | "Aprobada" | "Rechazada" | "Cancelada"
    [HttpPut("{id:int}/estado")]
    public ActionResult<SolicitudAdopcionResponseDto> UpdateEstado(int id, [FromBody] EstadoSolicitud nuevoEstado) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.CambiarEstado(id, nuevoEstado)));

    // PUT: api/solicitudes-adopcion/3/aprobar  (verifica que la mascota siga disponible; la reserva)
    [HttpPut("{id:int}/aprobar")]
    public ActionResult<SolicitudAdopcionResponseDto> Aprobar(int id) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.Aprobar(id)));

    [HttpPut("{id:int}/rechazar")]
    public ActionResult<SolicitudAdopcionResponseDto> Rechazar(int id) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.Rechazar(id)));

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _solicitudService.Eliminar(id);
        return NoContent();
    }
}
