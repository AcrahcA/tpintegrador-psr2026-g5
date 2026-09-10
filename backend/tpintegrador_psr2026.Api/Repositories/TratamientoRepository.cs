namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class TratamientoRepository : ITratamientoRepository
{
    private readonly List<Tratamiento> _elementos = new();
    private int _siguienteId = 1;

    public List<Tratamiento> Get() => _elementos;

    public Tratamiento? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Tratamiento Post(Tratamiento entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, Tratamiento entidad)
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