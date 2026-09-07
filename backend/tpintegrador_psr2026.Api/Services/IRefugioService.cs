namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IRefugioService
{
    Refugio ObtenerConfiguracion();
    Refugio ConfigurarRefugio(Refugio refugio);
    bool TieneLugarDisponible();
    int ContarMascotasActuales();
}
