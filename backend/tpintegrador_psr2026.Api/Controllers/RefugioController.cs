namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class RefugioController : ControllerBase
{
    private readonly IRefugioService _refugioService;

    public RefugioController(IRefugioService refugioService) => _refugioService = refugioService;

    [HttpGet]
    public ActionResult<RefugioResponseDto> Get() => Ok(RefugioResponseDto.Desde(_refugioService.Obtener()));

    // POST: api/refugio  (configura nombre y capacidad máxima)
    [HttpPost]
    public ActionResult<RefugioResponseDto> Post([FromBody] RefugioRequestDto request) =>
        Ok(RefugioResponseDto.Desde(_refugioService.Configurar(request.Nombre, request.CapacidadMaxima)));

    [HttpPut]
    public ActionResult<RefugioResponseDto> Put([FromBody] RefugioRequestDto request) => Post(request);

    // GET: api/refugio/capacidad
    [HttpGet("capacidad")]
    public ActionResult<CapacidadRefugioDto> GetCapacidad()
    {
        var refugio = _refugioService.Obtener();
        var ocupadas = _refugioService.ContarMascotasAlojadas();
        return Ok(new CapacidadRefugioDto(refugio.CapacidadMaxima, ocupadas,
            refugio.PlazasDisponibles(ocupadas), refugio.TieneLugarDisponible(ocupadas)));
    }
}
