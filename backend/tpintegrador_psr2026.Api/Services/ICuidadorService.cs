namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ICuidadorService
{
    List<Cuidador> ObtenerCuidadores();
    Cuidador? BuscarCuidador(int id);
    Cuidador RegistrarCuidador(Cuidador cuidador);
    bool ActualizarCuidador(int id, Cuidador cuidador);
    bool TieneDisponibilidad(int cuidadorId);
    int ObtenerCantidadAsignadas(int cuidadorId);
    bool EliminarCuidador(int id);
}
