namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class RefugioController : ControllerBase
{
    private readonly IRefugioService _refugioService;

    public RefugioController(IRefugioService refugioService)
    {
        _refugioService = refugioService;
    }

    // GET: api/refugio
    [HttpGet]
    public ActionResult<RefugioResponseDto> Get()
    {
        var refugio = _refugioService.Get();
        if (refugio is null) return NotFound();

        var response = new RefugioResponseDto(
            Id: refugio.Id,
            Nombre: refugio.Nombre,
            CapacidadMaxima: refugio.CapacidadMaxima
        );

        return Ok(response);
    }

    // POST: api/refugio
    [HttpPost]
    public ActionResult<RefugioResponseDto> Post([FromBody] RefugioRequestDto request)
    {
        // Creamos la entidad utilizando su constructor oficial de dominio
        var nuevoRefugio = new Refugio(
            nombre: request.Nombre,
            capacidadMaxima: request.CapacidadMaxima
        );

        var configurado = _refugioService.Post(nuevoRefugio);

        var response = new RefugioResponseDto(
            Id: configurado.Id,
            Nombre: configurado.Nombre,
            CapacidadMaxima: configurado.CapacidadMaxima
        );

        return Ok(response);
    }

    // GET: api/refugio/capacidad
    [HttpGet("capacidad")]
    public ActionResult<object> GetCapacidad()
    {
        var refugio = _refugioService.Get();
        if (refugio is null) return NotFound();

        var ocupadas = _refugioService.ContarMascotasActuales();

        return Ok(new
        {
            capacidadMaxima = refugio.CapacidadMaxima,
            mascotasActuales = ocupadas,
            plazasDisponibles = refugio.CapacidadMaxima - ocupadas,
            tieneLugarDisponible = _refugioService.TieneLugarDisponible()
        });
    }
}