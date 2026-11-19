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
    private readonly IFotoMascotaService _fotoService;
    private readonly IFotoMascotaStorage _storage;
    private readonly MascotaDtoMapper _mapper;

    public MascotasController(
        IMascotaService mascotaService,
        ICuidadorService cuidadorService,
        ITratamientoService tratamientoService,
        IHistorialSanitarioService historialService,
        IFotoMascotaService fotoService,
        IFotoMascotaStorage storage,
        MascotaDtoMapper mapper)
    {
        _mascotaService = mascotaService;
        _cuidadorService = cuidadorService;
        _tratamientoService = tratamientoService;
        _historialService = historialService;
        _fotoService = fotoService;
        _storage = storage;
        _mapper = mapper;
    }

    // GET: api/mascotas            -> todas (incluye adoptadas: historial del refugio)
    // GET: api/mascotas?estado=X   -> mascotas según su estado
    [HttpGet]
    public ActionResult<List<MascotaResponseDto>> Get([FromQuery] EstadoMascota? estado)
    {
        var mascotas = estado is null ? _mascotaService.Get() : _mascotaService.GetPorEstado(estado.Value);
        return Ok(_mapper.Muchos(mascotas));
    }

    // GET: api/mascotas/estado/DisponibleAdopcion
    [HttpGet("estado/{estado}")]
    public ActionResult<List<MascotaResponseDto>> GetPorEstado(EstadoMascota estado) =>
        Ok(_mapper.Muchos(_mascotaService.GetPorEstado(estado)));

    // GET: api/mascotas/alojadas   -> mascotas que ocupan plaza (no adoptadas)
    [HttpGet("alojadas")]
    public ActionResult<List<MascotaResponseDto>> GetAlojadas() =>
        Ok(_mapper.Muchos(_mascotaService.GetAlojadas()));

    // GET: api/mascotas/5
    [HttpGet("{id:int}")]
    public ActionResult<MascotaResponseDto> GetById(int id) =>
        Ok(_mapper.Uno(_mascotaService.ObtenerPorId(id)));

    // POST: api/mascotas  (ingreso directo; valida la capacidad del refugio)
    [HttpPost]
    public ActionResult<MascotaResponseDto> Create([FromBody] MascotaRequestDto request)
    {
        var creada = _mascotaService.Registrar(request.ADominio());
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, _mapper.Uno(creada));
    }

    // PUT: api/mascotas/5  (datos descriptivos; el estado se cambia con /estado)
    [HttpPut("{id:int}")]
    public ActionResult<MascotaResponseDto> Update(int id, [FromBody] MascotaRequestDto request)
    {
        var actual = _mascotaService.ObtenerPorId(id);
        var m = _mascotaService.Actualizar(id, request.Nombre, request.Especie ?? actual.Especie, request.Raza,
            request.EdadAproximada, request.Sexo, request.Tamaño);
        return Ok(_mapper.Uno(m));
    }

    // PUT: api/mascotas/5/estado   body: "DisponibleAdopcion"
    [HttpPut("{id:int}/estado")]
    public ActionResult<MascotaResponseDto> UpdateEstado(int id, [FromBody] EstadoMascota nuevoEstado) =>
        Ok(_mapper.Uno(_mascotaService.CambiarEstado(id, nuevoEstado)));

    // PUT: api/mascotas/5/cuidador/2
    [HttpPut("{id:int}/cuidador/{cuidadorId:int}")]
    public ActionResult<MascotaResponseDto> AsignarCuidador(int id, int cuidadorId) =>
        Ok(_mapper.Uno(_cuidadorService.AsignarMascota(cuidadorId, id)));

    // DELETE: api/mascotas/5/cuidador
    [HttpDelete("{id:int}/cuidador")]
    public ActionResult<MascotaResponseDto> QuitarCuidador(int id) =>
        Ok(_mapper.Uno(_cuidadorService.DesasignarMascota(id)));

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

    // GET: api/mascotas/5/fotos  (galería completa, la primera es la principal)
    [HttpGet("{id:int}/fotos")]
    public ActionResult<List<FotoMascotaDto>> GetFotos(int id) =>
        Ok(_fotoService.Listar(id).Select(FotoMascotaDto.Desde).ToList());

    // POST: api/mascotas/5/fotos  (multipart/form-data: "fotos" = archivos, "descripciones" = texto alternativo de cada una)
    [HttpPost("{id:int}/fotos")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FotoMascotaService.MaximoPorCarga * FotoMascotaStorage.TamañoMaximo + 1024 * 1024)]
    public ActionResult<List<FotoMascotaDto>> SubirFotos(int id, [FromForm] List<IFormFile> fotos, [FromForm] List<string>? descripciones)
    {
        var streams = new List<Stream>();
        try
        {
            var archivos = new List<ArchivoFoto>();
            for (var i = 0; i < fotos.Count; i++)
            {
                var stream = fotos[i].OpenReadStream();
                streams.Add(stream);
                archivos.Add(new ArchivoFoto(stream, fotos[i].ContentType, fotos[i].Length,
                    descripciones is not null && i < descripciones.Count ? descripciones[i] : null));
            }
            var creadas = _fotoService.Agregar(id, archivos);
            return StatusCode(StatusCodes.Status201Created, creadas.Select(FotoMascotaDto.Desde).ToList());
        }
        finally
        {
            foreach (var s in streams) s.Dispose();
        }
    }

    // PUT: api/mascotas/5/fotos/3  { "descripcion": "...", "principal": true }
    [HttpPut("{id:int}/fotos/{fotoId:int}")]
    public ActionResult<FotoMascotaDto> ActualizarFoto(int id, int fotoId, [FromBody] FotoMascotaUpdateDto request) =>
        Ok(FotoMascotaDto.Desde(_fotoService.Actualizar(id, fotoId, request.Descripcion, request.Principal)));

    // PUT: api/mascotas/5/fotos/3/mover?desplazamiento=-1  (reordenar la galería)
    [HttpPut("{id:int}/fotos/{fotoId:int}/mover")]
    public ActionResult<List<FotoMascotaDto>> MoverFoto(int id, int fotoId, [FromQuery] int desplazamiento) =>
        Ok(_fotoService.Mover(id, fotoId, desplazamiento).Select(FotoMascotaDto.Desde).ToList());

    // DELETE: api/mascotas/5/fotos/3
    [HttpDelete("{id:int}/fotos/{fotoId:int}")]
    public ActionResult EliminarFoto(int id, int fotoId)
    {
        _fotoService.Eliminar(id, fotoId);
        return NoContent();
    }

    // GET: api/mascotas/imagenes/{archivo}
    [HttpGet("imagenes/{archivo}")]
    public ActionResult GetImagen(string archivo)
    {
        var ruta = _storage.RutaFisica(archivo);
        if (ruta is null || !System.IO.File.Exists(ruta)) return NotFound();
        var tipo = Path.GetExtension(ruta) switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" };
        Response.Headers.CacheControl = "public, max-age=604800, immutable";
        return PhysicalFile(ruta, tipo);
    }

    // DELETE: api/mascotas/5  (sólo ingresos cargados por error; el resto se conserva en el historial)
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        var fotos = _fotoService.Listar(id);
        _mascotaService.Eliminar(id);
        foreach (var f in fotos) _storage.Eliminar(f.Url);
        return NoContent();
    }
}
