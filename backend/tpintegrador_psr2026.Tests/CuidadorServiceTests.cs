using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class TestFakeCuidadorRepo : ICuidadorRepository
{
    private readonly List<Cuidador> _data = new();
    public List<Cuidador> Get() => _data;
    public Cuidador? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Cuidador Post(Cuidador c) { c.Id = _data.Count + 1; _data.Add(c); return c; }
    public bool Put(int id, Cuidador c) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; c.Id = id; _data[idx] = c; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeMascotaRepoForCuidador : IMascotaRepository
{
    private readonly List<Mascota> _data = new();
    public List<Mascota> Get() => _data;
    public Mascota? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Mascota Post(Mascota m) { m.Id = _data.Count + 1; _data.Add(m); return m; }
    public bool Put(int id, Mascota m) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; m.Id = id; _data[idx] = m; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class CuidadorServiceTests
{
    [Fact]
    public void Validar_Cuidador_RespetaCapacidadMaxima()
    {
        var cuidadorRepo = new TestFakeCuidadorRepo();
        var mascotaRepo = new TestFakeMascotaRepoForCuidador();

        var cuidador = cuidadorRepo.Post(new Cuidador("Carlos Perez", 1, "Perros"));
        
        // Pasamos "Perro" como string en lugar de TipoAnimal
        var mascota = new Mascota("Rocco", "Perro", 4, Sexo.Macho, Tamaño.Grande, "Ovejero", DateTime.Now, EstadoMascota.Ingresada, null, null, null);
        var mascotaCreada = mascotaRepo.Post(mascota);

        var service = new CuidadorService(cuidadorRepo, mascotaRepo);

        var asignado = service.AsignarMascota(cuidador.Id, mascotaCreada.Id);
        Assert.True(asignado);
    }
}