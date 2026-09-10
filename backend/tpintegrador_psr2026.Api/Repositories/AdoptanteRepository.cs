namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class AdoptanteRepository : IAdoptanteRepository
{
    private readonly List<Adoptante> _elementos = new();
    private int _siguienteId = 1;

    public List<Adoptante> Get() => _elementos;

    public Adoptante? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Adoptante Post(Adoptante entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, Adoptante entidad)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        var indice = _elementos.IndexOf(existente);
        entidad.Id = id;
        _elementos[indice] = entidad;
        return true;
    }

    public bool Delete(int id)
    {
        var existente = GetPorId(id);
        if (existente is null) return false;

        _elementos.Remove(existente);
        return true;
    }
}