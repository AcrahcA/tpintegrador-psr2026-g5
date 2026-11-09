namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _mascotaService;

    public MascotasController(IMascotaService mascotaService)
    {
        _mascotaService = mascotaService;
    }

    // GET: api/mascotas
    [HttpGet]
    public ActionResult<List<MascotaResponseDto>> Get()
    {
        var mascotas = _mascotaService.Get();

        var response = mascotas.Select(m => new MascotaResponseDto(
            Id: m.Id,
            Nombre: m.Nombre,
            Especie: m.Especie,
            Raza: m.Raza,
            EdadAproximada: m.EdadAproximada,
            Sexo: m.Sexo,
            Tamaño: m.Tamaño,
            FechaIngreso: m.FechaIngreso,
            Estado: m.Estado
        )).ToList();

        return Ok(response);
    }

    // GET: api/mascotas/123
    [HttpGet("{id}")]
    public ActionResult<MascotaResponseDto> GetById(int id)
    {
        var m = _mascotaService.GetById(id);
        if (m is null) return NotFound();

        var response = new MascotaResponseDto(
            Id: m.Id,
            Nombre: m.Nombre,
            Especie: m.Especie,
            Raza: m.Raza,
            EdadAproximada: m.EdadAproximada,
            Sexo: m.Sexo,
            Tamaño: m.Tamaño,
            FechaIngreso: m.FechaIngreso,
            Estado: m.Estado
        );

        return Ok(response);
    }

    // POST: api/mascotas
    [HttpPost]
    public ActionResult<MascotaResponseDto> Create([FromBody] MascotaRequestDto request)
    {
        // Creamos la entidad usando su constructor oficial de dominio
        var nuevaMascota = new Mascota(
            nombre: request.Nombre,
            especie: request.Especie,
            edadAproximada: request.EdadAproximada,
            sexo: request.Sexo,
            tamaño: request.Tamaño,
            raza: request.Raza,
            estado: request.Estado
        );

        var creada = _mascotaService.Post(nuevaMascota);
        if (creada is null)
            return BadRequest("No se pudo registrar la mascota. El refugio ha alcanzado su capacidad máxima.");

        var response = new MascotaResponseDto(
            Id: creada.Id,
            Nombre: creada.Nombre,
            Especie: creada.Especie,
            Raza: creada.Raza,
            EdadAproximada: creada.EdadAproximada,
            Sexo: creada.Sexo,
            Tamaño: creada.Tamaño,
            FechaIngreso: creada.FechaIngreso,
            Estado: creada.Estado
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // PUT: api/mascotas/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoMascota nuevoEstado)
    {
        var actualizado = _mascotaService.PutEstado(id, nuevoEstado);
        if (!actualizado)
            return BadRequest("No se pudo cambiar el estado de la mascota. Si intenta habilitarla para adopción, verifique que no posea tratamientos médicos activos.");

        return NoContent();
    }

    // PUT: api/mascotas/123/cuidador/456
    [HttpPut("{id}/cuidador/{cuidadorId}")]
    public ActionResult AsignarCuidador(int id, int cuidadorId)
    {
        var asignado = _mascotaService.AsignarCuidador(id, cuidadorId);
        if (!asignado)
            return BadRequest("No se pudo asignar el cuidador. Verifique que la mascota y el cuidador existan, y que el cuidador tenga capacidad disponible.");

        return NoContent();
    }

    // GET: api/mascotas/123/disponibilidad-adopcion
    [HttpGet("{id}/disponibilidad-adopcion")]
    public ActionResult<object> GetDisponibilidadAdopcion(int id)
    {
        var mascota = _mascotaService.GetById(id);
        if (mascota is null) return NotFound();

        return Ok(new
        {
            mascotaId = id,
            disponibleParaAdopcion = _mascotaService.EstaDisponibleParaAdopcion(id),
            cumpleCondicionesSanitarias = _mascotaService.CumpleCondicionesSanitarias(id)
        });
    }

    // DELETE: api/mascotas/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _mascotaService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}