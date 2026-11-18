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

    public AvisosRescateController(IAvisoRescateService avisoService)
    {
        _avisoService = avisoService;
    }

    // GET: api/avisos-rescate
    [HttpGet]
    public ActionResult<List<AvisoRescateResponseDto>> Get()
    {
        var avisos = _avisoService.Get();

        var response = avisos.Select(a => new AvisoRescateResponseDto(
            Id: a.Id,
            Descripcion: a.Descripcion,
            TipoAnimal: a.TipoAnimal,
            Latitud: a.Latitud,
            Longitud: a.Longitud,
            FechaAviso: a.FechaAviso,
            ContactoNombre: a.ContactoNombre,
            ContactoTelefono: a.ContactoTelefono,
            EstadoAparente: a.EstadoAparente,
            NivelUrgencia: a.NivelUrgencia,
            Estado: a.Estado,
            MascotaGeneradaId: a.MascotaGeneradaId,
            Direccion: a.Direccion
        )).ToList();

        return Ok(response);
    }

    // GET: api/avisos-rescate/pendientes
    [HttpGet("pendientes")]
    public ActionResult<List<AvisoRescateResponseDto>> GetPendientes()
    {
        var avisos = _avisoService.GetPendientes();

        var response = avisos.Select(a => new AvisoRescateResponseDto(
            Id: a.Id,
            Descripcion: a.Descripcion,
            TipoAnimal: a.TipoAnimal,
            Latitud: a.Latitud,
            Longitud: a.Longitud,
            FechaAviso: a.FechaAviso,
            ContactoNombre: a.ContactoNombre,
            ContactoTelefono: a.ContactoTelefono,
            EstadoAparente: a.EstadoAparente,
            NivelUrgencia: a.NivelUrgencia,
            Estado: a.Estado,
            MascotaGeneradaId: a.MascotaGeneradaId,
            Direccion: a.Direccion
        )).ToList();

        return Ok(response);
    }

    // GET: api/avisos-rescate/123
    [HttpGet("{id}")]
    public ActionResult<AvisoRescateResponseDto> GetById(int id)
    {
        var a = _avisoService.GetById(id);
        if (a is null) return NotFound();

        var response = new AvisoRescateResponseDto(
            Id: a.Id,
            Descripcion: a.Descripcion,
            TipoAnimal: a.TipoAnimal,
            Latitud: a.Latitud,
            Longitud: a.Longitud,
            FechaAviso: a.FechaAviso,
            ContactoNombre: a.ContactoNombre,
            ContactoTelefono: a.ContactoTelefono,
            EstadoAparente: a.EstadoAparente,
            NivelUrgencia: a.NivelUrgencia,
            Estado: a.Estado,
            MascotaGeneradaId: a.MascotaGeneradaId,
            Direccion: a.Direccion
        );

        return Ok(response);
    }

    // POST: api/avisos-rescate
    [HttpPost]
    public ActionResult<AvisoRescateResponseDto> Create([FromBody] AvisoRescateRequestDto request)
    {
        // Creamos la entidad usando su constructor oficial de dominio
        var nuevoAviso = new AvisoRescate(
            descripcion: request.Descripcion,
            tipoAnimal: request.TipoAnimal,
            latitud: request.Latitud,
            longitud: request.Longitud,
            contactoNombre: request.ContactoNombre,
            estadoAparente: request.EstadoAparente,
            nivelUrgencia: request.NivelUrgencia,
            contactoTelefono: request.ContactoTelefono,
            direccion: request.Direccion
        );

        var creado = _avisoService.Post(nuevoAviso);

        var response = new AvisoRescateResponseDto(
            Id: creado.Id,
            Descripcion: creado.Descripcion,
            TipoAnimal: creado.TipoAnimal,
            Latitud: creado.Latitud,
            Longitud: creado.Longitud,
            FechaAviso: creado.FechaAviso,
            ContactoNombre: creado.ContactoNombre,
            ContactoTelefono: creado.ContactoTelefono,
            EstadoAparente: creado.EstadoAparente,
            NivelUrgencia: creado.NivelUrgencia,
            Estado: creado.Estado,
            MascotaGeneradaId: creado.MascotaGeneradaId,
            Direccion: creado.Direccion
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] AvisoRescateRequestDto request)
    {
        var datos = new AvisoRescate(request.Descripcion, request.TipoAnimal,
            request.Latitud, request.Longitud, request.ContactoNombre,
            request.EstadoAparente, request.NivelUrgencia, request.ContactoTelefono,
            direccion: request.Direccion);
        return _avisoService.Actualizar(id, datos) ? NoContent() : BadRequest("No se pudo editar el aviso.");
    }

    // PUT: api/avisos-rescate/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoAviso nuevoEstado)
    {
        var actualizado = _avisoService.PutEstado(id, nuevoEstado);
        if (!actualizado) return NotFound();
        return NoContent();
    }

    // POST: api/avisos-rescate/123/mascotas
    // (Nota: Si al atender el rescate querés enviar un MascotaRequestDto en lugar del dominio Mascota, se puede adaptar también aquí)
    [HttpPost("{id}/mascotas")]
    public ActionResult<MascotaResponseDto> AtenderYGenerarMascota(int id, [FromBody] MascotaRequestDto datosMascotaDto)
    {
        // Mapeamos el DTO de request de mascota a su entidad de dominio para el servicio
        var datosMascota = new Mascota(
            nombre: datosMascotaDto.Nombre,
            especie: datosMascotaDto.Especie,
            edadAproximada: datosMascotaDto.EdadAproximada,
            sexo: datosMascotaDto.Sexo,
            tamaño: datosMascotaDto.Tamaño,
            raza: datosMascotaDto.Raza,
            estado: datosMascotaDto.Estado
        );

        var mascotaGenerada = _avisoService.AtenderAvisoYGenerarMascota(id, datosMascota);
        if (mascotaGenerada is null)
            return BadRequest("No fue posible atender el aviso: asegúrese de que el aviso esté en estado 'Aceptado' y que el refugio tenga capacidad disponible.");

        // Mapeamos la mascota resultante al ResponseDto
        var response = new MascotaResponseDto(
            Id: mascotaGenerada.Id,
            Nombre: mascotaGenerada.Nombre,
            Especie: mascotaGenerada.Especie,
            Raza: mascotaGenerada.Raza,
            EdadAproximada: mascotaGenerada.EdadAproximada,
            Sexo: mascotaGenerada.Sexo,
            Tamaño: mascotaGenerada.Tamaño,
            FechaIngreso: mascotaGenerada.FechaIngreso,
            Estado: mascotaGenerada.Estado
        );

        return Ok(response);
    }

    // DELETE: api/avisos-rescate/123
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var eliminado = _avisoService.Delete(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}