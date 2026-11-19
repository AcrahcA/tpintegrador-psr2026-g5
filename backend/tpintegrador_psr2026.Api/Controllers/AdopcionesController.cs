namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdopcionesController : ControllerBase
{
    private readonly IAdopcionService _adopcionService;

    public AdopcionesController(IAdopcionService adopcionService) => _adopcionService = adopcionService;

    // GET: api/adopciones  (adopciones realizadas)
    [HttpGet]
    public ActionResult<List<AdopcionResponseDto>> Get() =>
        Ok(_adopcionService.Get().Select(AdopcionResponseDto.Desde).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<AdopcionResponseDto> GetById(int id) =>
        Ok(AdopcionResponseDto.Desde(_adopcionService.GetById(id)
            ?? throw new RecursoNoEncontradoException($"No existe la adopción #{id}.")));

    // GET: api/adopciones/mascotas/5
    [HttpGet("mascotas/{mascotaId:int}")]
    public ActionResult<AdopcionResponseDto> GetPorMascota(int mascotaId) =>
        Ok(AdopcionResponseDto.Desde(_adopcionService.GetPorMascota(mascotaId)
            ?? throw new RecursoNoEncontradoException($"La mascota #{mascotaId} no registra una adopción.")));

    // POST: api/adopciones  (adopción definitiva a partir de una solicitud aprobada)
    [HttpPost]
    public ActionResult<AdopcionResponseDto> Create([FromBody] AdopcionRequestDto request)
    {
        var creada = _adopcionService.Registrar(request.SolicitudAdopcionId, request.Observaciones, request.FechaAdopcion);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, AdopcionResponseDto.Desde(creada));
    }
}
