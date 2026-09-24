namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class CuidadorService : ICuidadorService
{
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public CuidadorService(ICuidadorRepository cuidadorRepository, IMascotaRepository mascotaRepository)
    {
        _cuidadorRepository = cuidadorRepository;
        _mascotaRepository = mascotaRepository;
    }

    public List<Cuidador> Get() => _cuidadorRepository.Get();

    public Cuidador? GetById(int id) => _cuidadorRepository.Get(id);

    public Cuidador Post(Cuidador cuidador)
    {
        // Se fuerza la asociación al refugio único (ID = 1) por regla de negocio
        cuidador.RefugioId = 1;
        return _cuidadorRepository.Post(cuidador);
    }

    public bool Put(int id, Cuidador cuidador)
    {
        cuidador.RefugioId = 1;
        return _cuidadorRepository.Put(id, cuidador);
    }

    public int ObtenerCantidadAsignadas(int cuidadorId) =>
        _mascotaRepository.Get().Count(m => m.CuidadorId == cuidadorId);

    public bool TieneDisponibilidad(int cuidadorId)
    {
        var cuidador = _cuidadorRepository.Get(cuidadorId);
        if (cuidador is null) return false;

        return ObtenerCantidadAsignadas(cuidadorId) < cuidador.CapacidadMaxima;
    }

    public bool Delete(int id) => _cuidadorRepository.Delete(id);
}