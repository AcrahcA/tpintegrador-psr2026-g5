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
    private readonly ICuidadorService _cuidadorService;
    private readonly ITratamientoService _tratamientoService;
    private readonly IHistorialSanitarioService _historialService;
    private readonly IFotoMascotaStorage _fotos;

    public MascotasController(
        IMascotaService mascotaService,
        ICuidadorService cuidadorService,
        ITratamientoService tratamientoService,
        IHistorialSanitarioService historialService,
        IFotoMascotaStorage fotos)
    {
        _mascotaService = mascotaService;
        _cuidadorService = cuidadorService;
        _tratamientoService = tratamientoService;
        _historialService = historialService;
        _fotos = fotos;
    }

    // GET: api/mascotas            -> todas (incluye adoptadas: historial del refugio)
    // GET: api/mascotas?estado=X   -> mascotas según su estado
    [HttpGet]
    public ActionResult<List<MascotaResponseDto>> Get([FromQuery] EstadoMascota? estado)
    {
        var mascotas = estado is null ? _mascotaService.Get() : _mascotaService.GetPorEstado(estado.Value);
        return Ok(mascotas.Select(MascotaResponseDto.Desde).ToList());
    }

    // GET: api/mascotas/estado/DisponibleAdopcion
    [HttpGet("estado/{estado}")]
    public ActionResult<List<MascotaResponseDto>> GetPorEstado(EstadoMascota estado) =>
        Ok(_mascotaService.GetPorEstado(estado).Select(MascotaResponseDto.Desde).ToList());

    // GET: api/mascotas/alojadas   -> mascotas que ocupan plaza (no adoptadas)
    [HttpGet("alojadas")]
    public ActionResult<List<MascotaResponseDto>> GetAlojadas() =>
        Ok(_mascotaService.GetAlojadas().Select(MascotaResponseDto.Desde).ToList());

    // GET: api/mascotas/5
    [HttpGet("{id:int}")]
    public ActionResult<MascotaResponseDto> GetById(int id) =>
        Ok(MascotaResponseDto.Desde(_mascotaService.ObtenerPorId(id)));

    // POST: api/mascotas  (ingreso directo; valida la capacidad del refugio)
    [HttpPost]
    public ActionResult<MascotaResponseDto> Create([FromBody] MascotaRequestDto request)
    {
        var creada = _mascotaService.Registrar(request.ADominio());
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, MascotaResponseDto.Desde(creada));
    }

    // PUT: api/mascotas/5  (datos descriptivos; el estado se cambia con /estado)
    [HttpPut("{id:int}")]
    public ActionResult<MascotaResponseDto> Update(int id, [FromBody] MascotaRequestDto request)
    {
        var actual = _mascotaService.ObtenerPorId(id);
        var m = _mascotaService.Actualizar(id, request.Nombre, request.Especie ?? actual.Especie, request.Raza,
            request.EdadAproximada, request.Sexo, request.Tamaño);
        return Ok(MascotaResponseDto.Desde(m));
    }

    // PUT: api/mascotas/5/estado   body: "DisponibleAdopcion"
    [HttpPut("{id:int}/estado")]
    public ActionResult<MascotaResponseDto> UpdateEstado(int id, [FromBody] EstadoMascota nuevoEstado) =>
        Ok(MascotaResponseDto.Desde(_mascotaService.CambiarEstado(id, nuevoEstado)));

    // PUT: api/mascotas/5/cuidador/2
    [HttpPut("{id:int}/cuidador/{cuidadorId:int}")]
    public ActionResult<MascotaResponseDto> AsignarCuidador(int id, int cuidadorId) =>
        Ok(MascotaResponseDto.Desde(_cuidadorService.AsignarMascota(cuidadorId, id)));

    // DELETE: api/mascotas/5/cuidador
    [HttpDelete("{id:int}/cuidador")]
    public ActionResult<MascotaResponseDto> QuitarCuidador(int id) =>
        Ok(MascotaResponseDto.Desde(_cuidadorService.DesasignarMascota(id)));

    // GET: api/mascotas/5/disponibilidad-adopcion
    [HttpGet("{id:int}/disponibilidad-adopcion")]
    public ActionResult<EvaluacionAdopcion> GetDisponibilidadAdopcion(int id) => Ok(_mascotaService.EvaluarAdopcion(id));

    // GET: api/mascotas/5/tratamientos
    [HttpGet("{id:int}/tratamientos")]
    public ActionResult<List<TratamientoResponseDto>> GetTratamientos(int id) =>
        Ok(_tratamientoService.GetByMascotaId(id).Select(t => TratamientoResponseDto.Desde(t, id)).ToList());

    // GET: api/mascotas/5/historial  (se conserva también para mascotas adoptadas)
    [HttpGet("{id:int}/historial")]
    public ActionResult<HistorialSanitarioDetalleDto> GetHistorial(int id)
    {
        var historial = _historialService.ObtenerOCrearPorMascota(id);
        var tratamientos = _tratamientoService.GetByMascotaId(id);
        var motivos = RequisitosSanitarios.Evaluar(tratamientos);
        return Ok(new HistorialSanitarioDetalleDto(historial.Id, id, historial.Observaciones, motivos.Count == 0, motivos,
            tratamientos.Select(t => TratamientoResponseDto.Desde(t, id)).ToList()));
    }

    // POST: api/mascotas/5/foto  (multipart/form-data, campo "foto")
    [HttpPost("{id:int}/foto")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FotoMascotaStorage.TamañoMaximo + 64 * 1024)]
    public ActionResult<MascotaResponseDto> SubirFoto(int id, IFormFile foto)
    {
        var mascota = _mascotaService.ObtenerPorId(id);
        if (foto is null || foto.Length == 0 || foto.Length > FotoMascotaStorage.TamañoMaximo || !FotoMascotaStorage.EsTipoPermitido(foto.ContentType))
            throw new ReglaNegocioException("Seleccioná una foto JPG, PNG o WebP de hasta 5 MB.");

        var anterior = mascota.FotoUrl;
        using var stream = foto.OpenReadStream();
        var url = _fotos.Guardar(stream, foto.ContentType);
        var actualizada = _mascotaService.ActualizarFoto(id, url);
        _fotos.Eliminar(anterior);
        return Ok(MascotaResponseDto.Desde(actualizada));
    }

    // DELETE: api/mascotas/5/foto
    [HttpDelete("{id:int}/foto")]
    public ActionResult QuitarFoto(int id)
    {
        var mascota = _mascotaService.ObtenerPorId(id);
        var anterior = mascota.FotoUrl;
        _mascotaService.ActualizarFoto(id, null);
        _fotos.Eliminar(anterior);
        return NoContent();
    }

    // GET: api/mascotas/imagenes/{archivo}
    [HttpGet("imagenes/{archivo}")]
    public ActionResult GetImagen(string archivo)
    {
        var ruta = _fotos.RutaFisica(archivo);
        if (ruta is null || !System.IO.File.Exists(ruta)) return NotFound();
        var tipo = Path.GetExtension(ruta) switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" };
        return PhysicalFile(ruta, tipo);
    }

    // DELETE: api/mascotas/5  (sólo ingresos cargados por error; el resto se conserva en el historial)
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _mascotaService.Eliminar(id);
        return NoContent();
    }
}
