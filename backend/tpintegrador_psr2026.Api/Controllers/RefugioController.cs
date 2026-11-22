namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Services.Seguridad;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Authorize(Roles = Roles.Administrador)]  // panel del refugio: sólo administradores (las acciones públicas se marcan aparte)
[Route("api/[controller]")]
public class RefugioController : ControllerBase
{
    private readonly IRefugioService _refugioService;
    private readonly IMascotaService _mascotaService;
    private readonly IAdopcionService _adopcionService;

    public RefugioController(IRefugioService refugioService, IMascotaService mascotaService, IAdopcionService adopcionService)
    {
        _refugioService = refugioService;
        _mascotaService = mascotaService;
        _adopcionService = adopcionService;
    }

    // GET: api/refugio  (público: nombre del refugio)
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<RefugioResponseDto> Get() => Ok(RefugioResponseDto.Desde(_refugioService.Obtener()));

    // GET: api/refugio/estadisticas  (público: números que muestra el sitio)
    [HttpGet("estadisticas")]
    [AllowAnonymous]
    public ActionResult<object> Estadisticas() => Ok(new
    {
        disponibles = _mascotaService.GetPorEstado(Domain.EstadoMascota.DisponibleAdopcion).Count,
        adopciones = _adopcionService.Get().Count
    });

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
