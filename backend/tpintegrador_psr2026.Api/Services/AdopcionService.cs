namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AdopcionService : IAdopcionService
{
    private readonly IAdopcionRepository _adopcionRepository;
    private readonly ISolicitudAdopcionRepository _solicitudRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IMascotaService _mascotaService;

    public AdopcionService(
        IAdopcionRepository adopcionRepository,
        ISolicitudAdopcionRepository solicitudRepository,
        IMascotaRepository mascotaRepository,
        IMascotaService mascotaService)
    {
        _adopcionRepository = adopcionRepository;
        _solicitudRepository = solicitudRepository;
        _mascotaRepository = mascotaRepository;
        _mascotaService = mascotaService;
    }

    public List<Adopcion> Get() => _adopcionRepository.Get();

    public Adopcion? GetById(int id) => _adopcionRepository.Get(id);

    // Regla: antes de finalizar el proceso debe verificarse que la mascota siga
    // disponible, no tenga tratamientos activos y que la solicitud este aprobada.
    public Adopcion? Post(int solicitudId, string? observaciones)
    {
        var solicitud = _solicitudRepository.Get(solicitudId);
        if (solicitud is null) return null;

        if (solicitud.Estado != EstadoSolicitud.Aprobada)
            return null;

        var mascota = _mascotaRepository.Get(solicitud.MascotaId);
        if (mascota is null) return null;

        if (mascota.Estado == EstadoMascota.Adoptada)
            return null;

        if (!_mascotaService.CumpleCondicionesSanitarias(mascota.Id))
            return null;

        var adopcion = _adopcionRepository.Post(new Adopcion(solicitud.Id, observaciones));

        mascota.Estado = EstadoMascota.Adoptada;
        _mascotaRepository.Put(mascota.Id, mascota);

        return adopcion;
    }
}