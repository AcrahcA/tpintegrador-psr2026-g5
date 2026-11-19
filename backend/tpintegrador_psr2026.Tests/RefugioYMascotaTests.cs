using Xunit;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Tests;

public class RefugioYMascotaTests
{
    [Fact]
    public void NoIngresanMascotas_CuandoElRefugioEstaLleno()
    {
        var s = new Sistema(capacidadRefugio: 1);
        s.NuevaMascota("Uno");

        var ex = Assert.Throws<ReglaNegocioException>(() => s.NuevaMascota("Dos"));
        Assert.Contains("capacidad máxima", ex.Message);
    }

    [Fact]
    public void LasMascotasAdoptadas_NoOcupanPlaza()
    {
        var s = new Sistema(capacidadRefugio: 1);
        var m = s.NuevaMascota();
        m.Estado = EstadoMascota.Adoptada;

        Assert.True(s.Refugio.TieneLugarDisponible());
        Assert.NotNull(s.NuevaMascota("Otra"));
    }

    [Fact]
    public void AlIngresar_LaMascotaQuedaIngresada_ConHistorialSanitario()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();

        Assert.Equal(EstadoMascota.Ingresada, m.Estado);
        Assert.Single(s.Db.Historiales, h => h.MascotaId == m.Id);
    }

    [Fact]
    public void NoSePuedeHabilitarParaAdopcion_SinVacunasNiControles()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();

        var ex = Assert.Throws<ReglaNegocioException>(() => s.Mascotas.CambiarEstado(m.Id, EstadoMascota.DisponibleAdopcion));
        Assert.Contains("Vacunacion", ex.Message);
        Assert.Contains("ControlVeterinario", ex.Message);
    }

    [Fact]
    public void EstadosDeProceso_NoSeAsignanManualmente()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();

        Assert.Throws<ReglaNegocioException>(() => s.Mascotas.CambiarEstado(m.Id, EstadoMascota.Adoptada));
        Assert.Throws<ReglaNegocioException>(() => s.Mascotas.CambiarEstado(m.Id, EstadoMascota.Reservada));
    }

    [Fact]
    public void ConsultarMascotasSegunSuEstado()
    {
        var s = new Sistema();
        s.MascotaDisponible("A");
        s.NuevaMascota("B");

        Assert.Single(s.Mascotas.GetPorEstado(EstadoMascota.DisponibleAdopcion));
        Assert.Single(s.Mascotas.GetPorEstado(EstadoMascota.Ingresada));
    }
}
