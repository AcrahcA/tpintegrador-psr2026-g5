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

    public List<Adopcion> ObtenerAdopciones() => _adopcionRepository.ObtenerTodos();

    public Adopcion? BuscarAdopcion(int id) => _adopcionRepository.ObtenerPorId(id);

    // Regla: antes de finalizar el proceso debe verificarse que la mascota siga
    // disponible, no tenga tratamientos activos y que la solicitud este aprobada.
    public Adopcion? ConfirmarAdopcion(int solicitudId, string? observaciones)
    {
        var solicitud = _solicitudRepository.ObtenerPorId(solicitudId);
        if (solicitud is null) return null;

        if (solicitud.Estado != EstadoSolicitud.Aprobada)
            return null;

        var mascota = _mascotaRepository.ObtenerPorId(solicitud.MascotaId);
        if (mascota is null) return null;

        if (mascota.Estado == EstadoMascota.Adoptada)
            return null;

        if (!_mascotaService.CumpleCondicionesSanitarias(mascota.Id))
            return null;

        var adopcion = _adopcionRepository.Agregar(new Adopcion
        {
            SolicitudAdopcionId = solicitud.Id,
            Observaciones = observaciones
        });

        mascota.Estado = EstadoMascota.Adoptada;
        _mascotaRepository.Actualizar(mascota.Id, mascota);

        return adopcion;
    }
}
