using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

// Pruebas de reglas con repositorios dobles; no sustituyen un test de integración PostgreSQL.
public class FakeSolicitudRepoIntegracion : ISolicitudAdopcionRepository
{
    private readonly List<SolicitudAdopcion> _items = new();
    public List<SolicitudAdopcion> Get() => _items;
    public SolicitudAdopcion? GetPorId(int id) => _items.FirstOrDefault(x => x.Id == id);
    public SolicitudAdopcion Post(SolicitudAdopcion x) { x.Id = _items.Count == 0 ? 1 : _items.Max(y => y.Id) + 1; _items.Add(x); return x; }
    public bool Put(int id, SolicitudAdopcion x) { var i = _items.FindIndex(y => y.Id == id); if (i < 0) return false; _items[i] = x; return true; }
    public bool Delete(int id) => _items.RemoveAll(x => x.Id == id) > 0;
}

public class FakeAdopcionRepoIntegracion : IAdopcionRepository
{
    private readonly List<Adopcion> _items = new();
    public List<Adopcion> Get() => _items;
    public Adopcion? GetPorId(int id) => _items.FirstOrDefault(x => x.Id == id);
    public Adopcion Post(Adopcion x) { x.Id = _items.Count == 0 ? 1 : _items.Max(y => y.Id) + 1; _items.Add(x); return x; }
    public bool Put(int id, Adopcion x) { var i = _items.FindIndex(y => y.Id == id); if (i < 0) return false; _items[i] = x; return true; }
    public bool Delete(int id) => _items.RemoveAll(x => x.Id == id) > 0;
}

public class ReglasNegocioIntegracionTests
{
    private static Mascota MascotaEjemplo() => new("Luna", "Gato", 3, Sexo.Hembra, Tamaño.Mediano);

    [Fact]
    public void Mascota_SinHistorial_NoPuedeHabilitarAdopcionNiMarcarseAdoptada()
    {
        var m = new TestFakeMascotaRepo();
        var mascota = m.Post(MascotaEjemplo());
        var service = new MascotaService(m, new TestFakeRefugioRepo(),
            new TestFakeCuidadorRepoForMascota(), new TestFakeHistorialRepoForMascota(),
            new TestFakeTratamientoRepoForMascota());
        Assert.False(service.CumpleCondicionesSanitarias(mascota.Id));
        Assert.False(service.PutEstado(mascota.Id, EstadoMascota.DisponibleAdopcion));
        Assert.False(service.PutEstado(mascota.Id, EstadoMascota.Adoptada));
        Assert.Equal(EstadoMascota.Ingresada, m.GetPorId(mascota.Id)!.Estado);
    }

    [Fact]
    public void Historial_RequiereControlOvacunaFinalizada_YNingunTratamientoActivo()
    {
        var h = new TestFakeHistorialRepoForMascota();
        var t = new TestFakeTratamientoRepoForMascota();
        var historia = h.Post(new HistorialSanitario(5));
        Assert.False(ReglasSanitarias.Cumple(5, h.Get(), t.Get()));
        var control = t.Post(new Tratamiento(historia.Id, TipoTratamiento.ControlVeterinario));
        control.Estado = EstadoTratamiento.Finalizado;
        Assert.True(ReglasSanitarias.Cumple(5, h.Get(), t.Get()));
        var activo = t.Post(new Tratamiento(historia.Id, TipoTratamiento.Medicacion));
        activo.Estado = EstadoTratamiento.EnCurso;
        Assert.False(ReglasSanitarias.Cumple(5, h.Get(), t.Get()));
    }

    [Fact]
    public void TratamientoFinalizado_PasaARecuperacion_NoHabilitaAdopcionAutomaticamente()
    {
        var m = new TestFakeMascotaRepoForTratamiento();
        var h = new TestFakeHistorialRepo();
        var t = new TestFakeTratamientoRepo();
        var mascota = m.Post(MascotaEjemplo());
        var service = new TratamientoService(t, h, m);
        var tr = service.Post(mascota.Id, new Tratamiento(0, TipoTratamiento.Vacunacion));
        Assert.NotNull(tr);
        Assert.True(service.PutEstado(tr!.Id, EstadoTratamiento.Finalizado));
        Assert.Equal(EstadoMascota.EnRecuperacion, m.GetPorId(mascota.Id)!.Estado);
    }

    [Fact]
    public void AprobarSolicitud_ReservaMascota_BloqueaSegundaAprobacion_YAdopcionEsDefinitiva()
    {
        var m = new TestFakeMascotaRepo();
        var h = new TestFakeHistorialRepoForMascota();
        var t = new TestFakeTratamientoRepoForMascota();
        var adoptantes = new FakeAdoptanteRepository();
        var solicitudes = new FakeSolicitudRepoIntegracion();
        var adopciones = new FakeAdopcionRepoIntegracion();
        var mascota = m.Post(MascotaEjemplo());
        var historia = h.Post(new HistorialSanitario(mascota.Id));
        var control = t.Post(new Tratamiento(historia.Id, TipoTratamiento.ControlVeterinario));
        control.Estado = EstadoTratamiento.Finalizado;
        mascota.Estado = EstadoMascota.DisponibleAdopcion;
        var a1 = adoptantes.Post(new Adoptante("Ana", "30000001", "1111111"));
        var a2 = adoptantes.Post(new Adoptante("Beto", "30000002", "2222222"));
        var s = new SolicitudAdopcionService(solicitudes, adoptantes, m, t, h);
        var req1 = s.Post(a1.Id, mascota.Id);
        var req2 = s.Post(a2.Id, mascota.Id);
        Assert.NotNull(req1);
        Assert.NotNull(req2);
        Assert.True(s.PutEstado(req1!.Id, EstadoSolicitud.Aprobada));
        Assert.Equal(EstadoMascota.Reservada, m.GetPorId(mascota.Id)!.Estado);
        Assert.False(s.PutEstado(req2!.Id, EstadoSolicitud.Aprobada));
        var adop = new AdopcionService(adopciones, solicitudes, m, h, t);
        Assert.NotNull(adop.Post(req1.Id, "Visita final OK"));
        Assert.Null(adop.Post(req1.Id, "Duplicado"));
        Assert.Equal(EstadoMascota.Adoptada, m.GetPorId(mascota.Id)!.Estado);
        Assert.False(s.EsMascotaAptaParaAdopcion(mascota.Id));
        var mascotasService = new MascotaService(m, new TestFakeRefugioRepo(),
            new TestFakeCuidadorRepoForMascota(), h, t, solicitudes);
        Assert.False(mascotasService.Delete(mascota.Id));
        Assert.NotNull(m.GetPorId(mascota.Id));
    }

    [Fact]
    public void CancelarSolicitudAprobada_LiberaReserva()
    {
        var m = new TestFakeMascotaRepo();
        var h = new TestFakeHistorialRepoForMascota();
        var t = new TestFakeTratamientoRepoForMascota();
        var a = new FakeAdoptanteRepository();
        var s = new FakeSolicitudRepoIntegracion();
        var pet = m.Post(MascotaEjemplo());
        var hist = h.Post(new HistorialSanitario(pet.Id));
        var tr = t.Post(new Tratamiento(hist.Id, TipoTratamiento.Vacunacion));
        tr.Estado = EstadoTratamiento.Finalizado;
        pet.Estado = EstadoMascota.DisponibleAdopcion;
        var person = a.Post(new Adoptante("Luz", "33333333", "11111111"));
        var service = new SolicitudAdopcionService(s, a, m, t, h);
        var req = service.Post(person.Id, pet.Id)!;
        Assert.True(service.PutEstado(req.Id, EstadoSolicitud.Aprobada));
        Assert.True(service.PutEstado(req.Id, EstadoSolicitud.Cancelada));
        Assert.Equal(EstadoMascota.DisponibleAdopcion, m.GetPorId(pet.Id)!.Estado);
    }

    [Fact]
    public void Refugio_NoPermiteReducirCapacidadPorDebajoDeOcupacion()
    {
        var repo = new TestFakeRefugioRepo();
        var mascotas = new TestFakeMascotaRepo();
        mascotas.Post(MascotaEjemplo());
        mascotas.Post(new Mascota("Pipo", "Perro", 2, Sexo.Macho, Tamaño.Mediano));
        var servicio = new RefugioService(repo, mascotas);
        Assert.Throws<ArgumentException>(() => servicio.Post(new Refugio("Patitas", 1)));
        Assert.Equal(2, servicio.ContarMascotasActuales());
    }

    [Fact]
    public void Cuidador_NoDuplicaCupoAlReasignarLaMismaMascota()
    {
        var c = new TestFakeCuidadorRepo();
        var m = new TestFakeMascotaRepoForCuidador();
        var cuidador = c.Post(new Cuidador("Marta", 1));
        var mascota = m.Post(MascotaEjemplo());
        var service = new CuidadorService(c, m);
        Assert.True(service.AsignarMascota(cuidador.Id, mascota.Id));
        Assert.True(service.AsignarMascota(cuidador.Id, mascota.Id));
        Assert.False(service.AsignarMascota(cuidador.Id, m.Post(new Mascota("Otra", "Gato", 1, Sexo.Hembra, Tamaño.Pequenio)).Id));
        Assert.False(service.Put(cuidador.Id, new Cuidador("Marta", 0)));
    }

    [Fact]
    public void Aviso_AceptadoGeneraUnaMascotaYRelacionaAmbosLados()
    {
        var avisos = new TestFakeAvisoRepo();
        var mascotas = new TestFakeMascotaRepoForAviso();
        var refugio = new TestFakeRefugioRepoForAviso();
        refugio.Post(new Refugio("Centro", 3));
        var service = new AvisoRescateService(avisos, mascotas, refugio);
        var aviso = service.Post(new AvisoRescate("Encontrado", TipoAnimal.Perro, -34.6, -58.4,
            "Vecino", EstadoAparente.Herido, NivelUrgencia.Alta));
        Assert.False(service.PutEstado(aviso.Id, EstadoAviso.Atendido));
        Assert.True(service.PutEstado(aviso.Id, EstadoAviso.EnEvaluacion));
        Assert.True(service.PutEstado(aviso.Id, EstadoAviso.Aceptado));
        var ingresado = service.AtenderAvisoYGenerarMascota(aviso.Id, new Mascota("Rescate", "Perro", 1, Sexo.Macho, Tamaño.Mediano));
        Assert.NotNull(ingresado);
        Assert.Equal(aviso.Id, ingresado!.AvisoRescateOrigenId);
        Assert.Equal(ingresado.Id, avisos.GetPorId(aviso.Id)!.MascotaGeneradaId);
        Assert.Null(service.AtenderAvisoYGenerarMascota(aviso.Id, new Mascota("Doble", "Perro", 1, Sexo.Macho, Tamaño.Mediano)));
    }
}
