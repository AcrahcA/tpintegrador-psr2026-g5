namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class RefugioRepository : IRefugioRepository
{
    private readonly List<Refugio> _elementos = new();
    private int _siguienteId = 1;

    public List<Refugio> Get() => _elementos;

    public Refugio? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Refugio Post(Refugio entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, Refugio entidad)
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