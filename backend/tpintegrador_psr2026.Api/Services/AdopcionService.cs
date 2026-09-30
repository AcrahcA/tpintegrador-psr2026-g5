namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AdopcionService : IAdopcionService
{
    private readonly IAdopcionRepository _adopcionRepository;
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IMascotaRepository _mascotaRepository;

    public AdopcionService(
        IAdopcionRepository adopcionRepository,
        ISolicitudAdopcionRepository solicitudRepository,
        IMascotaRepository mascotaRepository)
    {
        _adopcionRepository = adopcionRepository;
        _solicitudRepository = solicitudRepository;
        _mascotaRepository = mascotaRepository;
    }

    public List<Adopcion> Get() => _adopcionRepository.Get();

    public Adopcion? GetById(int id) => _adopcionRepository.Get().FirstOrDefault(a => a.Id == id);

    public Adopcion? Post(int solicitudId, string? observaciones)
    {
        var solicitud = _solicitudRepository.Get().FirstOrDefault(s => s.Id == solicitudId);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.Aprobada)
            return null;

        var mascota = _mascotaRepository.Get().FirstOrDefault(m => m.Id == solicitud.MascotaId);
        if (mascota is null || mascota.Estado != EstadoMascota.DisponibleAdopcion)
            return null;

        var adopcion = new Adopcion(solicitud.Id, observaciones);

        var adopcionCreada = _adopcionRepository.Post(adopcion);

        mascota.Estado = EstadoMascota.Adoptada;
        _mascotaRepository.Put(mascota.Id, mascota);

        return adopcionCreada;
    }
}