namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;


public interface IAdopcionService
{
    List<Adopcion> Get();
    Adopcion? GetById(int id);
    Adopcion? Post(int solicitudId, string? observaciones);
}
