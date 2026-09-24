namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;


public interface IAdoptanteService
{
   List<Adoptante> Get();
    Adoptante? GetById(int id);
    Adoptante Post(Adoptante adoptante);
    bool Delete(int id);
}
