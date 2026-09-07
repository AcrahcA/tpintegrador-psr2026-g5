namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;


public interface IAdoptanteService
{
    List<Adoptante> ObtenerAdoptantes();
    Adoptante? BuscarAdoptante(int id);
    Adoptante RegistrarAdoptante(Adoptante adoptante);
    bool EliminarAdoptante(int id);
}
