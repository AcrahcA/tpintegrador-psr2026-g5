namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class HistorialSanitarioService : IHistorialSanitarioService
{
    private readonly IHistorialSanitarioRepository _historiales;
    private readonly IMascotaRepository _mascotas;
    private readonly ITratamientoRepository _tratamientos;
    public HistorialSanitarioService(IHistorialSanitarioRepository historialRepository,
        IMascotaRepository mascotaRepository, ITratamientoRepository tratamientoRepository)
    { _historiales = historialRepository; _mascotas = mascotaRepository; _tratamientos = tratamientoRepository; }
    public List<HistorialSanitario> Get() => _historiales.Get();
    public HistorialSanitario? GetById(int id) => _historiales.GetPorId(id);
    public HistorialSanitario? GetByMascotaId(int mascotaId) =>
        _historiales.Get().FirstOrDefault(h => h.MascotaId == mascotaId);
    public HistorialSanitario Post(HistorialSanitario historial)
    {
        if (_mascotas.GetPorId(historial.MascotaId) is null || GetByMascotaId(historial.MascotaId) is not null)
            throw new ArgumentException("La mascota debe existir y no puede tener dos historiales sanitarios.");
        historial.Tratamientos = new List<Tratamiento>();
        return _historiales.Post(historial);
    }
    public bool Put(int id, HistorialSanitario datos)
    {
        var actual = GetById(id);
        if (actual is null || datos.MascotaId != actual.MascotaId) return false;
        actual.Observaciones = datos.Observaciones;
        return _historiales.Put(id, actual);
    }
    public bool Delete(int id)
    {
        var historial = GetById(id);
        if (historial is null || _mascotas.GetPorId(historial.MascotaId)?.Estado == EstadoMascota.Adoptada ||
            _tratamientos.Get().Any(t => t.HistorialSanitarioId == id)) return false;
        return _historiales.Delete(id);
    }
}
