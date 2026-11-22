namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface INotificacionService
{
    /// <summary>Deja un mensaje en el buzón. Si la persona no tiene cuenta (usuarioId nulo), no hace nada.</summary>
    void Notificar(int? usuarioId, TipoNotificacion tipo, string titulo, string mensaje, string? motivo = null,
        int? solicitudAdopcionId = null, int? avisoRescateId = null);
    List<Notificacion> Buzon(int usuarioId);
    int ContarNoLeidas(int usuarioId);
    Notificacion MarcarLeida(int usuarioId, int notificacionId);
    void MarcarTodasLeidas(int usuarioId);
}

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _notificacionRepository;

    public NotificacionService(INotificacionRepository notificacionRepository) => _notificacionRepository = notificacionRepository;

    public void Notificar(int? usuarioId, TipoNotificacion tipo, string titulo, string mensaje, string? motivo = null,
        int? solicitudAdopcionId = null, int? avisoRescateId = null)
    {
        if (usuarioId is null) return;
        _notificacionRepository.Post(new Notificacion(usuarioId.Value, tipo, titulo, mensaje,
            Notificacion.NormalizarMotivo(motivo), solicitudAdopcionId, avisoRescateId));
    }

    public List<Notificacion> Buzon(int usuarioId) => _notificacionRepository.GetPorUsuario(usuarioId);

    public int ContarNoLeidas(int usuarioId) => _notificacionRepository.ContarNoLeidas(usuarioId);

    public Notificacion MarcarLeida(int usuarioId, int notificacionId)
    {
        var n = _notificacionRepository.GetPorId(notificacionId);
        // Cada persona sólo puede ver y modificar sus propios mensajes
        if (n is null || n.UsuarioId != usuarioId)
            throw new RecursoNoEncontradoException("No existe ese mensaje en tu buzón.");
        if (!n.Leida)
        {
            n.Leida = true;
            _notificacionRepository.Put(n.Id, n);
        }
        return n;
    }

    public void MarcarTodasLeidas(int usuarioId) => _notificacionRepository.MarcarTodasLeidas(usuarioId);
}
