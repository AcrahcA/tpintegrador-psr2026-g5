namespace tpintegrador_psr2026.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.DTOs;
using tpintegrador_psr2026.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class CuidadoresController : ControllerBase
{
    private readonly ICuidadorService _cuidadorService;
    private readonly MascotaDtoMapper _mapper;

    public CuidadoresController(ICuidadorService cuidadorService, MascotaDtoMapper mapper)
    {
        _cuidadorService = cuidadorService;
        _mapper = mapper;
    }

    private CuidadorResponseDto Dto(Cuidador c) => CuidadorResponseDto.Desde(c, _cuidadorService.ObtenerCantidadAsignadas(c.Id));

    [HttpGet]
    public ActionResult<List<CuidadorResponseDto>> Get() => Ok(_cuidadorService.Get().Select(Dto).ToList());

    [HttpGet("{id:int}")]
    public ActionResult<CuidadorResponseDto> GetById(int id) => Ok(Dto(_cuidadorService.ObtenerPorId(id)));

    [HttpPost]
    public ActionResult<CuidadorResponseDto> Create([FromBody] CuidadorRequestDto request)
    {
        var creado = _cuidadorService.Registrar(
            new Cuidador(request.Nombre, request.CapacidadMaxima, request.Especializacion, request.Disponible ?? true));
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, Dto(creado));
    }

    [HttpPut("{id:int}")]
    public ActionResult<CuidadorResponseDto> Update(int id, [FromBody] CuidadorRequestDto request) =>
        Ok(Dto(_cuidadorService.Actualizar(id, request.Nombre, request.Especializacion, request.CapacidadMaxima, request.Disponible)));

    // GET: api/cuidadores/2/mascotas  (?incluirAdoptadas=true para ver también las ya adoptadas)
    [HttpGet("{id:int}/mascotas")]
    public ActionResult<List<MascotaResponseDto>> GetMascotas(int id, [FromQuery] bool incluirAdoptadas = false)
    {
        _cuidadorService.ObtenerPorId(id);
        return Ok(_mapper.Muchos(_cuidadorService.MascotasAsignadas(id, incluirAdoptadas)));
    }

    // PUT: api/cuidadores/2/mascotas/5  (valida disponibilidad, especialización y capacidad máxima)
    [HttpPut("{id:int}/mascotas/{mascotaId:int}")]
    public ActionResult<MascotaResponseDto> AsignarMascota(int id, int mascotaId) =>
        Ok(_mapper.Uno(_cuidadorService.AsignarMascota(id, mascotaId)));

    [HttpGet("{id:int}/disponibilidad")]
    public ActionResult<object> GetDisponibilidad(int id)
    {
        var cuidador = _cuidadorService.ObtenerPorId(id);
        return Ok(new
        {
            capacidadMaxima = cuidador.CapacidadMaxima,
            asignadasActualmente = _cuidadorService.ObtenerCantidadAsignadas(id),
            disponible = cuidador.Disponible,
            tiposQueAtiende = cuidador.TiposEspecializados().Count == 0
                ? Enum.GetValues<TipoAnimal>().ToList()
                : cuidador.TiposEspecializados().ToList(),
            tieneDisponibilidad = _cuidadorService.TieneDisponibilidad(id)
        });
    }

    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _cuidadorService.Eliminar(id);
        return NoContent();
    }
}
