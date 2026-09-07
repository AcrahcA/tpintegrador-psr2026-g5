namespace tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Domain;

public class TratamientoRepository : ITratamientoRepository
{
    private readonly List<Tratamiento> _elementos = new();
    private int _siguienteId = 1;

    public List<Tratamiento> ObtenerTodos() => _elementos;

    public Tratamiento? ObtenerPorId(int id) => _elementos.FirstOrDefault(e => e.Id == id);

    public Tratamiento Agregar(Tratamiento entidad)
    {
        entidad.Id = _siguienteId++;
        _elementos.Add(entidad);
        return entidad;
    }

    public bool Actualizar(int id, Tratamiento entidad)
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
