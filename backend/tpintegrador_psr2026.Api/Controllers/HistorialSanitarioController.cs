namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/historiales-sanitarios")]
public class HistorialSanitarioController : ControllerBase
{
    private readonly IHistorialSanitarioService _historialService;

    public HistorialSanitarioController(IHistorialSanitarioService historialService)
    {
        _historialService = historialService;
    }

    // GET: api/historiales-sanitarios
    [HttpGet]
    public ActionResult<List<HistorialSanitarioResponseDto>> Get()
    {
        var historiales = _historialService.Get();

        var response = historiales.Select(h => new HistorialSanitarioResponseDto(
            Id: h.Id,
            MascotaId: h.MascotaId,
            Observaciones: h.Observaciones
        )).ToList();

        return Ok(response);
    }

    // GET: api/historiales-sanitarios/123
    [HttpGet("{id}")]
    public ActionResult<HistorialSanitarioResponseDto> GetById(int id)
    {
        var h = _historialService.GetById(id);
        if (h is null) return NotFound();

        var response = new HistorialSanitarioResponseDto(
            Id: h.Id,
            MascotaId: h.MascotaId,
            Observaciones: h.Observaciones
        );

        return Ok(response);
    }

    // GET: api/historiales-sanitarios/mascotas/123
    [HttpGet("mascotas/{mascotaId}")]
    public ActionResult<HistorialSanitarioResponseDto> GetByMascota(int mascotaId)
    {
        var h = _historialService.GetByMascotaId(mascotaId);
        if (h is null) return NotFound("No se encontró historial sanitario para esa mascota.");

        var response = new HistorialSanitarioResponseDto(
            Id: h.Id,
            MascotaId: h.MascotaId,
            Observaciones: h.Observaciones
        );

        return Ok(response);
    }

    // POST: api/historiales-sanitarios
    [HttpPost]
    public ActionResult<HistorialSanitarioResponseDto> Create([FromBody] HistorialSanitarioRequestDto request)
    {
        // Creamos la entidad utilizando su constructor de dominio
        var nuevoHistorial = new HistorialSanitario(
            mascotaId: request.MascotaId,
            observaciones: request.Observaciones
        );

        var creado = _historialService.Post(nuevoHistorial);

        var response = new HistorialSanitarioResponseDto(
            Id: creado.Id,
            MascotaId: creado.MascotaId,
            Observaciones: creado.Observaciones
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT: api/historiales-sanitarios/123
    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] HistorialSanitarioRequestDto request)
    {
        // Mapeamos el DTO de request a la entidad de dominio para pasársela al servicio
        var historialActualizado = new HistorialSanitario(
            mascotaId: request.MascotaId,
            observaciones: request.Observaciones
        );

        var actualizado = _historialService.Put(id, historialActualizado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // DELETE: api/historiales-sanitarios/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _historialService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}