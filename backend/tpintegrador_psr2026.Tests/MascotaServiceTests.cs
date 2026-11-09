using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class TestFakeMascotaRepo : IMascotaRepository
{
    private readonly List<Mascota> _data = new();
    public List<Mascota> Get() => _data;
    public Mascota? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Mascota Post(Mascota m) { m.Id = _data.Count + 1; _data.Add(m); return m; }
    public bool Put(int id, Mascota m) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; m.Id = id; _data[idx] = m; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeRefugioRepo : IRefugioRepository
{
    private readonly List<Refugio> _data = new();
    public List<Refugio> Get() => _data;
    public Refugio? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Refugio Post(Refugio r) { _data.Clear(); _data.Add(r); return r; }
    public bool Put(int id, Refugio r) { _data.Clear(); _data.Add(r); return true; }
    public bool Delete(int id) { _data.Clear(); return true; }
}

public class TestFakeCuidadorRepoForMascota : ICuidadorRepository
{
    private readonly List<Cuidador> _data = new();
    public List<Cuidador> Get() => _data;
    public Cuidador? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Cuidador Post(Cuidador c) { c.Id = _data.Count + 1; _data.Add(c); return c; }
    public bool Put(int id, Cuidador c) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; c.Id = id; _data[idx] = c; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeHistorialRepoForMascota : IHistorialSanitarioRepository
{
    private readonly List<HistorialSanitario> _data = new();
    public List<HistorialSanitario> Get() => _data;
    public HistorialSanitario? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public HistorialSanitario Post(HistorialSanitario h) { h.Id = _data.Count + 1; _data.Add(h); return h; }
    public bool Put(int id, HistorialSanitario h) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; h.Id = id; _data[idx] = h; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeTratamientoRepoForMascota : ITratamientoRepository
{
    private readonly List<Tratamiento> _data = new();
    public List<Tratamiento> Get() => _data;
    public Tratamiento? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Tratamiento Post(Tratamiento t) { t.Id = _data.Count + 1; _data.Add(t); return t; }
    public bool Put(int id, Tratamiento t) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; t.Id = id; _data[idx] = t; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class MascotaServiceTests
{
    [Fact]
    public void Validar_Mascota_Y_Refugio_ControlanCapacidad()
    {
        var mascotaRepo = new TestFakeMascotaRepo();
        var refugioRepo = new TestFakeRefugioRepo();
        var cuidadorRepo = new TestFakeCuidadorRepoForMascota();
        var tratamientoRepo = new TestFakeTratamientoRepoForMascota();
        var historialRepo = new TestFakeHistorialRepoForMascota();

        // Usando constructor real de Refugio: (nombre, capacidadMax, ...)
        var refugio = new Refugio("Refugio Central", 1, null, null);
        refugioRepo.Post(refugio);

        var service = new MascotaService(mascotaRepo, refugioRepo, cuidadorRepo, historialRepo, tratamientoRepo);

        // Usando constructor real de Mascota: (nombre, especie (string o enum), edad, sexo, tamaño, ...)
        var m1 = new Mascota("Firulais", "Perro", 2, Sexo.Macho, Tamaño.Mediano, "Mestizo", DateTime.Now, EstadoMascota.Ingresada, null, null, null);
        var resultado1 = service.Post(m1);
        Assert.NotNull(resultado1);

        var m2 = new Mascota("Manchas", "Perro", 1, Sexo.Macho, Tamaño.Pequenio, "Caniche", DateTime.Now, EstadoMascota.Ingresada, null, null, null);
        var resultado2 = service.Post(m2);
        Assert.Null(resultado2); // Rechazado por refugio lleno
    }
}