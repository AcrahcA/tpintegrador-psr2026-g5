namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface IRefugioService
{
    Refugio Obtener();
    Refugio Configurar(string nombre, int capacidadMaxima);
    int ContarMascotasAlojadas();
    bool TieneLugarDisponible();
    /// <summary>Lanza una excepción de negocio si el refugio alcanzó su capacidad máxima.</summary>
    void ValidarCapacidadDisponible();
}

public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _refugioRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public RefugioService(IRefugioRepository refugioRepository, IMascotaRepository mascotaRepository)
    {
        _refugioRepository = refugioRepository;
        _mascotaRepository = mascotaRepository;
    }

    public Refugio Obtener() =>
        _refugioRepository.GetActual()
        ?? _refugioRepository.Post(new Refugio(Refugio.NombrePorDefecto, Refugio.CapacidadPorDefecto));

    public Refugio Configurar(string nombre, int capacidadMaxima)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("El nombre del refugio es obligatorio.");
        if (capacidadMaxima <= 0)
            throw new ReglaNegocioException("La capacidad máxima del refugio debe ser un número mayor a cero.");

        var alojadas = ContarMascotasAlojadas();
        if (capacidadMaxima < alojadas)
            throw new ReglaNegocioException($"La capacidad no puede ser menor a la cantidad de mascotas alojadas actualmente ({alojadas}).");

        var refugio = Obtener();
        refugio.Nombre = nombre.Trim();
        refugio.CapacidadMaxima = capacidadMaxima;
        _refugioRepository.Put(refugio.Id, refugio);
        return refugio;
    }

    // Las mascotas adoptadas no ocupan plaza dentro del refugio
    public int ContarMascotasAlojadas() => _mascotaRepository.ContarAlojadas();

    public bool TieneLugarDisponible() => Obtener().TieneLugarDisponible(ContarMascotasAlojadas());

    public void ValidarCapacidadDisponible()
    {
        if (!TieneLugarDisponible())
            throw new ReglaNegocioException(
                $"El refugio alcanzó su capacidad máxima ({Obtener().CapacidadMaxima} mascotas). " +
                "El aviso puede quedar pendiente o derivarse a otra organización.");
    }
}
