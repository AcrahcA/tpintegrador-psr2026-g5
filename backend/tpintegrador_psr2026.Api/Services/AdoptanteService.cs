namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AdoptanteService : IAdoptanteService
{
    private readonly IAdoptanteRepository _adoptanteRepository;

    public AdoptanteService(IAdoptanteRepository adoptanteRepository)
    {
        _adoptanteRepository = adoptanteRepository;
    }

    public List<Adoptante> ObtenerAdoptantes() => _adoptanteRepository.ObtenerTodos();

    public Adoptante? BuscarAdoptante(int id) => _adoptanteRepository.ObtenerPorId(id);

    public Adoptante RegistrarAdoptante(Adoptante adoptante) => _adoptanteRepository.Agregar(adoptante);

    public bool EliminarAdoptante(int id) => _adoptanteRepository.Eliminar(id);
}
