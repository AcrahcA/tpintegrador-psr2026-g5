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

    public List<Cuidador> ObtenerCuidadores() => _cuidadorRepository.ObtenerTodos();

    public Cuidador? BuscarCuidador(int id) => _cuidadorRepository.ObtenerPorId(id);

    public Cuidador RegistrarCuidador(Cuidador cuidador)
    {
        // Se fuerza la asociación al refugio único (ID = 1) por regla de negocio
        cuidador.RefugioId = 1;
        return _cuidadorRepository.Agregar(cuidador);
    }

    public bool ActualizarCuidador(int id, Cuidador cuidador)
    {
        cuidador.RefugioId = 1;
        return _cuidadorRepository.Actualizar(id, cuidador);
    }

    public int ObtenerCantidadAsignadas(int cuidadorId) =>
        _mascotaRepository.ObtenerTodos().Count(m => m.CuidadorId == cuidadorId);

    public bool TieneDisponibilidad(int cuidadorId)
    {
        var cuidador = _cuidadorRepository.ObtenerPorId(cuidadorId);
        if (cuidador is null) return false;

        return ObtenerCantidadAsignadas(cuidadorId) < cuidador.CapacidadMaxima;
    }

    public bool EliminarCuidador(int id) => _cuidadorRepository.Eliminar(id);
}