namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AvisoRescateService : IAvisoRescateService
{
    private readonly IAvisoRescateRepository _avisoRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IRefugioRepository _refugioRepository;

    public AvisoRescateService(
        IAvisoRescateRepository avisoRepository,
        IMascotaRepository mascotaRepository,
        IRefugioRepository refugioRepository)
    {
        _avisoRepository = avisoRepository;
        _mascotaRepository = mascotaRepository;
        _refugioRepository = refugioRepository;
    }

    public List<AvisoRescate> Get() => _avisoRepository.Get();

    public List<AvisoRescate> GetPendientes() =>
        _avisoRepository.Get()
            .Where(a => a.Estado == EstadoAviso.Reportado)
            .ToList();

    public AvisoRescate? GetById(int id) => _avisoRepository.Get().FirstOrDefault(a => a.Id == id);

    public AvisoRescate Post(AvisoRescate aviso)
    {
        aviso.FechaAviso = DateTime.Now;
        aviso.Estado = EstadoAviso.Reportado;
        return _avisoRepository.Post(aviso);
    }

    public bool PutEstado(int id, EstadoAviso nuevoEstado)
    {
        var aviso = _avisoRepository.Get().FirstOrDefault(a => a.Id == id);
        if (aviso is null) return false;

        aviso.Estado = nuevoEstado;
        return _avisoRepository.Put(id, aviso);
    }

    public Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota)
    {
        var aviso = _avisoRepository.Get().FirstOrDefault(a => a.Id == avisoId);
        if (aviso is null || aviso.Estado != EstadoAviso.Aceptado)
            return null;

        var refugio = _refugioRepository.Get().FirstOrDefault();
        if (refugio is not null && _mascotaRepository.Get().Count >= refugio.CapacidadMaxima)
            return null;

        datosMascota.Estado = EstadoMascota.Ingresada;
        var mascotaCreada = _mascotaRepository.Post(datosMascota);

        aviso.MascotaGeneradaId = mascotaCreada.Id;
        aviso.Estado = EstadoAviso.Atendido;
        _avisoRepository.Put(aviso.Id, aviso);

        return mascotaCreada;
    }

    public bool Delete(int id) => _avisoRepository.Delete(id);
}