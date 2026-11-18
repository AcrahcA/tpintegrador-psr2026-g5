namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _refugio;
    private readonly IMascotaRepository _mascotas;
    public RefugioService(IRefugioRepository refugioRepository, IMascotaRepository mascotaRepository)
    { _refugio = refugioRepository; _mascotas = mascotaRepository; }

    public Refugio Get() => _refugio.Get().FirstOrDefault() ??
        _refugio.Post(new Refugio("Refugio de Mascotas", 50));

    public Refugio Post(Refugio nuevo)
    {
        if (string.IsNullOrWhiteSpace(nuevo.Nombre) || nuevo.CapacidadMaxima <= 0)
            throw new ArgumentException("Indicá un nombre y capacidad mayor que cero.");
        var ocupadas = ContarMascotasActuales();
        if (nuevo.CapacidadMaxima < ocupadas)
            throw new ArgumentException($"No se puede reducir la capacidad a {nuevo.CapacidadMaxima}: hay {ocupadas} mascotas alojadas.");
        var existente = _refugio.Get().FirstOrDefault();
        if (existente is null) return _refugio.Post(nuevo);
        existente.Nombre = nuevo.Nombre.Trim();
        existente.CapacidadMaxima = nuevo.CapacidadMaxima;
        if (!_refugio.Put(existente.Id, existente))
            throw new OperacionNoPermitidaException("No se pudo guardar la configuración.");
        return existente;
    }

    public int ContarMascotasActuales() => _mascotas.Get().Count(m => m.Estado != EstadoMascota.Adoptada);
    public bool TieneLugarDisponible() => ContarMascotasActuales() < Get().CapacidadMaxima;
}
