namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface INotificacionRepository : IRepositorio<Notificacion>
{
    /// <summary>Buzón de la persona, de la más nueva a la más vieja.</summary>
    List<Notificacion> GetPorUsuario(int usuarioId);
    int ContarNoLeidas(int usuarioId);
    void MarcarTodasLeidas(int usuarioId);
}
