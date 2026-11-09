using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class FakeAdoptanteRepository : IAdoptanteRepository
{
    private readonly List<Adoptante> _data = new();
    public List<Adoptante> Get() => _data;
    public Adoptante? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Adoptante Post(Adoptante a) { a.Id = _data.Count + 1; _data.Add(a); return a; }
    public bool Put(int id, Adoptante a) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; a.Id = id; _data[idx] = a; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class AdoptanteServiceTests
{
    [Fact]
    public void Validar_Adoptante_RequiereDatosYdniUnico()
    {
        var repo = new FakeAdoptanteRepository();
        var service = new AdoptanteService(repo);

        var a1 = service.Post(new Adoptante("Lucia Gomez", "32111222", "11554433", "Rivadavia 100"));
        Assert.NotNull(a1);

        var aDuplicado = service.Post(new Adoptante("Otro Nombre", "32111222", "11889900", "Belgrano 200"));
        Assert.Null(aDuplicado);
    }
}