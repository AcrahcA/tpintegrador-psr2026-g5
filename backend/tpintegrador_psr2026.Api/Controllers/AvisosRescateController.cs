namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/avisos-rescate")]
public class AvisosRescateController : ControllerBase
{
    private readonly IAvisoRescateService _avisoService;
    private readonly MascotaDtoMapper _mapper;

    public AvisosRescateController(IAvisoRescateService avisoService, MascotaDtoMapper mapper)
    {
        _avisoService = avisoService;
        _mapper = mapper;
    }

    // GET: api/avisos-rescate  |  api/avisos-rescate?estado=Aceptado
    [HttpGet]
    public ActionResult<List<AvisoRescateResponseDto>> Get([FromQuery] EstadoAviso? estado)
    {
        var avisos = estado is null ? _avisoService.Get() : _avisoService.GetPorEstado(estado.Value);
        return Ok(avisos.Select(AvisoRescateResponseDto.Desde).ToList());
    }

    // GET: api/avisos-rescate/pendientes  (Reportado o EnEvaluacion, por urgencia)
    [HttpGet("pendientes")]
    public ActionResult<List<AvisoRescateResponseDto>> GetPendientes() =>
        Ok(_avisoService.GetPendientes().Select(AvisoRescateResponseDto.Desde).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<AvisoRescateResponseDto> GetById(int id) =>
        Ok(AvisoRescateResponseDto.Desde(_avisoService.ObtenerPorId(id)));

    // POST: api/avisos-rescate  (registro temporal del aviso; queda "Reportado")
    [HttpPost]
    public ActionResult<AvisoRescateResponseDto> Create([FromBody] AvisoRescateRequestDto request)
    {
        var creado = _avisoService.Registrar(request.ADominio());
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, AvisoRescateResponseDto.Desde(creado));
    }

    [HttpPut("{id:int}")]
    public ActionResult<AvisoRescateResponseDto> Update(int id, [FromBody] AvisoRescateRequestDto request) =>
        Ok(AvisoRescateResponseDto.Desde(_avisoService.Actualizar(id, request.ADominio())));

    // PUT: api/avisos-rescate/5/estado   body: "EnEvaluacion"
    [HttpPut("{id:int}/estado")]
    public ActionResult<AvisoRescateResponseDto> UpdateEstado(int id, [FromBody] EstadoAviso nuevoEstado) =>
        Ok(AvisoRescateResponseDto.Desde(_avisoService.CambiarEstado(id, nuevoEstado)));

    // PUT: api/avisos-rescate/5/aceptar  (verifica que haya lugar en el refugio)
    [HttpPut("{id:int}/aceptar")]
    public ActionResult<AvisoRescateResponseDto> Aceptar(int id) =>
        Ok(AvisoRescateResponseDto.Desde(_avisoService.Aceptar(id)));

    // PUT: api/avisos-rescate/5/descartar
    [HttpPut("{id:int}/descartar")]
    public ActionResult<AvisoRescateResponseDto> Descartar(int id) =>
        Ok(AvisoRescateResponseDto.Desde(_avisoService.Descartar(id)));

    // POST: api/avisos-rescate/5/mascotas  (rescate realizado: ingresa la mascota y el aviso pasa a Atendido)
    [HttpPost("{id:int}/mascotas")]
    public ActionResult<MascotaResponseDto> AtenderYGenerarMascota(int id, [FromBody] MascotaRequestDto datosMascota)
    {
        var aviso = _avisoService.ObtenerPorId(id);
        var mascota = _avisoService.AtenderAvisoYGenerarMascota(id, datosMascota.ADominio(aviso.TipoAnimal));
        return CreatedAtAction(nameof(MascotasController.GetById), "Mascotas", new { id = mascota.Id }, _mapper.Uno(mascota));
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _avisoService.Eliminar(id);
        return NoContent();
    }
}
