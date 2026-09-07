namespace tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Domain;

public class AdoptanteRepository : IAdoptanteRepository
{
    private readonly List<Adoptante> _elementos = new();
    private int _siguienteId = 1;

    public List<Adoptante> ObtenerTodos() => _elementos;

    public Adoptante? ObtenerPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Adoptante Agregar(Adoptante entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Actualizar(int id, Adoptante entidad)
    {
        var existente = ObtenerPorId(id);
        if (existente is null) return false;

        var indice = _elementos.IndexOf(existente);
        entidad.Id = id;
        _elementos[indice] = entidad;
        return true;
    }

    public bool Eliminar(int id)
    {
        var existente = ObtenerPorId(id);
        if (existente is null) return false;

        _elementos.Remove(existente);
        return true;
    }
}
