namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ICuidadorService
{
List<Cuidador> Get();
    Cuidador? GetById(int id);
    Cuidador Post(Cuidador cuidador);
    bool Put(int id, Cuidador cuidador);
    int ObtenerCantidadAsignadas(int cuidadorId);
    bool TieneDisponibilidad(int cuidadorId);
    bool Delete(int id);
}
