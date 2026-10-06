namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IRefugioRepository _refugioRepository;
    private readonly ICuidadorRepository _cuidadorRepository;
    private readonly IHistorialSanitarioRepository _historialRepository;
    private readonly ITratamientoRepository _tratamientoRepository;

    public MascotaService(
        IMascotaRepository mascotaRepository,
        IRefugioRepository refugioRepository,
        ICuidadorRepository cuidadorRepository,
        IHistorialSanitarioRepository historialRepository,
        ITratamientoRepository tratamientoRepository)
    {
        _mascotaRepository = mascotaRepository;
        _refugioRepository = refugioRepository;
        _cuidadorRepository = cuidadorRepository;
        _historialRepository = historialRepository;
        _tratamientoRepository = tratamientoRepository;
    }

    public List<Mascota> Get() => _mascotaRepository.Get();

    public Mascota? GetById(int id) => _mascotaRepository.Get().FirstOrDefault(m => m.Id == id);

    public Mascota? Post(Mascota mascota)
    {
        var refugio = _refugioRepository.Get().FirstOrDefault();
        var cantidadActual = _mascotaRepository.Get().Count(m => m.Estado != EstadoMascota.Adoptada);

        // Validar capacidad máxima del refugio
        if (refugio is not null && cantidadActual >= refugio.CapacidadMaxima)
            return null;

        mascota.FechaIngreso = DateTime.Now;
        mascota.Estado = EstadoMascota.Ingresada;
        return _mascotaRepository.Post(mascota);
    }

    public bool PutEstado(int id, EstadoMascota nuevoEstado)
    {
        var mascota = GetById(id);
        if (mascota is null) return false;

        // Regla: Adoptada es un estado final, no puede volver a otro estado
        if (mascota.Estado == EstadoMascota.Adoptada)
            return false;

        // Regla: Para estar disponible para adopción debe cumplir condiciones sanitarias
        if (nuevoEstado == EstadoMascota.DisponibleAdopcion && !CumpleCondicionesSanitarias(id))
            return false;

        mascota.Estado = nuevoEstado;
        return _mascotaRepository.Put(id, mascota);
    }

    public bool AsignarCuidador(int mascotaId, int cuidadorId)
    {
        var mascota = GetById(mascotaId);
        var cuidador = _cuidadorRepository.Get().FirstOrDefault(c => c.Id == cuidadorId);

        if (mascota is null || cuidador is null) return false;

        // Regla: Una mascota adoptada no requiere ni debe reasignar cuidador activo
        if (mascota.Estado == EstadoMascota.Adoptada) return false;

        // Validar capacidad del cuidador (descontando las mascotas ya adoptadas)
        var asignadas = _mascotaRepository.Get().Count(m => m.CuidadorId == cuidadorId && m.Estado != EstadoMascota.Adoptada);
        if (asignadas >= cuidador.CapacidadMaxima) return false;

        mascota.CuidadorId = cuidadorId;
        return _mascotaRepository.Put(mascotaId, mascota);
    }

    public bool EstaDisponibleParaAdopcion(int mascotaId)
    {
        var mascota = GetById(mascotaId);
        if (mascota is null) return false;

        return mascota.Estado == EstadoMascota.DisponibleAdopcion && CumpleCondicionesSanitarias(mascotaId);
    }

    public bool CumpleCondicionesSanitarias(int mascotaId)
    {
        var historial = _historialRepository.Get().FirstOrDefault(h => h.MascotaId == mascotaId);
        if (historial is null) return true;

        var tieneTratamientoIncompatible = _tratamientoRepository.Get()
            .Any(t => t.HistorialSanitarioId == historial.Id && 
                     (t.Estado == EstadoTratamiento.EnCurso || t.Estado == EstadoTratamiento.Pendiente));

        return !tieneTratamientoIncompatible;
    }

    public bool Delete(int id) => _mascotaRepository.Delete(id);
}