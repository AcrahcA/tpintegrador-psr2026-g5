namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class CuidadorService : ICuidadorService
{
    private readonly ICuidadorRepository _cuidadores;
    private readonly IMascotaRepository _mascotas;
    public CuidadorService(ICuidadorRepository cuidadorRepository, IMascotaRepository mascotaRepository)
    { _cuidadores = cuidadorRepository; _mascotas = mascotaRepository; }

    public List<Cuidador> Get() => _cuidadores.Get();
    public Cuidador? GetById(int id) => _cuidadores.GetPorId(id);
    public Cuidador Post(Cuidador cuidador)
    {
        if (string.IsNullOrWhiteSpace(cuidador.Nombre) || cuidador.CapacidadMaxima <= 0)
            throw new ArgumentException("El cuidador requiere nombre y capacidad mayor que cero.");
        return _cuidadores.Post(cuidador);
    }
    public bool Put(int id, Cuidador cuidador)
    {
        var existente = GetById(id);
        if (existente is null || string.IsNullOrWhiteSpace(cuidador.Nombre) ||
            cuidador.CapacidadMaxima < 1 || cuidador.CapacidadMaxima < ObtenerCantidadAsignadas(id)) return false;
        existente.Nombre = cuidador.Nombre.Trim();
        existente.Especializacion = cuidador.Especializacion;
        existente.CapacidadMaxima = cuidador.CapacidadMaxima;
        return _cuidadores.Put(id, existente);
    }
    public bool Delete(int id) =>
        ObtenerCantidadAsignadas(id) == 0 && !_mascotas.Get().Any(m => m.CuidadorId == id) && _cuidadores.Delete(id);
    public int ObtenerCantidadAsignadas(int cuidadorId) =>
        _mascotas.Get().Count(m => m.CuidadorId == cuidadorId && m.Estado != EstadoMascota.Adoptada);
    public bool TieneDisponibilidad(int cuidadorId) =>
        GetById(cuidadorId) is { CapacidadMaxima: > 0 } cuidador &&
        ObtenerCantidadAsignadas(cuidadorId) < cuidador.CapacidadMaxima;
    public bool AsignarMascota(int cuidadorId, int mascotaId)
    {
        var cuidador = GetById(cuidadorId);
        var mascota = _mascotas.GetPorId(mascotaId);
        if (cuidador is null || mascota is null || mascota.Estado == EstadoMascota.Adoptada) return false;
        if (mascota.CuidadorId == cuidadorId) return true;
        if (!TieneDisponibilidad(cuidadorId)) return false;
        mascota.CuidadorId = cuidadorId;
        return _mascotas.Put(mascotaId, mascota);
    }
}
