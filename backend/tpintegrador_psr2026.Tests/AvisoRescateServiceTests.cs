using Xunit;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Tests;

public class AvisoRescateServiceTests
{
    private static AvisoRescate NuevoAviso() =>
        new("Perrito perdido", TipoAnimal.Perro, -34.6, -58.3, "Vecina", EstadoAparente.Herido, NivelUrgencia.Alta);

    [Fact]
    public void AvisoNuevo_QuedaReportadoYPendiente()
    {
        var s = new Sistema();
        var aviso = s.Avisos.Registrar(NuevoAviso());

        Assert.Equal(EstadoAviso.Reportado, aviso.Estado);
        Assert.Single(s.Avisos.GetPendientes());
    }

    [Fact]
    public void SoloUnAvisoAceptado_GeneraElIngresoDeUnaMascota()
    {
        var s = new Sistema();
        var aviso = s.Avisos.Registrar(NuevoAviso());
        var datos = new Mascota("Pipo", TipoAnimal.Perro, 1, Sexo.Macho, Tamaño.Pequenio);

        Assert.Throws<ReglaNegocioException>(() => s.Avisos.AtenderAvisoYGenerarMascota(aviso.Id, datos));

        s.Avisos.Aceptar(aviso.Id);
        var mascota = s.Avisos.AtenderAvisoYGenerarMascota(aviso.Id, datos);

        Assert.Equal(EstadoAviso.Atendido, aviso.Estado);
        Assert.Equal(mascota.Id, aviso.MascotaGeneradaId);
        Assert.Equal(aviso.Id, mascota.AvisoRescateOrigenId);
        Assert.Equal(EstadoMascota.Ingresada, mascota.Estado);
    }

    [Fact]
    public void NoSeAceptaUnAviso_SiNoHayLugarEnElRefugio()
    {
        var s = new Sistema(capacidadRefugio: 1);
        s.NuevaMascota();
        var aviso = s.Avisos.Registrar(NuevoAviso());

        Assert.Throws<ReglaNegocioException>(() => s.Avisos.Aceptar(aviso.Id));
        Assert.Equal(EstadoAviso.Reportado, aviso.Estado); // queda pendiente
    }

    [Fact]
    public void AvisoDescartado_EsEstadoFinal()
    {
        var s = new Sistema();
        var aviso = s.Avisos.Registrar(NuevoAviso());
        s.Avisos.Descartar(aviso.Id);

        Assert.Throws<ReglaNegocioException>(() => s.Avisos.Aceptar(aviso.Id));
        Assert.Throws<ReglaNegocioException>(() => s.Avisos.CambiarEstado(aviso.Id, EstadoAviso.Atendido));
    }

    [Fact]
    public void CoordenadasInvalidas_SonRechazadas()
    {
        var s = new Sistema();
        var aviso = new AvisoRescate("x", TipoAnimal.Gato, 120, 0, "J", EstadoAparente.Sano, NivelUrgencia.Baja);
        Assert.Throws<ReglaNegocioException>(() => s.Avisos.Registrar(aviso));
    }
}
