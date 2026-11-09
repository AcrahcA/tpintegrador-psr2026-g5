namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class TratamientosController : ControllerBase
{
    private readonly ITratamientoService _tratamientoService;

    public TratamientosController(ITratamientoService tratamientoService)
    {
        _tratamientoService = tratamientoService;
    }

    // GET: api/tratamientos
    [HttpGet]
    public ActionResult<List<TratamientoResponseDto>> Get()
    {
        var tratamientos = _tratamientoService.Get();

        var response = tratamientos.Select(t => new TratamientoResponseDto(
            Id: t.Id,
            HistorialSanitarioId: t.HistorialSanitarioId,
            Tipo: t.Tipo,
            Descripcion: t.Descripcion,
            FechaInicio: t.FechaInicio,
            FechaFin: t.FechaFin,
            Estado: t.Estado
        )).ToList();

        return Ok(response);
    }

    // GET: api/tratamientos/mascotas/123
    [HttpGet("mascotas/{mascotaId}")]
    public ActionResult<List<TratamientoResponseDto>> GetByMascota(int mascotaId)
    {
        var tratamientos = _tratamientoService.GetByMascotaId(mascotaId);

        var response = tratamientos.Select(t => new TratamientoResponseDto(
            Id: t.Id,
            HistorialSanitarioId: t.HistorialSanitarioId,
            Tipo: t.Tipo,
            Descripcion: t.Descripcion,
            FechaInicio: t.FechaInicio,
            FechaFin: t.FechaFin,
            Estado: t.Estado
        )).ToList();

        return Ok(response);
    }

    // GET: api/tratamientos/123
    [HttpGet("{id}")]
    public ActionResult<TratamientoResponseDto> GetById(int id)
    {
        var t = _tratamientoService.GetById(id);
        if (t is null) return NotFound();

        var response = new TratamientoResponseDto(
            Id: t.Id,
            HistorialSanitarioId: t.HistorialSanitarioId,
            Tipo: t.Tipo,
            Descripcion: t.Descripcion,
            FechaInicio: t.FechaInicio,
            FechaFin: t.FechaFin,
            Estado: t.Estado
        );

        return Ok(response);
    }

    // POST: api/tratamientos/mascotas/123
    [HttpPost("mascotas/{mascotaId}")]
    public ActionResult<TratamientoResponseDto> Create(int mascotaId, [FromBody] TratamientoRequestDto request)
    {
        // Creamos la entidad usando su constructor de dominio oficial
        var nuevoTratamiento = new Tratamiento(
            historialSanitarioId: request.HistorialSanitarioId,
            tipo: request.Tipo,
            descripcion: request.Descripcion,
            fechaInicio: request.FechaInicio,
            fechaFin: request.FechaFin,
            estado: request.Estado
        );

        var creado = _tratamientoService.Post(mascotaId, nuevoTratamiento);
        if (creado is null)
            return BadRequest("No se pudo registrar el tratamiento. Verifique que la mascota y su historial sanitario existan.");

        var response = new TratamientoResponseDto(
            Id: creado.Id,
            HistorialSanitarioId: creado.HistorialSanitarioId,
            Tipo: creado.Tipo,
            Descripcion: creado.Descripcion,
            FechaInicio: creado.FechaInicio,
            FechaFin: creado.FechaFin,
            Estado: creado.Estado
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT: api/tratamientos/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoTratamiento nuevoEstado)
    {
        var actualizado = _tratamientoService.PutEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // DELETE: api/tratamientos/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _tratamientoService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}