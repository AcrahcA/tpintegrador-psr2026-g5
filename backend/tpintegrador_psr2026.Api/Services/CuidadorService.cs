namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface ICuidadorService
{
    List<Cuidador> Get();
    Cuidador? GetById(int id);
    Cuidador ObtenerPorId(int id);
    Cuidador Registrar(Cuidador cuidador);
    Cuidador Actualizar(int id, string nombre, string? especializacion, int capacidadMaxima, bool? disponible);
    void Eliminar(int id);
    List<Mascota> MascotasAsignadas(int cuidadorId, bool incluirAdoptadas = false);
    int ObtenerCantidadAsignadas(int cuidadorId);
    bool TieneDisponibilidad(int cuidadorId);
    Mascota AsignarMascota(int cuidadorId, int mascotaId);
    Mascota DesasignarMascota(int mascotaId);
}

public class CuidadorService : ICuidadorService
{
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IRefugioService _refugioService;

    public CuidadorService(ICuidadorRepository cuidadorRepository, IMascotaRepository mascotaRepository, IRefugioService refugioService)
    {
        _cuidadorRepository = cuidadorRepository;
        _mascotaRepository = mascotaRepository;
        _refugioService = refugioService;
    }

    public List<Cuidador> Get() => _cuidadorRepository.Get();

    public Cuidador? GetById(int id) => _cuidadorRepository.GetPorId(id);

    public Cuidador ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe el cuidador #{id}.");

    public Cuidador Registrar(Cuidador cuidador)
    {
        cuidador.Nombre = cuidador.Nombre?.Trim() ?? string.Empty;
        cuidador.Especializacion = string.IsNullOrWhiteSpace(cuidador.Especializacion) ? null : cuidador.Especializacion.Trim();
        cuidador.Validar();
        cuidador.RefugioId = _refugioService.Obtener().Id;
        return _cuidadorRepository.Post(cuidador);
    }

    public Cuidador Actualizar(int id, string nombre, string? especializacion, int capacidadMaxima, bool? disponible)
    {
        var cuidador = ObtenerPorId(id);
        var asignadas = MascotasAsignadas(id);

        if (capacidadMaxima < asignadas.Count)
            throw new ReglaNegocioException($"El cuidador tiene {asignadas.Count} mascotas asignadas: la capacidad no puede ser menor.");

        cuidador.Nombre = nombre?.Trim() ?? string.Empty;
        cuidador.Especializacion = string.IsNullOrWhiteSpace(especializacion) ? null : especializacion.Trim();
        cuidador.CapacidadMaxima = capacidadMaxima;
        if (disponible.HasValue) cuidador.Disponible = disponible.Value;
        cuidador.Validar();

        var noAtendibles = asignadas.Where(m => !cuidador.PuedeAtender(m.Especie)).Select(m => m.Nombre).ToList();
        if (noAtendibles.Count > 0)
            throw new ReglaNegocioException($"Con esa especialización no podría atender a: {string.Join(", ", noAtendibles)}. Reasignalas primero.");

        _cuidadorRepository.Put(id, cuidador);
        return cuidador;
    }

    public void Eliminar(int id)
    {
        ObtenerPorId(id);
        if (ObtenerCantidadAsignadas(id) > 0)
            throw new ReglaNegocioException("El cuidador tiene mascotas asignadas. Reasignalas antes de eliminarlo.");
        _cuidadorRepository.Delete(id);
    }

    public List<Mascota> MascotasAsignadas(int cuidadorId, bool incluirAdoptadas = false) =>
        _mascotaRepository.GetPorCuidador(cuidadorId, incluirAdoptadas);

    // Las mascotas adoptadas no se cuentan como carga del cuidador
    public int ObtenerCantidadAsignadas(int cuidadorId) => _mascotaRepository.ContarAsignadasACuidador(cuidadorId);

    public bool TieneDisponibilidad(int cuidadorId)
    {
        var cuidador = GetById(cuidadorId);
        return cuidador is not null && cuidador.Disponible && cuidador.TieneCapacidad(ObtenerCantidadAsignadas(cuidadorId));
    }

    public Mascota AsignarMascota(int cuidadorId, int mascotaId)
    {
        var cuidador = ObtenerPorId(cuidadorId);
        var mascota = _mascotaRepository.GetPorId(mascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");

        if (mascota.FueAdoptada)
            throw new ReglaNegocioException("La mascota ya fue adoptada y no requiere cuidador.");
        if (mascota.CuidadorId == cuidadorId)
            return mascota;
        if (!cuidador.Disponible)
            throw new ReglaNegocioException($"El cuidador {cuidador.Nombre} no se encuentra disponible.");
        if (!cuidador.PuedeAtender(mascota.Especie))
            throw new ReglaNegocioException($"El cuidador {cuidador.Nombre} (especialización: {cuidador.Especializacion}) no puede atender animales de tipo {mascota.Especie}.");

        var asignadas = ObtenerCantidadAsignadas(cuidadorId);
        if (!cuidador.TieneCapacidad(asignadas))
            throw new ReglaNegocioException($"El cuidador {cuidador.Nombre} alcanzó su capacidad máxima ({cuidador.CapacidadMaxima} mascotas).");

        mascota.CuidadorId = cuidadorId;
        _mascotaRepository.Put(mascota.Id, mascota);
        return mascota;
    }

    public Mascota DesasignarMascota(int mascotaId)
    {
        var mascota = _mascotaRepository.GetPorId(mascotaId)
                      ?? throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");
        mascota.CuidadorId = null;
        _mascotaRepository.Put(mascota.Id, mascota);
        return mascota;
    }
}
