namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdoptantesController : ControllerBase
{
    private readonly IAdoptanteService _adoptanteService;

    public AdoptantesController(IAdoptanteService adoptanteService)
    {
        _adoptanteService = adoptanteService;
    }

    // GET: api/adoptantes
    [HttpGet]
    public ActionResult<List<AdoptanteResponseDto>> Get()
    {
        var adoptantes = _adoptanteService.Get();

        var response = adoptantes.Select(a => new AdoptanteResponseDto(
            Id: a.Id,
            Nombre: a.Nombre,
            Dni: a.Dni,
            Telefono: a.Telefono,
            Direccion: a.Direccion
        )).ToList();

        return Ok(response);
    }

    // GET: api/adoptantes/123
    [HttpGet("{id}")]
    public ActionResult<AdoptanteResponseDto> GetById(int id)
    {
        var adoptante = _adoptanteService.GetById(id);
        if (adoptante is null) return NotFound();

        var response = new AdoptanteResponseDto(
            Id: adoptante.Id,
            Nombre: adoptante.Nombre,
            Dni: adoptante.Dni,
            Telefono: adoptante.Telefono,
            Direccion: adoptante.Direccion
        );

        return Ok(response);
    }

    // POST: api/adoptantes
    [HttpPost]
    public ActionResult<AdoptanteResponseDto> Create([FromBody] AdoptanteRequestDto request)
    {
        var nuevoAdoptante = new Adoptante(
            nombre: request.Nombre,
            dni: request.Dni,
            telefono: request.Telefono,
            direccion: request.Direccion
        );

        var creado = _adoptanteService.Post(nuevoAdoptante);
        
        // Validamos si el servicio rechazó la creación (por DNI duplicado o datos incompletos)
        if (creado is null)
        {
            return BadRequest("No se pudo registrar el adoptante. Verifique que los datos sean correctos o que el DNI no esté duplicado.");
        }

        var response = new AdoptanteResponseDto(
            Id: creado.Id,
            Nombre: creado.Nombre,
            Dni: creado.Dni,
            Telefono: creado.Telefono,
            Direccion: creado.Direccion
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // DELETE: api/adoptantes/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _adoptanteService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}