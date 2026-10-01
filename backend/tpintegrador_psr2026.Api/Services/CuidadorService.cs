namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class CuidadorService : ICuidadorService
{
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public CuidadorService(
        ICuidadorRepository cuidadorRepository,
        IMascotaRepository mascotaRepository)
    {
        _cuidadorRepository = cuidadorRepository;
        _mascotaRepository = mascotaRepository;
    }

    public List<Cuidador> Get() => _cuidadorRepository.Get();

    public Cuidador? GetById(int id) => _cuidadorRepository.Get().FirstOrDefault(c => c.Id == id);

    public Cuidador Post(Cuidador cuidador) => _cuidadorRepository.Post(cuidador);

    public bool Put(int id, Cuidador cuidador) => _cuidadorRepository.Put(id, cuidador);

    public bool Delete(int id) => _cuidadorRepository.Delete(id);

    public int ObtenerCantidadAsignadas(int cuidadorId) =>
        _mascotaRepository.Get().Count(m => m.CuidadorId == cuidadorId && m.Estado != EstadoMascota.Adoptada);

    public bool TieneDisponibilidad(int cuidadorId)
    {
        var cuidador = GetById(cuidadorId);
        if (cuidador is null) return false;

        return ObtenerCantidadAsignadas(cuidadorId) < cuidador.CapacidadMaxima;
    }
}