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
        
        // Permite atender si el aviso existe y no fue previamente atendidoni descartado
        if (aviso is null || aviso.Estado == EstadoAviso.Atendido || aviso.Estado == EstadoAviso.Descartado)
            return null;

        // Validar la capacidad máxima del refugio (excluyendo las mascotas que ya fueron adoptadas)
        var refugio = _refugioRepository.Get().FirstOrDefault();
        var ocupacionActual = _mascotaRepository.Get().Count(m => m.Estado != EstadoMascota.Adoptada);

        if (refugio is not null && ocupacionActual >= refugio.CapacidadMaxima)
            return null;

        // Dar de alta la mascota con estado Ingresada
        datosMascota.Estado = EstadoMascota.Ingresada;
        var mascotaCreada = _mascotaRepository.Post(datosMascota);

        // Vincular la mascota generada y actualizar el estado del aviso a Atendido
        aviso.MascotaGeneradaId = mascotaCreada.Id;
        aviso.Estado = EstadoAviso.Atendido;
        _avisoRepository.Put(aviso.Id, aviso);

        return mascotaCreada;
    }

    public bool Delete(int id) => _avisoRepository.Delete(id);
}