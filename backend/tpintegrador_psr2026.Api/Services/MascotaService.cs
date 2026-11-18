namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotas;
    private readonly IRefugioRepository _refugio;
    private readonly ICuidadorRepository _cuidadores;
    private readonly IHistorialSanitarioRepository _historiales;
    private readonly ITratamientoRepository _tratamientos;
    private readonly ISolicitudAdopcionRepository? _solicitudes;
    private readonly IAvisoRescateRepository? _avisos;
    private readonly IOperacionAtomica? _atomica;

    public MascotaService(IMascotaRepository mascotaRepository,
        IRefugioRepository refugioRepository, ICuidadorRepository cuidadorRepository,
        IHistorialSanitarioRepository historialRepository,
        ITratamientoRepository tratamientoRepository,
        ISolicitudAdopcionRepository? solicitudRepository = null,
        IAvisoRescateRepository? avisoRepository = null,
        IOperacionAtomica? operacionAtomica = null)
    {
        _mascotas = mascotaRepository;
        _refugio = refugioRepository;
        _cuidadores = cuidadorRepository;
        _historiales = historialRepository;
        _tratamientos = tratamientoRepository;
        _solicitudes = solicitudRepository;
        _avisos = avisoRepository;
        _atomica = operacionAtomica;
    }

    public List<Mascota> Get() => _mascotas.Get();
    public Mascota? GetById(int id) => _mascotas.GetPorId(id);

    public Mascota? Post(Mascota mascota)
    {
        using var transaccion = _atomica?.Iniciar();
        if (string.IsNullOrWhiteSpace(mascota.Nombre) ||
            string.IsNullOrWhiteSpace(mascota.Especie) || mascota.EdadAproximada < 0)
            return null;
        var config = _refugio.Get().FirstOrDefault();
        if (config is not null && _mascotas.Get().Count(m => m.Estado != EstadoMascota.Adoptada) >= config.CapacidadMaxima)
            return null;
        mascota.Nombre = mascota.Nombre.Trim();
        mascota.FechaIngreso = DateTime.UtcNow;
        mascota.Estado = EstadoMascota.Ingresada;
        mascota.RefugioId = config?.Id;
        mascota.CuidadorId = null;
        mascota.FotoUrl = null;
        var creada = _mascotas.Post(mascota);
        transaccion?.Confirmar();
        return creada;
    }

    public bool Actualizar(int id, Mascota datos)
    {
        var actual = GetById(id);
        if (actual is null || actual.Estado == EstadoMascota.Adoptada ||
            string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Especie) || datos.EdadAproximada < 0)
            return false;
        actual.Nombre = datos.Nombre.Trim();
        actual.Especie = datos.Especie.Trim();
        actual.Raza = datos.Raza;
        actual.EdadAproximada = datos.EdadAproximada;
        actual.Sexo = datos.Sexo;
        actual.Tamaño = datos.Tamaño;
        return _mascotas.Put(id, actual);
    }

    public bool PutEstado(int id, EstadoMascota estado)
    {
        var mascota = GetById(id);
        if (mascota is null) return false;
        if (mascota.Estado == estado) return true;
        // Reservada y Adoptada SOLO se asignan mediante las operaciones de adopción.
        if (estado is EstadoMascota.Reservada or EstadoMascota.Adoptada ||
            mascota.Estado is EstadoMascota.Reservada or EstadoMascota.Adoptada)
            return false;
        if (estado == EstadoMascota.DisponibleAdopcion && !CumpleCondicionesSanitarias(id)) return false;
        // No retroceder artificialmente la mascota a recién ingresada.
        if (estado == EstadoMascota.Ingresada) return false;
        mascota.Estado = estado;
        return _mascotas.Put(id, mascota);
    }

    public bool AsignarCuidador(int mascotaId, int cuidadorId)
    {
        var mascota = GetById(mascotaId);
        var cuidador = _cuidadores.GetPorId(cuidadorId);
        if (mascota is null || cuidador is null || mascota.Estado == EstadoMascota.Adoptada ||
            cuidador.CapacidadMaxima <= 0) return false;
        if (mascota.CuidadorId == cuidadorId) return true; // reasignación idéntica no consume otro cupo
        var ocupacion = _mascotas.Get().Count(m => m.CuidadorId == cuidadorId && m.Estado != EstadoMascota.Adoptada);
        if (ocupacion >= cuidador.CapacidadMaxima) return false;
        mascota.CuidadorId = cuidadorId;
        return _mascotas.Put(mascotaId, mascota);
    }

    public bool QuitarCuidador(int mascotaId)
    {
        var mascota = GetById(mascotaId);
        if (mascota is null || mascota.Estado == EstadoMascota.Adoptada) return false;
        mascota.CuidadorId = null;
        return _mascotas.Put(mascota.Id, mascota);
    }

    public bool EstaDisponibleParaAdopcion(int mascotaId) =>
        GetById(mascotaId)?.Estado == EstadoMascota.DisponibleAdopcion && CumpleCondicionesSanitarias(mascotaId);

    public bool CumpleCondicionesSanitarias(int mascotaId) =>
        ReglasSanitarias.Cumple(mascotaId, _historiales.Get(), _tratamientos.Get());

    public bool Delete(int id)
    {
        var mascota = GetById(id);
        if (mascota is null || mascota.Estado == EstadoMascota.Adoptada ||
            _historiales.Get().Any(h => h.MascotaId == id) ||
            (_solicitudes?.Get().Any(s => s.MascotaId == id) ?? false) ||
            (_avisos?.Get().Any(a => a.MascotaGeneradaId == id) ?? false)) return false;
        return _mascotas.Delete(id);
    }
}
