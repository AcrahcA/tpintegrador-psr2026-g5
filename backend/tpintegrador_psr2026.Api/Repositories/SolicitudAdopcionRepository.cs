namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public class SolicitudAdopcionRepository : ISolicitudAdopcionRepository
{
    private readonly List<SolicitudAdopcion> _elementos = new();
    private int _siguienteId = 1;

    public List<SolicitudAdopcion> Get() => _elementos;

    public SolicitudAdopcion? GetPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public SolicitudAdopcion Post(SolicitudAdopcion entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Put(int id, SolicitudAdopcion entidad)
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