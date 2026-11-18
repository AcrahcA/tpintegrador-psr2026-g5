using Xunit;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Tests;

public class CuidadorServiceTests
{
    [Fact]
    public void Cuidador_RespetaSuCapacidadMaxima()
    {
        var s = new Sistema();
        var c = s.Cuidadores.Registrar(new Cuidador("Carlos", 1, "Perros"));
        var m1 = s.NuevaMascota("Rocco");
        var m2 = s.NuevaMascota("Toby");

        s.Cuidadores.AsignarMascota(c.Id, m1.Id);
        var ex = Assert.Throws<ReglaNegocioException>(() => s.Cuidadores.AsignarMascota(c.Id, m2.Id));
        Assert.Contains("capacidad máxima", ex.Message);
    }

    [Fact]
    public void Cuidador_SoloAtiendeLosTiposDeSuEspecializacion()
    {
        var s = new Sistema();
        var c = s.Cuidadores.Registrar(new Cuidador("Marta", 3, "Gatos"));
        var perro = s.NuevaMascota("Rocco", TipoAnimal.Perro);
        var gato = s.NuevaMascota("Michi", TipoAnimal.Gato);

        Assert.Throws<ReglaNegocioException>(() => s.Cuidadores.AsignarMascota(c.Id, perro.Id));
        Assert.Equal(c.Id, s.Cuidadores.AsignarMascota(c.Id, gato.Id).CuidadorId);
    }

    [Fact]
    public void CuidadorNoDisponible_NoRecibeMascotas()
    {
        var s = new Sistema();
        var c = s.Cuidadores.Registrar(new Cuidador("Juan", 3, null, disponible: false));
        var m = s.NuevaMascota();

        Assert.Throws<ReglaNegocioException>(() => s.Cuidadores.AsignarMascota(c.Id, m.Id));
    }

    [Theory]
    [InlineData("Perros", TipoAnimal.Perro, true)]
    [InlineData("Perros", TipoAnimal.Gato, false)]
    [InlineData("Gatos y aves", TipoAnimal.Ave, true)]
    [InlineData("General", TipoAnimal.Otro, true)]
    [InlineData(null, TipoAnimal.Gato, true)]
    [InlineData("Felinos", TipoAnimal.Gato, true)]
    public void PuedeAtender_InterpretaLaEspecializacion(string? especializacion, TipoAnimal tipo, bool esperado) =>
        Assert.Equal(esperado, new Cuidador("X", 1, especializacion).PuedeAtender(tipo));

    [Fact]
    public void ConsultarMascotasAsignadasAUnCuidador_ExcluyeAdoptadas()
    {
        var s = new Sistema();
        var c = s.Cuidadores.Registrar(new Cuidador("Ana", 5));
        var m1 = s.NuevaMascota("A");
        var m2 = s.NuevaMascota("B");
        s.Cuidadores.AsignarMascota(c.Id, m1.Id);
        s.Cuidadores.AsignarMascota(c.Id, m2.Id);
        m2.Estado = EstadoMascota.Adoptada;

        Assert.Single(s.Cuidadores.MascotasAsignadas(c.Id));
        Assert.Equal(2, s.Cuidadores.MascotasAsignadas(c.Id, incluirAdoptadas: true).Count);
    }
}
