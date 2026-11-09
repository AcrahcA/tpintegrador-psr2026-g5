using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdopcionesController : ControllerBase
{
    private readonly IAdopcionService _adopcionService;

    public AdopcionesController(IAdopcionService adopcionService)
    {
        _adopcionService = adopcionService;
    }

    // GET: api/adopciones
    [HttpGet]
    public ActionResult<List<AdopcionResponseDto>> Get()
    {
        var adopciones = _adopcionService.Get();

        // Mapeamos cada entidad de dominio a su ResponseDto correspondiente
        var response = adopciones.Select(a => new AdopcionResponseDto(
            Id: a.Id,
            SolicitudAdopcionId: a.SolicitudAdopcionId,
            FechaAdopcion: a.FechaAdopcion,
            Observaciones: a.Observaciones
        )).ToList();

        return Ok(response);
    }

    // GET: api/adopciones/123
    [HttpGet("{id}")]
    public ActionResult<AdopcionResponseDto> GetById(int id)
    {
        var adopcion = _adopcionService.GetById(id);
        if (adopcion is null) return NotFound();

        // Mapeamos al ResponseDto
        var response = new AdopcionResponseDto(
            Id: adopcion.Id,
            SolicitudAdopcionId: adopcion.SolicitudAdopcionId,
            FechaAdopcion: adopcion.FechaAdopcion,
            Observaciones: adopcion.Observaciones
        );

        return Ok(response);
    }

    // POST: api/adopciones
    [HttpPost]
    public ActionResult<AdopcionResponseDto> Create([FromBody] AdopcionRequestDto request)
    {
        // Llamamos al servicio pasando los datos del RequestDto (usando el constructor de la entidad o sus propiedades)
        var creada = _adopcionService.Post(request.SolicitudAdopcionId, request.Observaciones);
        if (creada is null)
            return BadRequest("No se pudo registrar la adopción. Verifique que la solicitud esté aprobada y la mascota disponible.");

        // Mapeamos la entidad creada al ResponseDto para devolverla limpiamente
        var response = new AdopcionResponseDto(
            Id: creada.Id,
            SolicitudAdopcionId: creada.SolicitudAdopcionId,
            FechaAdopcion: creada.FechaAdopcion,
            Observaciones: creada.Observaciones
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}