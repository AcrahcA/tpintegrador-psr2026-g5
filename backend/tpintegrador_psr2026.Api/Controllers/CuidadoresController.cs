namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly ICuidadorService _cuidadorService;

    public CuidadoresController(ICuidadorService cuidadorService)
    {
        _cuidadorService = cuidadorService;
    }

    // GET: api/cuidadores
    [HttpGet]
    public ActionResult<List<CuidadorResponseDto>> Get()
    {
        var cuidadores = _cuidadorService.Get();

        var response = cuidadores.Select(c => new CuidadorResponseDto(
            Id: c.Id,
            Nombre: c.Nombre,
            Especializacion: c.Especializacion,
            CapacidadMaxima: c.CapacidadMaxima
        )).ToList();

        return Ok(response);
    }

    // GET: api/cuidadores/123
    [HttpGet("{id}")]
    public ActionResult<CuidadorResponseDto> GetById(int id)
    {
        var c = _cuidadorService.GetById(id);
        if (c is null) return NotFound();

        var response = new CuidadorResponseDto(
            Id: c.Id,
            Nombre: c.Nombre,
            Especializacion: c.Especializacion,
            CapacidadMaxima: c.CapacidadMaxima
        );

        return Ok(response);
    }

    // POST: api/cuidadores
    [HttpPost]
    public ActionResult<CuidadorResponseDto> Create([FromBody] CuidadorRequestDto request)
    {
        var nuevoCuidador = new Cuidador(
            nombre: request.Nombre,
            capacidadMaxima: request.CapacidadMaxima,
            especializacion: request.Especializacion
        );

        var creado = _cuidadorService.Post(nuevoCuidador);

        var response = new CuidadorResponseDto(
            Id: creado.Id,
            Nombre: creado.Nombre,
            Especializacion: creado.Especializacion,
            CapacidadMaxima: creado.CapacidadMaxima
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT: api/cuidadores/123
    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] CuidadorRequestDto request)
    {
        var cuidadorActualizado = new Cuidador(
            nombre: request.Nombre,
            capacidadMaxima: request.CapacidadMaxima,
            especializacion: request.Especializacion
        );

        var actualizado = _cuidadorService.Put(id, cuidadorActualizado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // PUT: api/cuidadores/123/mascotas/456  <-- Reemplaza el antiguo post/método de asignación por este PUT limpio
    [HttpPut("{id}/mascotas/{mascotaId}")]
    public ActionResult AsignarMascota(int id, int mascotaId)
    {
        var asignado = _cuidadorService.AsignarMascota(id, mascotaId);
        if (!asignado)
            return BadRequest("No se pudo asignar la mascota. Verifique que el cuidador y la mascota existan, y que el cuidador no haya superado su capacidad máxima.");

        return NoContent();
    }

    // GET: api/cuidadores/123/disponibilidad
    [HttpGet("{id}/disponibilidad")]
    public ActionResult<object> GetDisponibilidad(int id)
    {
        var cuidador = _cuidadorService.GetById(id);
        if (cuidador is null) return NotFound();

        return Ok(new
        {
            capacidadMaxima = cuidador.CapacidadMaxima,
            asignadasActualmente = _cuidadorService.ObtenerCantidadAsignadas(id),
            tieneDisponibilidad = _cuidadorService.TieneDisponibilidad(id)
        });
    }

    // DELETE: api/cuidadores/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _cuidadorService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}