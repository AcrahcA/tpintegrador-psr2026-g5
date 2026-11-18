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
    private readonly ITratamientoService _tratamientoService;

    public HistorialSanitarioController(IHistorialSanitarioService historialService, ITratamientoService tratamientoService)
    {
        _historialService = historialService;
        _tratamientoService = tratamientoService;
    }

    [HttpGet]
    public ActionResult<List<HistorialSanitarioResponseDto>> Get() =>
        Ok(_historialService.Get().Select(HistorialSanitarioResponseDto.Desde).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<HistorialSanitarioResponseDto> GetById(int id) =>
        Ok(HistorialSanitarioResponseDto.Desde(_historialService.ObtenerPorId(id)));

    // GET: api/historiales-sanitarios/mascotas/5  (historial completo, con tratamientos)
    [HttpGet("mascotas/{mascotaId:int}")]
    public ActionResult<HistorialSanitarioDetalleDto> GetByMascota(int mascotaId)
    {
        var historial = _historialService.ObtenerOCrearPorMascota(mascotaId);
        var tratamientos = _tratamientoService.GetByMascotaId(mascotaId);
        var motivos = RequisitosSanitarios.Evaluar(tratamientos);
        return Ok(new HistorialSanitarioDetalleDto(historial.Id, mascotaId, historial.Observaciones, motivos.Count == 0, motivos,
            tratamientos.Select(t => TratamientoResponseDto.Desde(t, mascotaId)).ToList()));
    }

    // POST: api/historiales-sanitarios  (si la mascota ya tiene historial, actualiza sus observaciones)
    [HttpPost]
    public ActionResult<HistorialSanitarioResponseDto> Create([FromBody] HistorialSanitarioRequestDto request)
    {
        var h = _historialService.Crear(request.MascotaId, request.Observaciones);
        return CreatedAtAction(nameof(GetById), new { id = h.Id }, HistorialSanitarioResponseDto.Desde(h));
    }

    // PUT: api/historiales-sanitarios/3  (observaciones)
    [HttpPut("{id:int}")]
    public ActionResult<HistorialSanitarioResponseDto> Update(int id, [FromBody] HistorialSanitarioRequestDto request) =>
        Ok(HistorialSanitarioResponseDto.Desde(_historialService.ActualizarObservaciones(id, request.Observaciones)));

    // El historial sanitario no se elimina: la mascota no debe perderlo aunque sea adoptada.
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id) =>
        throw new ReglaNegocioException("El historial sanitario no puede eliminarse: se conserva aunque la mascota sea adoptada.");
}
