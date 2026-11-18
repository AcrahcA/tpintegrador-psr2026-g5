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

    public List<Adoptante> Get() => _adoptanteRepository.Get();

    public Adoptante? GetById(int id) => _adoptanteRepository.Get().FirstOrDefault(a => a.Id == id);

    public Adoptante? Post(Adoptante adoptante)
    {
        // Regla: Nombre, DNI y Teléfono son obligatorios como datos de contacto
        if (string.IsNullOrWhiteSpace(adoptante.Nombre) || 
            string.IsNullOrWhiteSpace(adoptante.Dni) || 
            string.IsNullOrWhiteSpace(adoptante.Telefono))
        {
            return null;
        }

        // Regla: No puede haber adoptantes duplicados (DNI único)
        var existeDni = _adoptanteRepository.Get().Any(a => a.Dni == adoptante.Dni);
        if (existeDni)
        {
            return null;
        }

        return _adoptanteRepository.Post(adoptante);
    }

    public bool Actualizar(int id, Adoptante datos)
    {
        var actual = GetById(id);
        if (actual is null || string.IsNullOrWhiteSpace(datos.Telefono) ||
            _adoptanteRepository.Get().Any(x => x.Id != id && x.Dni == datos.Dni)) return false;
        actual.Nombre = datos.Nombre;
        actual.Dni = datos.Dni;
        actual.Telefono = datos.Telefono;
        actual.Direccion = datos.Direccion;
        return _adoptanteRepository.Put(id, actual);
    }

    public bool Delete(int id) => _adoptanteRepository.Delete(id);
}