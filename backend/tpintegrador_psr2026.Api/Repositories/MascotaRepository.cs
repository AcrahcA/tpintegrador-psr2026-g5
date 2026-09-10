namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class MascotaRepository : IMascotaRepository
{
    private readonly List<Mascota> _elementos = new();
    private int _siguienteId = 1;

    public List<Mascota> Get() => _elementos;

    public Mascota? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Mascota Post(Mascota entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, Mascota entidad)
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