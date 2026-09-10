namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IHistorialSanitarioService
{
    List<HistorialSanitario> ObtenerHistoriales();
    HistorialSanitario? BuscarPorId(int id);
    HistorialSanitario? BuscarPorMascotaId(int mascotaId);
    HistorialSanitario CrearHistorial(HistorialSanitario historial);
    bool ActualizarHistorial(int id, HistorialSanitario historial);
    bool EliminarHistorial(int id);
}