namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _mascotaService;
    private readonly IMascotaFotoService _fotoService;

    public MascotasController(IMascotaService mascotaService, IMascotaFotoService fotoService)
    {
        _mascotaService = mascotaService;
        _fotoService = fotoService;
    }

    // GET: api/mascotas
    [HttpGet]
    public ActionResult<List<MascotaResponseDto>> Get()
    {
        var mascotas = _mascotaService.Get();

        var response = mascotas.Select(m => new MascotaResponseDto(
            Id: m.Id,
            Nombre: m.Nombre,
            Especie: m.Especie,
            Raza: m.Raza,
            EdadAproximada: m.EdadAproximada,
            Sexo: m.Sexo,
            Tamaño: m.Tamaño,
            FechaIngreso: m.FechaIngreso,
            Estado: m.Estado,
            CuidadorId: m.CuidadorId,
            AvisoRescateOrigenId: m.AvisoRescateOrigenId,
            FotoUrl: m.FotoUrl
        )).ToList();

        return Ok(response);
    }

    // GET: api/mascotas/123
    [HttpGet("{id}")]
    public ActionResult<MascotaResponseDto> GetById(int id)
    {
        var m = _mascotaService.GetById(id);
        if (m is null) return NotFound();

        var response = new MascotaResponseDto(
            Id: m.Id,
            Nombre: m.Nombre,
            Especie: m.Especie,
            Raza: m.Raza,
            EdadAproximada: m.EdadAproximada,
            Sexo: m.Sexo,
            Tamaño: m.Tamaño,
            FechaIngreso: m.FechaIngreso,
            Estado: m.Estado,
            CuidadorId: m.CuidadorId,
            AvisoRescateOrigenId: m.AvisoRescateOrigenId,
            FotoUrl: m.FotoUrl
        );

        return Ok(response);
    }

    // POST: api/mascotas
    [HttpPost]
    public ActionResult<MascotaResponseDto> Create([FromBody] MascotaRequestDto request)
    {
        // Creamos la entidad usando su constructor oficial de dominio
        var nuevaMascota = new Mascota(
            nombre: request.Nombre,
            especie: request.Especie,
            edadAproximada: request.EdadAproximada,
            sexo: request.Sexo,
            tamaño: request.Tamaño,
            raza: request.Raza,
            estado: request.Estado
        );

        var creada = _mascotaService.Post(nuevaMascota);
        if (creada is null)
            return BadRequest("No se pudo registrar la mascota. El refugio ha alcanzado su capacidad máxima.");

        var response = new MascotaResponseDto(
            Id: creada.Id,
            Nombre: creada.Nombre,
            Especie: creada.Especie,
            Raza: creada.Raza,
            EdadAproximada: creada.EdadAproximada,
            Sexo: creada.Sexo,
            Tamaño: creada.Tamaño,
            FechaIngreso: creada.FechaIngreso,
            Estado: creada.Estado,
            CuidadorId: creada.CuidadorId,
            AvisoRescateOrigenId: creada.AvisoRescateOrigenId,
            FotoUrl: creada.FotoUrl
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] MascotaRequestDto request)
    {
        var datos = new Mascota(request.Nombre, request.Especie, request.EdadAproximada,
            request.Sexo, request.Tamaño, request.Raza);
        return _mascotaService.Actualizar(id, datos) ? NoContent() : NotFound();
    }

    // POST multipart/form-data: api/Mascotas/123/foto
    // Actualiza la foto de una mascota existente sin tocar sus datos ni su historial.
    [HttpPost("{id:int}/foto")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> SubirFoto(int id, [FromForm] IFormFile? foto, CancellationToken cancellationToken)
    {
        if (foto is null) return BadRequest("Seleccioná una fotografía.");
        try
        {
            var url = await _fotoService.GuardarAsync(id, foto, cancellationToken);
            if (url is null) return NotFound("La mascota no existe.");
            return Ok(new { fotoUrl = url });
        }
        catch (ArgumentException error)
        {
            return BadRequest(error.Message);
        }
    }

    // DELETE: api/Mascotas/123/foto (permite volver a 'sin foto').
    [HttpDelete("{id:int}/foto")]
    public async Task<IActionResult> QuitarFoto(int id, CancellationToken cancellationToken) =>
        await _fotoService.QuitarAsync(id, cancellationToken) ? NoContent() : NotFound();

    // GET: api/Mascotas/imagenes/GUID.jpg
    // Ruta de lectura pública: usada por el panel y el catálogo de adopciones.
    [HttpGet("imagenes/{nombreArchivo}")]
    public async Task<IActionResult> Imagen(string nombreArchivo)
    {
        var encontrado = await _fotoService.BuscarArchivoAsync(nombreArchivo);
        if (encontrado is null) return NotFound();
        Response.Headers["Cache-Control"] = "public, max-age=604800";
        return PhysicalFile(encontrado.Value.Ruta, encontrado.Value.Tipo);
    }

    // PUT: api/mascotas/123/estado
    [HttpPut("{id}/estado")]
    public ActionResult UpdateEstado(int id, [FromBody] EstadoMascota nuevoEstado)
    {
        var actualizado = _mascotaService.PutEstado(id, nuevoEstado);
        if (!actualizado)
            return BadRequest("No se pudo cambiar el estado de la mascota. Si intenta habilitarla para adopción, verifique que no posea tratamientos médicos activos.");

        return NoContent();
    }

    // PUT: api/mascotas/123/cuidador/456
    [HttpPut("{id}/cuidador/{cuidadorId}")]
    public ActionResult AsignarCuidador(int id, int cuidadorId)
    {
        var asignado = _mascotaService.AsignarCuidador(id, cuidadorId);
        if (!asignado)
            return BadRequest("No se pudo asignar el cuidador. Verifique que la mascota y el cuidador existan, y que el cuidador tenga capacidad disponible.");

        return NoContent();
    }

    [HttpDelete("{id}/cuidador")]
    public IActionResult QuitarCuidador(int id) =>
        _mascotaService.QuitarCuidador(id) ? NoContent() : BadRequest("No se pudo quitar el cuidador.");

    // GET: api/mascotas/123/disponibilidad-adopcion
    [HttpGet("{id}/disponibilidad-adopcion")]
    public ActionResult<object> GetDisponibilidadAdopcion(int id)
    {
        var mascota = _mascotaService.GetById(id);
        if (mascota is null) return NotFound();

        return Ok(new
        {
            mascotaId = id,
            disponibleParaAdopcion = _mascotaService.EstaDisponibleParaAdopcion(id),
            cumpleCondicionesSanitarias = _mascotaService.CumpleCondicionesSanitarias(id)
        });
    }

    // DELETE: api/mascotas/123
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var antiguaUrl = _mascotaService.GetById(id)?.FotoUrl;
        var eliminado = _mascotaService.Delete(id);
        if (!eliminado) return NotFound();
        await _fotoService.EliminarSiExisteAsync(antiguaUrl);
        return NoContent();
    }
}