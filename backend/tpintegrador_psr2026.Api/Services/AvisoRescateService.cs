namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AvisoRescateService : IAvisoRescateService
{
    private readonly IAvisoRescateRepository _avisoRepository;
    private readonly IMascotaService _mascotaService;

    public AvisoRescateService(IAvisoRescateRepository avisoRepository, IMascotaService mascotaService)
    {
        _avisoRepository = avisoRepository;
        _mascotaService = mascotaService;
    }

    public List<AvisoRescate> Get() => _avisoRepository.Get();

    // Devuelve los avisos cuyo estado sea Reportado
    public List<AvisoRescate> GetPendientes()
    {
        return _avisoRepository.Get()
            .Where(a => a.Estado == EstadoAviso.Reportado)
            .ToList();
    }

    public AvisoRescate? GetById(int id) => _avisoRepository.Get(id);

    public AvisoRescate Post(AvisoRescate aviso)
    {
        aviso.Estado = EstadoAviso.Reportado;
        return _avisoRepository.Post(aviso);
    }

    // Permite cambiar el estado a cualquier valor del enum EstadoAviso (EnEvaluacion, Aceptado, Descartado, etc.)
    public bool PutEstado(int id, EstadoAviso nuevoEstado)
    {
        var aviso = _avisoRepository.Get(id);
        if (aviso is null) return false;

        aviso.Estado = nuevoEstado;
        return _avisoRepository.Put(id, aviso);
    }

    // Regla de negocio: Solo se puede atender un aviso si fue previamente 'Aceptado'
    // y si el refugio cuenta con capacidad disponible para la mascota.
    public Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota)
    {
        var aviso = _avisoRepository.Get(avisoId);
        if (aviso is null) return null;

        // Solo permite continuar si el estado es 'Aceptado'
        if (aviso.Estado != EstadoAviso.Aceptado)
        {
            return null;
        }

        var mascotaCreada = _mascotaService.Post(datosMascota);
        if (mascotaCreada is null)
        {
            // No había capacidad disponible en el refugio
            return null;
        }

        mascotaCreada.AvisoRescateOrigenId = aviso.Id;
        aviso.MascotaGeneradaId = mascotaCreada.Id;
        aviso.Estado = EstadoAviso.Atendido;
        _avisoRepository.Put(aviso.Id, aviso);

        return mascotaCreada;
    }

    public bool Delete(int id) => _avisoRepository.Delete(id);
}