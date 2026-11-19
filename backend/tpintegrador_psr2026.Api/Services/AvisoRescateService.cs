namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface IAvisoRescateService
{
    List<AvisoRescate> Get();
    List<AvisoRescate> GetPendientes();
    List<AvisoRescate> GetPorEstado(EstadoAviso estado);
    AvisoRescate? GetById(int id);
    AvisoRescate ObtenerPorId(int id);
    AvisoRescate Registrar(AvisoRescate aviso);
    AvisoRescate Actualizar(int id, AvisoRescate datos);
    AvisoRescate CambiarEstado(int id, EstadoAviso nuevoEstado);
    AvisoRescate Aceptar(int id);
    AvisoRescate Descartar(int id);
    /// <summary>Registra el rescate de un aviso aceptado: crea la mascota y marca el aviso como atendido.</summary>
    Mascota AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota);
    void Eliminar(int id);
}

public class AvisoRescateService : IAvisoRescateService
{
    private readonly IAvisoRescateRepository _avisoRepository;
    private readonly IMascotaService _mascotaService;
    private readonly IRefugioService _refugioService;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public AvisoRescateService(
        IAvisoRescateRepository avisoRepository,
        IMascotaService mascotaService,
        IRefugioService refugioService,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _avisoRepository = avisoRepository;
        _mascotaService = mascotaService;
        _refugioService = refugioService;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public List<AvisoRescate> Get() => _avisoRepository.Get();

    public List<AvisoRescate> GetPendientes() => _avisoRepository.GetPendientes();

    public List<AvisoRescate> GetPorEstado(EstadoAviso estado) => _avisoRepository.GetPorEstado(estado);

    public AvisoRescate? GetById(int id) => _avisoRepository.GetPorId(id);

    public AvisoRescate ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe el aviso de rescate #{id}.");

    private static void Validar(AvisoRescate aviso)
    {
        if (string.IsNullOrWhiteSpace(aviso.Descripcion))
            throw new ReglaNegocioException("La descripción del aviso es obligatoria.");
        if (string.IsNullOrWhiteSpace(aviso.ContactoNombre))
            throw new ReglaNegocioException("Los datos de contacto de quien realizó el reporte son obligatorios.");
        AvisoRescate.ValidarCoordenadas(aviso.Latitud, aviso.Longitud);
    }

    public AvisoRescate Registrar(AvisoRescate aviso)
    {
        Validar(aviso);
        aviso.FechaAviso = DateTime.UtcNow;
        aviso.Estado = EstadoAviso.Reportado;
        aviso.MascotaGeneradaId = null;
        return _avisoRepository.Post(aviso);
    }

    public AvisoRescate Actualizar(int id, AvisoRescate datos)
    {
        var aviso = ObtenerPorId(id);
        if (aviso.EstaCerrado)
            throw new ReglaNegocioException($"El aviso está {aviso.Estado} y ya no puede modificarse.");

        aviso.Descripcion = datos.Descripcion;
        aviso.TipoAnimal = datos.TipoAnimal;
        aviso.Latitud = datos.Latitud;
        aviso.Longitud = datos.Longitud;
        aviso.Direccion = datos.Direccion;
        aviso.ContactoNombre = datos.ContactoNombre;
        aviso.ContactoTelefono = datos.ContactoTelefono;
        aviso.EstadoAparente = datos.EstadoAparente;
        aviso.NivelUrgencia = datos.NivelUrgencia;
        Validar(aviso);

        _avisoRepository.Put(id, aviso);
        return aviso;
    }

    public AvisoRescate CambiarEstado(int id, EstadoAviso nuevoEstado)
    {
        var aviso = ObtenerPorId(id);
        if (aviso.Estado == nuevoEstado) return aviso;

        if (nuevoEstado == EstadoAviso.Atendido)
            throw new ReglaNegocioException("Un aviso pasa a 'Atendido' al registrar el ingreso de la mascota rescatada (POST /api/avisos-rescate/{id}/mascotas).");
        if (!aviso.PuedeCambiarA(nuevoEstado))
            throw new ReglaNegocioException($"Un aviso en estado {aviso.Estado} no puede pasar a {nuevoEstado}.");

        // Regla: sólo se acepta intervenir si hay lugar para recibir al animal
        if (nuevoEstado == EstadoAviso.Aceptado)
            _refugioService.ValidarCapacidadDisponible();

        aviso.Estado = nuevoEstado;
        _avisoRepository.Put(id, aviso);
        return aviso;
    }

    public AvisoRescate Aceptar(int id) => CambiarEstado(id, EstadoAviso.Aceptado);

    public AvisoRescate Descartar(int id) => CambiarEstado(id, EstadoAviso.Descartado);

    public Mascota AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota)
    {
        var aviso = ObtenerPorId(avisoId);
        if (aviso.Estado != EstadoAviso.Aceptado)
            throw new ReglaNegocioException($"Sólo un aviso 'Aceptado' puede generar el ingreso de una mascota (estado actual: {aviso.Estado}).");

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            // MascotaService valida la capacidad del refugio y crea el historial sanitario
            var mascota = _mascotaService.Registrar(datosMascota, aviso.Id);

            aviso.MascotaGeneradaId = mascota.Id;
            aviso.Estado = EstadoAviso.Atendido;
            _avisoRepository.Put(aviso.Id, aviso);
            return mascota;
        });
    }

    public void Eliminar(int id)
    {
        var aviso = ObtenerPorId(id);
        if (aviso.Estado == EstadoAviso.Atendido)
            throw new ReglaNegocioException("El aviso ya fue atendido y generó el ingreso de una mascota: se conserva en el historial.");
        _avisoRepository.Delete(id);
    }
}
