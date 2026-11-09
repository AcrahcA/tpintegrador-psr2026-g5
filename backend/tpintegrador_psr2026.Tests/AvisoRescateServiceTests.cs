using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class TestFakeAvisoRepo : IAvisoRescateRepository
{
    private readonly List<AvisoRescate> _data = new();
    public List<AvisoRescate> Get() => _data;
    public AvisoRescate? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public AvisoRescate Post(AvisoRescate a) { a.Id = _data.Count + 1; _data.Add(a); return a; }
    public bool Put(int id, AvisoRescate a) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; a.Id = id; _data[idx] = a; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeMascotaRepoForAviso : IMascotaRepository
{
    private readonly List<Mascota> _data = new();
    public List<Mascota> Get() => _data;
    public Mascota? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Mascota Post(Mascota m) { m.Id = _data.Count + 1; _data.Add(m); return m; }
    public bool Put(int id, Mascota m) { var idx = _data.FindIndex(x => x.Id == id); if (idx == -1) return false; m.Id = id; _data[idx] = m; return true; }
    public bool Delete(int id) => _data.RemoveAll(x => x.Id == id) > 0;
}

public class TestFakeRefugioRepoForAviso : IRefugioRepository
{
    private readonly List<Refugio> _data = new();
    public List<Refugio> Get() => _data;
    public Refugio? GetPorId(int id) => _data.FirstOrDefault(x => x.Id == id);
    public Refugio Post(Refugio r) { _data.Clear(); _data.Add(r); return r; }
    public bool Put(int id, Refugio r) { _data.Clear(); _data.Add(r); return true; }
    public bool Delete(int id) { _data.Clear(); return true; }
}

public class AvisoRescateServiceTests
{
    [Fact]
    public void Validar_SolicitudAdopcion_InstanciaAtributosCorrectamente()
    {
        var solicitud = new SolicitudAdopcion(adoptanteId: 2, mascotaId: 7);
        
        Assert.Equal(2, solicitud.AdoptanteId);
        Assert.Equal(7, solicitud.MascotaId);
        Assert.Equal(EstadoSolicitud.Pendiente, solicitud.Estado);
    }

    [Fact]
    public void Validar_AvisoRescate_GeneraMascotaAlAtender()
    {
        var avisoRepo = new TestFakeAvisoRepo();
        var mascotaRepo = new TestFakeMascotaRepoForAviso();
        var refugioRepo = new TestFakeRefugioRepoForAviso();

        var refugio = new Refugio("Refugio Sur", 5, null, null);
        refugioRepo.Post(refugio);
        
        // Constructor real de AvisoRescate
        var aviso = new AvisoRescate("Perrito perdido", TipoAnimal.Perro, -34.6, -58.3, "Plaza", EstadoAparente.Sano, NivelUrgencia.Media, null, DateTime.Now, EstadoAviso.Reportado, null);
        var avisoCreado = avisoRepo.Post(aviso);
        
        var service = new AvisoRescateService(avisoRepo, mascotaRepo, refugioRepo);
        var nuevaMascota = new Mascota("Pipo", "Perro", 1, Sexo.Macho, Tamaño.Pequenio, "Callejero", DateTime.Now, EstadoMascota.Ingresada, null, null, null);
        
        var resultado = service.AtenderAvisoYGenerarMascota(avisoCreado.Id, nuevaMascota);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoAviso.Atendido, avisoRepo.Get().First().Estado);
        Assert.Single(mascotaRepo.Get());
    }
}