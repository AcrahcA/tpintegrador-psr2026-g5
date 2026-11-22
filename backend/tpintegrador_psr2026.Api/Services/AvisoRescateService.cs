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
    /// <summary>Registra el aviso. Si quien reporta tiene cuenta, se vincula para avisarle por su buzón.</summary>
    AvisoRescate Registrar(AvisoRescate aviso, int? usuarioId = null);
    AvisoRescate Actualizar(int id, AvisoRescate datos);
    AvisoRescate CambiarEstado(int id, EstadoAviso nuevoEstado, string? motivo = null);
    AvisoRescate Aceptar(int id);
    AvisoRescate Descartar(int id, string? motivo = null);
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
    private readonly INotificacionService _notificaciones;

    public AvisoRescateService(
        IAvisoRescateRepository avisoRepository,
        IMascotaService mascotaService,
        IRefugioService refugioService,
        IUnidadDeTrabajo unidadDeTrabajo,
        INotificacionService notificaciones)
    {
        _notificaciones = notificaciones;
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

    public AvisoRescate Registrar(AvisoRescate aviso, int? usuarioId = null)
    {
        Validar(aviso);
        aviso.FechaAviso = DateTime.UtcNow;
        aviso.Estado = EstadoAviso.Reportado;
        aviso.MascotaGeneradaId = null;
        aviso.UsuarioId = usuarioId;
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

    public AvisoRescate CambiarEstado(int id, EstadoAviso nuevoEstado, string? motivo = null)
    {
        var aviso = ObtenerPorId(id);
        if (aviso.Estado == nuevoEstado) return aviso;
        motivo = Notificacion.NormalizarMotivo(motivo);

        if (nuevoEstado == EstadoAviso.Atendido)
            throw new ReglaNegocioException("Un aviso pasa a 'Atendido' al registrar el ingreso de la mascota rescatada (POST /api/avisos-rescate/{id}/mascotas).");
        if (!aviso.PuedeCambiarA(nuevoEstado))
            throw new ReglaNegocioException($"Un aviso en estado {aviso.Estado} no puede pasar a {nuevoEstado}.");

        // Regla: sólo se acepta intervenir si hay lugar para recibir al animal
        if (nuevoEstado == EstadoAviso.Aceptado)
            _refugioService.ValidarCapacidadDisponible();

        return _unidadDeTrabajo.EjecutarEnTransaccion(() =>
        {
            aviso.Estado = nuevoEstado;
            _avisoRepository.Put(id, aviso);

            // Buzón de quien reportó (si lo hizo con su cuenta). El motivo es opcional.
            if (nuevoEstado == EstadoAviso.Aceptado)
                _notificaciones.Notificar(aviso.UsuarioId, TipoNotificacion.AvisoAceptado, $"Aceptamos tu aviso #{aviso.Id}",
                    "El refugio va a intervenir para rescatar al animal. Te avisamos cuando lo rescatemos.", motivo, avisoRescateId: aviso.Id);
            else if (nuevoEstado == EstadoAviso.Descartado)
                _notificaciones.Notificar(aviso.UsuarioId, TipoNotificacion.AvisoDescartado, $"Tu aviso #{aviso.Id} fue descartado",
                    "El refugio revisó el aviso y no va a intervenir esta vez. Gracias por avisarnos.", motivo, avisoRescateId: aviso.Id);
            return aviso;
        });
    }

    public AvisoRescate Aceptar(int id) => CambiarEstado(id, EstadoAviso.Aceptado);

    public AvisoRescate Descartar(int id, string? motivo = null) => CambiarEstado(id, EstadoAviso.Descartado, motivo);

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

            _notificaciones.Notificar(aviso.UsuarioId, TipoNotificacion.AvisoAtendido,
                $"Rescatamos al animal de tu aviso #{aviso.Id}",
                $"Ingresó al refugio con el nombre {mascota.Nombre}. Gracias por avisarnos: ya está recibiendo cuidados.",
                avisoRescateId: aviso.Id);
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
