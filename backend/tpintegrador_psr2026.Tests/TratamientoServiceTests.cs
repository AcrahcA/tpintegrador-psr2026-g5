using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class TestFakeTratamientoRepo : ITratamientoRepository
{
    private readonly List<Tratamiento> _data = new();
    public List<Tratamiento> Get() => _data;
    public Tratamiento? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Tratamiento Post(Tratamiento t) { t.Id = _data.Count + 1; _data.Add(t); return t; }
    public bool Put(int id, Tratamiento t) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; t.Id = id; _data[idx] = t; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeHistorialRepo : IHistorialSanitarioRepository
{
    private readonly List<HistorialSanitario> _data = new();
    public List<HistorialSanitario> Get() => _data;
    public HistorialSanitario? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public HistorialSanitario Post(HistorialSanitario h) { h.Id = _data.Count + 1; _data.Add(h); return h; }
    public bool Put(int id, HistorialSanitario h) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; h.Id = id; _data[idx] = h; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeMascotaRepoForTratamiento : IMascotaRepository
{
    private readonly List<Mascota> _data = new();
    public List<Mascota> Get() => _data;
    public Mascota? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Mascota Post(Mascota m) { m.Id = _data.Count + 1; _data.Add(m); return m; }
    public bool Put(int id, Mascota m) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; m.Id = id; _data[idx] = m; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TratamientoServiceTests
{
    [Fact]
    public void Validar_Tratamiento_E_HistorialSanitario_AutomatizanProcesos()
    {
        var tratamientoRepo = new TestFakeTratamientoRepo();
        var historialRepo = new TestFakeHistorialRepo();
        var mascotaRepo = new TestFakeMascotaRepoForTratamiento();

        var mascota = new Mascota("Luna", "Gato", 3, Sexo.Hembra, Tamaño.Pequenio, "Siamés", DateTime.Now, EstadoMascota.Ingresada, null, null, null);
        var mascotaCreada = mascotaRepo.Post(mascota);

        var service = new TratamientoService(tratamientoRepo, historialRepo, mascotaRepo);

        // Usando constructor real de Tratamiento: (historialSanitarioId, tipoTratamiento, descripcion, fechaInicio, fechaFin, estado)
        var nuevoTratamiento = new Tratamiento(1, TipoTratamiento.Vacunacion, "Vacuna antirrábica", DateTime.Now, null, EstadoTratamiento.Pendiente);
        var tratamiento = service.Post(mascotaCreada.Id, nuevoTratamiento);

        Assert.NotNull(tratamiento);
        Assert.Single(historialRepo.Get());
    }
}