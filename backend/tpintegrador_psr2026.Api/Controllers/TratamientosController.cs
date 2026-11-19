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

    public TratamientosController(ITratamientoService tratamientoService) => _tratamientoService = tratamientoService;

    private TratamientoResponseDto Dto(Tratamiento t) => TratamientoResponseDto.Desde(t, _tratamientoService.ObtenerMascotaId(t));

    [HttpGet]
    public ActionResult<List<TratamientoResponseDto>> Get() => Ok(_tratamientoService.Get().Select(Dto).ToList());

    // GET: api/tratamientos/mascotas/5
    [HttpGet("mascotas/{mascotaId:int}")]
    public ActionResult<List<TratamientoResponseDto>> GetByMascota(int mascotaId) =>
        Ok(_tratamientoService.GetByMascotaId(mascotaId).Select(t => TratamientoResponseDto.Desde(t, mascotaId)).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<TratamientoResponseDto> GetById(int id) => Ok(Dto(_tratamientoService.ObtenerPorId(id)));

    // POST: api/tratamientos/mascotas/5  (se agrega al historial sanitario de la mascota)
    [HttpPost("mascotas/{mascotaId:int}")]
    public ActionResult<TratamientoResponseDto> Create(int mascotaId, [FromBody] TratamientoRequestDto request)
    {
        var creado = _tratamientoService.Registrar(mascotaId, request.ADominio());
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, TratamientoResponseDto.Desde(creado, mascotaId));
    }

    // PUT: api/tratamientos/7  (tipo, descripción y fecha de inicio; el estado se cambia con /estado)
    [HttpPut("{id:int}")]
    public ActionResult<TratamientoResponseDto> Update(int id, [FromBody] TratamientoRequestDto request) =>
        Ok(Dto(_tratamientoService.Actualizar(id, request.Tipo, request.Descripcion, request.FechaInicio)));

    // PUT: api/tratamientos/7/estado   body: "Finalizado"
    [HttpPut("{id:int}/estado")]
    public ActionResult<TratamientoResponseDto> UpdateEstado(int id, [FromBody] EstadoTratamiento nuevoEstado) =>
        Ok(Dto(_tratamientoService.CambiarEstado(id, nuevoEstado)));

    // DELETE: api/tratamientos/7  (sólo tratamientos pendientes cargados por error)
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _tratamientoService.Eliminar(id);
        return NoContent();
    }
}
