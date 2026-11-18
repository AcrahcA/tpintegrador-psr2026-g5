using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

/// <summary>"Base de datos" en memoria compartida por todos los repositorios falsos.</summary>
public class BaseEnMemoria
{
    public List<Refugio> Refugios { get; } = new();
    public List<Mascota> Mascotas { get; } = new();
    public List<Cuidador> Cuidadores { get; } = new();
    public List<AvisoRescate> Avisos { get; } = new();
    public List<HistorialSanitario> Historiales { get; } = new();
    public List<Tratamiento> Tratamientos { get; } = new();
    public List<Adoptante> Adoptantes { get; } = new();
    public List<SolicitudAdopcion> Solicitudes { get; } = new();
    public List<Adopcion> Adopciones { get; } = new();
}

public class RepoEnMemoria<T> : IRepositorio<T> where T : class, IEntidad
{
    protected readonly List<T> Datos;
    public RepoEnMemoria(List<T> datos) => Datos = datos;
    public List<T> Get() => Datos.ToList();
    public T? GetPorId(int id) => Datos.FirstOrDefault(x => x.Id == id);
    public T Post(T e) { e.Id = Datos.Count == 0 ? 1 : Datos.Max(x => x.Id) + 1; Datos.Add(e); return e; }
    public bool Put(int id, T e)
    {
        var i = Datos.FindIndex(x => x.Id == id);
        if (i < 0) return false;
        e.Id = id; Datos[i] = e; return true;
    }
    public bool Delete(int id) => Datos.RemoveAll(x => x.Id == id) > 0;
}

public class FakeRefugioRepo : RepoEnMemoria<Refugio>, IRefugioRepository
{
    public FakeRefugioRepo(BaseEnMemoria db) : base(db.Refugios) { }
    public Refugio? GetActual() => Datos.FirstOrDefault();
}

public class FakeMascotaRepo : RepoEnMemoria<Mascota>, IMascotaRepository
{
    public FakeMascotaRepo(BaseEnMemoria db) : base(db.Mascotas) { }
    public List<Mascota> GetPorEstado(EstadoMascota estado) => Datos.Where(m => m.Estado == estado).ToList();
    public List<Mascota> GetAlojadas() => Datos.Where(m => m.EstaAlojada).ToList();
    public int ContarAlojadas() => Datos.Count(m => m.EstaAlojada);
    public List<Mascota> GetPorCuidador(int cuidadorId, bool incluirAdoptadas = false) =>
        Datos.Where(m => m.CuidadorId == cuidadorId && (incluirAdoptadas || m.EstaAlojada)).ToList();
    public int ContarAsignadasACuidador(int cuidadorId) => GetPorCuidador(cuidadorId).Count;
}

public class FakeCuidadorRepo : RepoEnMemoria<Cuidador>, ICuidadorRepository
{
    public FakeCuidadorRepo(BaseEnMemoria db) : base(db.Cuidadores) { }
    public List<Cuidador> GetDisponibles() => Datos.Where(c => c.Disponible).ToList();
}

public class FakeAvisoRepo : RepoEnMemoria<AvisoRescate>, IAvisoRescateRepository
{
    public FakeAvisoRepo(BaseEnMemoria db) : base(db.Avisos) { }
    public List<AvisoRescate> GetPorEstado(EstadoAviso estado) => Datos.Where(a => a.Estado == estado).ToList();
    public List<AvisoRescate> GetPendientes() => Datos.Where(a => a.EstaPendiente).ToList();
}

public class FakeHistorialRepo : RepoEnMemoria<HistorialSanitario>, IHistorialSanitarioRepository
{
    public FakeHistorialRepo(BaseEnMemoria db) : base(db.Historiales) { }
    public HistorialSanitario? GetPorMascota(int mascotaId) => Datos.FirstOrDefault(h => h.MascotaId == mascotaId);
}

public class FakeTratamientoRepo : RepoEnMemoria<Tratamiento>, ITratamientoRepository
{
    private readonly BaseEnMemoria _db;
    public FakeTratamientoRepo(BaseEnMemoria db) : base(db.Tratamientos) => _db = db;
    public List<Tratamiento> GetPorMascota(int mascotaId)
    {
        var h = _db.Historiales.FirstOrDefault(x => x.MascotaId == mascotaId);
        return h is null ? new() : GetPorHistorial(h.Id);
    }
    public List<Tratamiento> GetPorHistorial(int historialSanitarioId) =>
        Datos.Where(t => t.HistorialSanitarioId == historialSanitarioId).ToList();
    public bool TieneTratamientosActivos(int mascotaId) => GetPorMascota(mascotaId).Any(t => t.EstaActivo);
}

public class FakeAdoptanteRepo : RepoEnMemoria<Adoptante>, IAdoptanteRepository
{
    public FakeAdoptanteRepo(BaseEnMemoria db) : base(db.Adoptantes) { }
    public Adoptante? GetPorDni(string dni) => Datos.FirstOrDefault(a => a.Dni == dni);
}

public class FakeSolicitudRepo : RepoEnMemoria<SolicitudAdopcion>, ISolicitudAdopcionRepository
{
    public FakeSolicitudRepo(BaseEnMemoria db) : base(db.Solicitudes) { }
    public List<SolicitudAdopcion> GetPorEstado(params EstadoSolicitud[] estados) => Datos.Where(s => estados.Contains(s.Estado)).ToList();
    public List<SolicitudAdopcion> GetPorMascota(int mascotaId) => Datos.Where(s => s.MascotaId == mascotaId).ToList();
    public List<SolicitudAdopcion> GetPorAdoptante(int adoptanteId) => Datos.Where(s => s.AdoptanteId == adoptanteId).ToList();
}

public class FakeAdopcionRepo : IAdopcionRepository
{
    private readonly List<Adopcion> _datos;
    public FakeAdopcionRepo(BaseEnMemoria db) => _datos = db.Adopciones;
    public List<Adopcion> Get() => _datos.ToList();
    public Adopcion? GetPorId(int id) => _datos.FirstOrDefault(a => a.Id == id);
    public Adopcion Post(Adopcion a) { a.Id = _datos.Count + 1; _datos.Add(a); return a; }
    public Adopcion? GetPorMascota(int mascotaId) => _datos.FirstOrDefault(a => a.MascotaId == mascotaId);
    public List<Adopcion> GetPorAdoptante(int adoptanteId) => _datos.Where(a => a.AdoptanteId == adoptanteId).ToList();
    public bool ExisteAdopcionDeMascota(int mascotaId) => _datos.Any(a => a.MascotaId == mascotaId);
    public bool ExisteAdopcionDeSolicitud(int solicitudId) => _datos.Any(a => a.SolicitudAdopcionId == solicitudId);
}

public class FakeUnidadDeTrabajo : IUnidadDeTrabajo
{
    public T EjecutarEnTransaccion<T>(Func<T> operacion) => operacion();
}

/// <summary>Arma todos los services reales sobre los repositorios en memoria.</summary>
public class Sistema
{
    public BaseEnMemoria Db { get; } = new();
    public IRefugioService Refugio { get; }
    public IMascotaService Mascotas { get; }
    public ICuidadorService Cuidadores { get; }
    public IAvisoRescateService Avisos { get; }
    public IHistorialSanitarioService Historiales { get; }
    public ITratamientoService Tratamientos { get; }
    public IAdoptanteService Adoptantes { get; }
    public ISolicitudAdopcionService Solicitudes { get; }
    public IAdopcionService Adopciones { get; }

    public Sistema(int capacidadRefugio = 10)
    {
        var uow = new FakeUnidadDeTrabajo();
        var refugioRepo = new FakeRefugioRepo(Db);
        var mascotaRepo = new FakeMascotaRepo(Db);
        var cuidadorRepo = new FakeCuidadorRepo(Db);
        var avisoRepo = new FakeAvisoRepo(Db);
        var historialRepo = new FakeHistorialRepo(Db);
        var tratamientoRepo = new FakeTratamientoRepo(Db);
        var adoptanteRepo = new FakeAdoptanteRepo(Db);
        var solicitudRepo = new FakeSolicitudRepo(Db);
        var adopcionRepo = new FakeAdopcionRepo(Db);

        refugioRepo.Post(new Refugio("Refugio de prueba", capacidadRefugio));

        Refugio = new RefugioService(refugioRepo, mascotaRepo);
        Mascotas = new MascotaService(mascotaRepo, Refugio, cuidadorRepo, historialRepo, tratamientoRepo, solicitudRepo, uow);
        Cuidadores = new CuidadorService(cuidadorRepo, mascotaRepo, Refugio);
        Avisos = new AvisoRescateService(avisoRepo, Mascotas, Refugio, uow);
        Historiales = new HistorialSanitarioService(historialRepo, mascotaRepo);
        Tratamientos = new TratamientoService(tratamientoRepo, Historiales, historialRepo, mascotaRepo, uow);
        Adoptantes = new AdoptanteService(adoptanteRepo, solicitudRepo);
        Solicitudes = new SolicitudAdopcionService(solicitudRepo, adoptanteRepo, Adoptantes, mascotaRepo, Mascotas, adopcionRepo, uow);
        Adopciones = new AdopcionService(adopcionRepo, solicitudRepo, mascotaRepo, Mascotas, uow);
    }

    public Mascota NuevaMascota(string nombre = "Firulais", TipoAnimal especie = TipoAnimal.Perro) =>
        Mascotas.Registrar(new Mascota(nombre, especie, 2, Sexo.Macho, Tamaño.Mediano));

    /// <summary>Mascota con vacuna y control finalizados, habilitada para adopción.</summary>
    public Mascota MascotaDisponible(string nombre = "Miel")
    {
        var m = NuevaMascota(nombre);
        Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.Vacunacion, estado: EstadoTratamiento.Finalizado));
        Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.ControlVeterinario, estado: EstadoTratamiento.Finalizado));
        return Mascotas.CambiarEstado(m.Id, EstadoMascota.DisponibleAdopcion);
    }

    public Adoptante NuevoAdoptante(string dni = "30111222") => Adoptantes.Registrar(new Adoptante("Laura", dni, "11 5555"));
}
