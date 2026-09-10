namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class CuidadorRepository : ICuidadorRepository
{
    private readonly List<Cuidador> _elementos = new();
    private int _siguienteId = 1;

    public List<Cuidador> Get() => _elementos;

    public Cuidador? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Cuidador Post(Cuidador entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, Cuidador entidad)
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