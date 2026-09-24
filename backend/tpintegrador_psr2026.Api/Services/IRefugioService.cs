namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IRefugioService
{
  Refugio Get();
    Refugio Post(Refugio refugio);
    int ContarMascotasActuales();
    bool TieneLugarDisponible();
}
