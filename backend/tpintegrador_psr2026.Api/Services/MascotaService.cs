namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

/// <summary>Resultado de evaluar si una mascota puede ser adoptada.</summary>
public record EvaluacionAdopcion(
    int MascotaId,
    EstadoMascota Estado,
    bool CumpleCondicionesSanitarias,
    bool DisponibleParaAdopcion,
    List<string> Motivos);

public interface IMascotaService
{
    List<Mascota> Get();
    List<Mascota> GetPorEstado(EstadoMascota estado);
    List<Mascota> GetAlojadas();
    Mascota? GetById(int id);
    Mascota ObtenerPorId(int id);
    Mascota Registrar(Mascota mascota, int? avisoRescateOrigenId = null);
    Mascota Actualizar(int id, string nombre, TipoAnimal especie, string? raza, int edadAproximada, Sexo sexo, Tamaño tamaño);
    Mascota CambiarEstado(int id, EstadoMascota nuevoEstado);
    bool CumpleCondicionesSanitarias(int mascotaId);
    EvaluacionAdopcion EvaluarAdopcion(int mascotaId);
    /// <summary>Lanza una excepción si la mascota no está en condiciones de recibir solicitudes o ser adoptada.</summary>
    void ValidarAptaParaAdopcion(Mascota mascota, bool permitirReservada);
    Mascota ActualizarFoto(int id, string? fotoUrl);
    void Eliminar(int id);
}

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IRefugioService _refugioService;
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public MascotaService(
        IMascotaRepository mascotaRepository,
        IRefugioService refugioService,
        ICuidadorRepository cuidadorRepository,
        IHistorialSanitarioRepository historialRepository,
        ITratamientoRepository tratamientoRepository,
        ISolicitudAdopcionRepository solicitudRepository,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _mascotaRepository = mascotaRepository;
        _refugioService = refugioService;
        _cuidadorRepository = cuidadorRepository;
        _historialRepository = historialRepository;
        _tratamientoRepository = tratamientoRepository;
        _solicitudRepository = solicitudRepository;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<Mascota> Get() => _mascotaRepository.Get();

    public List<Mascota> GetPorEstado(EstadoMascota estado) => _mascotaRepository.GetPorEstado(estado);

    public List<Mascota> GetAlojadas() => _mascotaRepository.GetAlojadas();

    public Mascota? GetById(int id) => _mascotaRepository.GetPorId(id);

    public Mascota ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe la mascota #{id}.");

    public Mascota Registrar(Mascota mascota, int? avisoRescateOrigenId = null)
    {
        mascota.Validar();

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            // Regla: no pueden ingresar mascotas si el refugio alcanzó su capacidad máxima
            _refugioService.ValidarCapacidadDisponible();

            mascota.RefugioId = _refugioService.Obtener().Id;
            mascota.Estado = EstadoMascota.Ingresada;
            mascota.FechaIngreso = DateTime.UtcNow;
            mascota.CuidadorId = null;
            mascota.AvisoRescateOrigenId = avisoRescateOrigenId;
            var creada = _mascotaRepository.Post(mascota);

            // Composición: toda mascota nace con su historial sanitario
            _historialRepository.Post(new HistorialSanitario(creada.Id, "Historial creado al ingresar al refugio."));
            return creada;
        });
    }

    public Mascota Actualizar(int id, string nombre, TipoAnimal especie, string? raza, int edadAproximada, Sexo sexo, Tamaño tamaño)
    {
        var mascota = ObtenerPorId(id);
        if (mascota.FueAdoptada)
            throw new ReglaNegocioException("Una mascota adoptada se conserva en el historial y no puede modificarse.");

        if (mascota.CuidadorId is { } cuidadorId && especie != mascota.Especie)
        {
            var cuidador = _cuidadorRepository.GetPorId(cuidadorId);
            if (cuidador is not null && !cuidador.PuedeAtender(especie))
                throw new ReglaNegocioException($"El cuidador asignado ({cuidador.Nombre}) no puede atender animales de tipo {especie}.");
        }

        mascota.Nombre = nombre?.Trim() ?? string.Empty;
        mascota.Especie = especie;
        mascota.Raza = string.IsNullOrWhiteSpace(raza) ? null : raza.Trim();
        mascota.EdadAproximada = edadAproximada;
        mascota.Sexo = sexo;
        mascota.Tamaño = tamaño;
        mascota.Validar();

        _mascotaRepository.Put(id, mascota);
        return mascota;
    }

    public Mascota CambiarEstado(int id, EstadoMascota nuevoEstado)
    {
        var mascota = ObtenerPorId(id);
        if (mascota.Estado == nuevoEstado) return mascota;

        if (mascota.FueAdoptada)
            throw new ReglaNegocioException("La mascota ya fue adoptada: es un estado final.");
        if (mascota.Estado == EstadoMascota.Reservada)
            throw new ReglaNegocioException("La mascota está reservada por una solicitud aprobada. Cancelá la solicitud o completá la adopción.");
        if (!Mascota.EsEstadoManual(nuevoEstado))
            throw new ReglaNegocioException(nuevoEstado switch
            {
                EstadoMascota.Reservada => "Una mascota pasa a 'Reservada' sólo al aprobar una solicitud de adopción.",
                EstadoMascota.Adoptada => "Una mascota pasa a 'Adoptada' sólo al registrar la adopción definitiva.",
                _ => "El estado 'Ingresada' sólo se asigna al momento del ingreso."
            });

        var tieneActivos = _tratamientoRepository.TieneTratamientosActivos(id);

        if (nuevoEstado == EstadoMascota.EnTratamiento && !tieneActivos)
            throw new ReglaNegocioException("Para pasar a 'En tratamiento' la mascota debe tener un tratamiento pendiente o en curso.");
        if (nuevoEstado == EstadoMascota.EnRecuperacion && tieneActivos)
            throw new ReglaNegocioException("La mascota todavía tiene tratamientos pendientes o en curso.");
        if (nuevoEstado == EstadoMascota.DisponibleAdopcion)
        {
            var motivos = MotivosSanitarios(id);
            if (motivos.Count > 0)
                throw new ReglaNegocioException("No cumple las condiciones sanitarias para adopción: " + string.Join(" ", motivos));
        }

        mascota.Estado = nuevoEstado;
        _mascotaRepository.Put(id, mascota);
        return mascota;
    }

    private List<string> MotivosSanitarios(int mascotaId) =>
        RequisitosSanitarios.Evaluar(_tratamientoRepository.GetPorMascota(mascotaId));

    public bool CumpleCondicionesSanitarias(int mascotaId) => MotivosSanitarios(mascotaId).Count == 0;

    public EvaluacionAdopcion EvaluarAdopcion(int mascotaId)
    {
        var mascota = ObtenerPorId(mascotaId);
        var motivos = MotivosSanitarios(mascotaId);
        var cumple = motivos.Count == 0;

        if (mascota.Estado != EstadoMascota.DisponibleAdopcion)
            motivos.Insert(0, $"Su estado actual es {mascota.Estado}; debe estar 'DisponibleAdopcion'.");

        return new EvaluacionAdopcion(mascotaId, mascota.Estado, cumple,
            mascota.Estado == EstadoMascota.DisponibleAdopcion && cumple, motivos);
    }

    public void ValidarAptaParaAdopcion(Mascota mascota, bool permitirReservada)
    {
        if (mascota.FueAdoptada)
            throw new ReglaNegocioException($"La mascota {mascota.Nombre} ya fue adoptada y no admite nuevas solicitudes.");

        var estadoValido = mascota.Estado == EstadoMascota.DisponibleAdopcion
                           || (permitirReservada && mascota.Estado == EstadoMascota.Reservada);
        if (!estadoValido)
            throw new ReglaNegocioException($"La mascota {mascota.Nombre} no está disponible para adopción (estado actual: {mascota.Estado}).");

        var motivos = MotivosSanitarios(mascota.Id);
        if (motivos.Count > 0)
            throw new ReglaNegocioException($"La mascota {mascota.Nombre} no cumple las condiciones sanitarias: " + string.Join(" ", motivos));
    }

    public Mascota ActualizarFoto(int id, string? fotoUrl)
    {
        var mascota = ObtenerPorId(id);
        mascota.FotoUrl = fotoUrl;
        _mascotaRepository.Put(id, mascota);
        return mascota;
    }

    public void Eliminar(int id)
    {
        var mascota = ObtenerPorId(id);

        // Regla: se conserva el historial. Sólo se puede borrar un ingreso cargado por error.
        if (mascota.Estado != EstadoMascota.Ingresada)
            throw new ReglaNegocioException("Sólo pueden eliminarse mascotas recién ingresadas. Las demás se conservan en el historial del refugio.");
        if (mascota.AvisoRescateOrigenId is not null)
            throw new ReglaNegocioException("La mascota proviene de un aviso de rescate atendido y debe conservarse en el historial.");
        if (_tratamientoRepository.GetPorMascota(id).Count > 0)
            throw new ReglaNegocioException("La mascota tiene tratamientos registrados en su historial sanitario y no puede eliminarse.");
        if (_solicitudRepository.GetPorMascota(id).Count > 0)
            throw new ReglaNegocioException("La mascota tiene solicitudes de adopción registradas y no puede eliminarse.");

        _mascotaRepository.Delete(id); // el historial sanitario vacío se elimina en cascada
    }
}
