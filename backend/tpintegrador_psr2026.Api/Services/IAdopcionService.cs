namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;


public interface IAdopcionService
{
    List<Adopcion> ObtenerAdopciones();
    Adopcion? BuscarAdopcion(int id);
    Adopcion? ConfirmarAdopcion(int solicitudId, string? observaciones);
}
