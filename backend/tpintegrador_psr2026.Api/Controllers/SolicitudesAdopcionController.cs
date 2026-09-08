namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesAdopcionController : ControllerBase
{
    private readonly ISolicitudAdopcionService _solicitudService;
    private readonly IMascotaService _mascotaService;
    private readonly IAdoptanteService _adoptanteService;

    public SolicitudesAdopcionController(
        ISolicitudAdopcionService solicitudService,
        IMascotaService mascotaService,
        IAdoptanteService adoptanteService)
    {
        _solicitudService = solicitudService;
        _mascotaService = mascotaService;
        _adoptanteService = adoptanteService;
    }

    [HttpGet]
    public IActionResult ObtenerTodas()
    {
        var solicitudes = _solicitudService.ObtenerSolicitudes()
            .Select(s => MapearRespuesta(s));
        return Ok(solicitudes);
    }

    [HttpGet("pendientes")]
    public IActionResult ObtenerPendientes()
    {
        var pendientes = _solicitudService.ObtenerPendientes()
            .Select(s => MapearRespuesta(s));
        return Ok(pendientes);
    }

    [HttpGet("{id}")]
    public IActionResult ObtenerPorId(int id)
    {
        var solicitud = _solicitudService.BuscarSolicitud(id);
        if (solicitud is null) return NotFound();
        return Ok(MapearRespuesta(solicitud));
    }

    public record NuevaSolicitudRequest(int AdoptanteId, int MascotaId);

    [HttpPost]
    public IActionResult Crear([FromBody] NuevaSolicitudRequest request)
    {
        var creada = _solicitudService.RealizarSolicitud(request.AdoptanteId, request.MascotaId);
        if (creada is null)
            return BadRequest("La mascota no existe o no se encuentra disponible para adopcion.");

        var respuesta = MapearRespuesta(creada);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, respuesta);
    }

    [HttpPut("{id}/estado")]
    public ActionResult CambiarEstado(int id, [FromBody] EstadoSolicitud nuevoEstado)
    {
        var actualizado = _solicitudService.CambiarEstado(id, nuevoEstado);
        if (!actualizado) 
            return BadRequest("No se pudo cambiar el estado de la solicitud. Verifique que la solicitud exista y que la mascota continue disponible.");
            
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Eliminar(int id)
    {
        var eliminado = _solicitudService.EliminarSolicitud(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }

    // Método auxiliar para armar el objeto limpio de salida
    private object MapearRespuesta(SolicitudAdopcion solicitud)
    {
        var adoptante = solicitud.Adoptante ?? _adoptanteService.BuscarAdoptante(solicitud.AdoptanteId);
        var mascota = solicitud.Mascota ?? _mascotaService.BuscarMascota(solicitud.MascotaId);

        return new
        {
            id = solicitud.Id,
            adoptanteId = solicitud.AdoptanteId,
            nombreAdoptante = adoptante?.Nombre ?? "Desconocido",
            mascotaId = solicitud.MascotaId,
            nombreMascota = mascota?.Nombre ?? "Desconocido",
            fechaSolicitud = solicitud.FechaSolicitud,
            estado = solicitud.Estado.ToString()
        };
    }
}