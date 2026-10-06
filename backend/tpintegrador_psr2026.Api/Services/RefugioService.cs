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
            // Configuración por defecto usando el constructor parametrizado
            refugio = _refugioRepository.Post(new Refugio("Refugio de Mascotas", 50));
        }
        return refugio;
    }

    public Refugio Post(Refugio refugio)
    {
        // Regla: La capacidad máxima debe ser un número mayor a cero
        if (refugio.CapacidadMaxima <= 0)
        {
            throw new ArgumentException("La capacidad máxima del refugio debe ser un número mayor a cero.");
        }

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
        // Las mascotas adoptadas no ocupan plaza dentro del refugio
        return _mascotaRepository.Get().Count(m => m.Estado != EstadoMascota.Adoptada);
    }

    public bool TieneLugarDisponible()
    {
        var config = Get();
        return ContarMascotasActuales() < config.CapacidadMaxima;
    }
}