namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class HistorialSanitarioService : IHistorialSanitarioService
{
    private readonly IHistorialSanitarioRepository _historialRepository;

    public HistorialSanitarioService(IHistorialSanitarioRepository historialRepository)
    {
        _historialRepository = historialRepository;
    }

    public List<HistorialSanitario> ObtenerHistoriales() => _historialRepository.ObtenerTodos();

    public HistorialSanitario? BuscarPorId(int id) => _historialRepository.ObtenerPorId(id);

    public HistorialSanitario? BuscarPorMascotaId(int mascotaId)
    {
        return _historialRepository.ObtenerTodos()
            .FirstOrDefault(h => h.MascotaId == mascotaId);
    }

    public HistorialSanitario CrearHistorial(HistorialSanitario historial)
    {
        return _historialRepository.Agregar(historial);
    }

    public bool ActualizarHistorial(int id, HistorialSanitario historial)
    {
        return _historialRepository.Actualizar(id, historial);
    }

    public bool EliminarHistorial(int id) => _historialRepository.Eliminar(id);
}