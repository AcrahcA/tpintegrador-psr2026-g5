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

    public Refugio Get()
    {
        var refugio = _refugioRepository.Get().FirstOrDefault();
        if (refugio is null)
        {
            // Configuración por defecto si todavía no fue creada
            refugio = _refugioRepository.Post(new Refugio { Nombre = "Refugio de Mascotas", CapacidadMaxima = 50 });
        }
        return refugio;
    }

    public Refugio Post(Refugio refugio)
    {
        var existente = _refugioRepository.Get().FirstOrDefault();
        if (existente is null)
        {
            return _refugioRepository.Post(refugio);
        }

        _refugioRepository.Put(existente.Id, refugio);
        return refugio;
    }

    public int ContarMascotasActuales()
    {
        // Las mascotas adoptadas ya no ocupan una plaza dentro del refugio
        return _mascotaRepository.Get().Count(m => m.Estado != EstadoMascota.Adoptada);
    }

    public bool TieneLugarDisponible()
    {
        var config = Get();
        return ContarMascotasActuales() < config.CapacidadMaxima;
    }
}