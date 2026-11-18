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
    private readonly IAdoptanteService _adoptanteService;

    public SolicitudesAdopcionController(ISolicitudAdopcionService solicitudService, IAdoptanteService adoptanteService)
    {
        _solicitudService = solicitudService;
        _adoptanteService = adoptanteService;
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

    // El formulario público no necesita consultar la lista de adoptantes.
    [HttpPost("publica")]
    public ActionResult<SolicitudAdopcionResponseDto> CrearPublica([FromBody] SolicitudPublicaRequestDto request)
    {
        if (request.MascotaId <= 0 || string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Dni))
            return BadRequest("Completá nombre, DNI, teléfono y mascota.");

        if (!_solicitudService.EsMascotaAptaParaAdopcion(request.MascotaId))
            return BadRequest("La mascota ya no está disponible para adopción.");

        var dni = new string(request.Dni.Where(char.IsDigit).ToArray());
        if (dni.Length < 7 || dni.Length > 9) return BadRequest("El DNI debe tener de 7 a 9 dígitos.");

        var adoptante = _adoptanteService.Get().FirstOrDefault(a => a.Dni == dni);
        if (adoptante is null)
        {
            adoptante = _adoptanteService.Post(new Adoptante(request.Nombre.Trim(), dni,
                request.Telefono.Trim(), request.Direccion));
            if (adoptante is null) return BadRequest("No se pudo registrar el adoptante.");
        }
        var solicitud = _solicitudService.Post(adoptante.Id, request.MascotaId);
        if (solicitud is null) return BadRequest("No se pudo registrar la solicitud.");
        return CreatedAtAction(nameof(GetById), new { id = solicitud.Id },
            new SolicitudAdopcionResponseDto(solicitud.Id, solicitud.AdoptanteId,
                solicitud.MascotaId, solicitud.FechaSolicitud, solicitud.Estado));
    }

    // PUT: api/solicitudes-adopcion/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoSolicitud nuevoEstado)
    {
        var actualizado = _solicitudService.PutEstado(id, nuevoEstado);
        if (!actualizado)
            return BadRequest("Transición de solicitud no permitida: para aprobar necesitás mascota disponible, controles sanitarios finalizados y ninguna reserva existente. Para liberar una reserva cancelá la solicitud aprobada.");

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