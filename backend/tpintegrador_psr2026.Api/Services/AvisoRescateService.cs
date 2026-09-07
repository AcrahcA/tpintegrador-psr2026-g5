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

    public List<AvisoRescate> ObtenerAvisos() => _avisoRepository.ObtenerTodos();

    // Devuelve los avisos cuyo estado sea Reportado / Pendiente
    public List<AvisoRescate> ObtenerPendientes()
    {
        return _avisoRepository.ObtenerTodos()
            .Where(a => a.Estado == EstadoAviso.Reportado)
            .ToList();
    }

    public AvisoRescate? BuscarAviso(int id) => _avisoRepository.ObtenerPorId(id);

    public AvisoRescate RegistrarAviso(AvisoRescate aviso)
    {
        aviso.Estado = EstadoAviso.Reportado;
        return _avisoRepository.Agregar(aviso);
    }

    public bool CambiarEstado(int id, EstadoAviso nuevoEstado)
    {
        var aviso = _avisoRepository.ObtenerPorId(id);
        if (aviso is null) return false;

        aviso.Estado = nuevoEstado;
        return _avisoRepository.Actualizar(id, aviso);
    }

    // Cambia el estado del aviso a Atendido
    public bool AceptarAviso(int id)
    {
        return CambiarEstado(id, EstadoAviso.Atendido);
    }

    // Cambia el estado del aviso a Descartado
    public bool DescartarAviso(int id)
    {
        return CambiarEstado(id, EstadoAviso.Descartado);
    }

    // Regla: un aviso de rescate atendido podra generar el ingreso de una nueva mascota,
    // siempre que el refugio tenga capacidad disponible.
    public Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota)
    {
        var aviso = _avisoRepository.ObtenerPorId(avisoId);
        if (aviso is null) return null;

        if (aviso.Estado is EstadoAviso.Descartado or EstadoAviso.Atendido)
            return null;

        var mascotaCreada = _mascotaService.IngresarMascota(datosMascota);
        if (mascotaCreada is null)
        {
            // No habia capacidad disponible en el refugio
            return null;
        }

        mascotaCreada.AvisoRescateOrigenId = aviso.Id;
        aviso.MascotaGeneradaId = mascotaCreada.Id;
        aviso.Estado = EstadoAviso.Atendido;
        _avisoRepository.Actualizar(aviso.Id, aviso);

        return mascotaCreada;
    }

    public bool EliminarAviso(int id) => _avisoRepository.Eliminar(id);
}