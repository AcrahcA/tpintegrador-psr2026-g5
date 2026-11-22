using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Repositories;

public class NotificacionRepository : Repositorio<Notificacion>, INotificacionRepository
{
    public NotificacionRepository(RefugioDbContext context) : base(context) { }

    public List<Notificacion> GetPorUsuario(int usuarioId) =>
        Set.Where(n => n.UsuarioId == usuarioId).OrderByDescending(n => n.Fecha).ThenByDescending(n => n.Id).ToList();

    public int ContarNoLeidas(int usuarioId) => Set.Count(n => n.UsuarioId == usuarioId && !n.Leida);

    public void MarcarTodasLeidas(int usuarioId)
    {
        foreach (var n in Set.Where(n => n.UsuarioId == usuarioId && !n.Leida)) n.Leida = true;
        Context.SaveChanges();
    }
}
