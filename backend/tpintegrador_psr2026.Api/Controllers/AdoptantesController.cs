namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AdoptantesController : ControllerBase
{
    private readonly IAdoptanteService _adoptanteService;

    public AdoptantesController(IAdoptanteService adoptanteService) => _adoptanteService = adoptanteService;

    [HttpGet]
    public ActionResult<List<AdoptanteResponseDto>> Get() =>
        Ok(_adoptanteService.Get().Select(AdoptanteResponseDto.Desde).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<AdoptanteResponseDto> GetById(int id) =>
        Ok(AdoptanteResponseDto.Desde(_adoptanteService.ObtenerPorId(id)));

    // POST: api/adoptantes  (registrar posible adoptante; DNI único)
    [HttpPost]
    public ActionResult<AdoptanteResponseDto> Create([FromBody] AdoptanteRequestDto request)
    {
        var creado = _adoptanteService.Registrar(request.ADominio());
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, AdoptanteResponseDto.Desde(creado));
    }

    [HttpPut("{id:int}")]
    public ActionResult<AdoptanteResponseDto> Update(int id, [FromBody] AdoptanteRequestDto request) =>
        Ok(AdoptanteResponseDto.Desde(_adoptanteService.Actualizar(id, request.ADominio())));

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _adoptanteService.Eliminar(id);
        return NoContent();
    }
}
