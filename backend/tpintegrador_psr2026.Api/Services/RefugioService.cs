namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _refugioRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public RefugioService(IRefugioRepository refugioRepository, IMascotaRepository mascotaRepository)
    {
        _refugioRepository = refugioRepository;
        _mascotaRepository = mascotaRepository;
    }

    public Refugio ObtenerConfiguracion()
    {
        var refugio = _refugioRepository.ObtenerTodos().FirstOrDefault();
        if (refugio is null)
        {
            // Configuracion por defecto si todavia no fue creada
            refugio = _refugioRepository.Agregar(new Refugio { Nombre = "Refugio de Mascotas", CapacidadMaxima = 50 });
        }
        return refugio;
    }

    public Refugio ConfigurarRefugio(Refugio refugio)
    {
        var existente = _refugioRepository.ObtenerTodos().FirstOrDefault();
        if (existente is null)
        {
            return _refugioRepository.Agregar(refugio);
        }

        _refugioRepository.Actualizar(existente.Id, refugio);
        return refugio;
    }

    public int ContarMascotasActuales()
    {
        // Las mascotas adoptadas ya no ocupan una plaza dentro del refugio
        return _mascotaRepository.ObtenerTodos().Count(m => m.Estado != EstadoMascota.Adoptada);
    }

    public bool TieneLugarDisponible()
    {
        var config = ObtenerConfiguracion();
        return ContarMascotasActuales() < config.CapacidadMaxima;
    }
}
