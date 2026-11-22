namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Services.Seguridad;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Authorize(Roles = Roles.Administrador)]  // panel del refugio: sólo administradores (las acciones públicas se marcan aparte)
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

    // POST: api/solicitudes-adopcion/publica  (desde el sitio: cualquier persona con sesión iniciada)
    // Se marca AllowAnonymous para no exigir el rol de administrador de la clase, y se exige la sesión acá.
    [HttpPost("publica")]
    [AllowAnonymous]
    public ActionResult<SolicitudAdopcionResponseDto> CreatePublica([FromBody] SolicitudPublicaRequestDto request)
    {
        if (User.UsuarioId() is not int usuarioId)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Iniciá sesión",
                detail: "Para enviar una solicitud de adopción tenés que iniciar sesión o crear una cuenta.");

        var datos = request.ADominio();
        datos.Email ??= User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var creada = _solicitudService.CrearPublica(usuarioId, datos, request.MascotaId);
        return StatusCode(StatusCodes.Status201Created, SolicitudAdopcionResponseDto.Desde(creada));
    }

    // PUT: api/solicitudes-adopcion/3/estado   body: "EnEvaluacion" | "Aprobada" | "Rechazada" | "Cancelada"
    [HttpPut("{id:int}/estado")]
    public ActionResult<SolicitudAdopcionResponseDto> UpdateEstado(int id, [FromBody] EstadoSolicitud nuevoEstado) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.CambiarEstado(id, nuevoEstado)));

    // PUT: api/solicitudes-adopcion/3/aprobar  (verifica que la mascota siga disponible; la reserva)
    [HttpPut("{id:int}/aprobar")]
    public ActionResult<SolicitudAdopcionResponseDto> Aprobar(int id) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.Aprobar(id)));

    // PUT: api/solicitudes-adopcion/3/rechazar   body opcional: { "motivo": "..." } (le llega a la persona en su buzón)
    [HttpPut("{id:int}/rechazar")]
    public ActionResult<SolicitudAdopcionResponseDto> Rechazar(int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] MotivoDto? body) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.Rechazar(id, body?.Motivo)));

    // PUT: api/solicitudes-adopcion/3/cancelar    body opcional: { "motivo": "..." }
    [HttpPut("{id:int}/cancelar")]
    public ActionResult<SolicitudAdopcionResponseDto> Cancelar(int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] MotivoDto? body) =>
        Ok(SolicitudAdopcionResponseDto.Desde(_solicitudService.Cancelar(id, body?.Motivo)));

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _solicitudService.Eliminar(id);
        return NoContent();
    }
}
