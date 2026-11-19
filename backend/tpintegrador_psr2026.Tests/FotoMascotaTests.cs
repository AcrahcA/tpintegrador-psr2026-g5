using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Tests;

public class FotoMascotaTests
{
    private static ArchivoFoto Png(string? descripcion = null) => new(new MemoryStream(new byte[] { 1, 2, 3 }), "image/png", 3, descripcion);

    [Fact]
    public void SePuedenSubirVariasFotos_ConDescripcionPorDefecto()
    {
        var s = new Sistema();
        var m = s.NuevaMascota("Miel");

        var fotos = s.Fotos.Agregar(m.Id, new[] { Png("Miel en el parque"), Png() });

        Assert.Equal(2, fotos.Count);
        Assert.Equal("Miel en el parque", fotos[0].Descripcion);
        Assert.Equal("Foto de Miel", fotos[1].Descripcion);   // texto alternativo nunca vacío
        Assert.Equal(new[] { 0, 1 }, s.Fotos.Listar(m.Id).Select(f => f.Orden));
    }

    [Fact]
    public void NoSeSuperaElMaximoDeFotosPorMascota()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();
        for (var i = 0; i < 2; i++) s.Fotos.Agregar(m.Id, Enumerable.Range(0, 6).Select(_ => Png()).ToList());

        Assert.Throws<ReglaNegocioException>(() => s.Fotos.Agregar(m.Id, new[] { Png() }));
        Assert.Equal(FotoMascota.MaximoPorMascota, s.Fotos.Listar(m.Id).Count);
    }

    [Fact]
    public void ArchivoInvalido_NoSeGuardaNinguno()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();
        var invalido = new ArchivoFoto(new MemoryStream(new byte[] { 1 }), "application/pdf", 1, null);

        Assert.Throws<ReglaNegocioException>(() => s.Fotos.Agregar(m.Id, new[] { Png(), invalido }));
        Assert.Empty(s.Fotos.Listar(m.Id));
    }

    [Fact]
    public void HacerPrincipal_YReordenar()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();
        var fotos = s.Fotos.Agregar(m.Id, new[] { Png("a"), Png("b"), Png("c") });

        s.Fotos.Actualizar(m.Id, fotos[2].Id, null, principal: true);
        Assert.Equal(new[] { "c", "a", "b" }, s.Fotos.Listar(m.Id).Select(f => f.Descripcion));

        s.Fotos.Mover(m.Id, fotos[2].Id, +1);
        Assert.Equal(new[] { "a", "c", "b" }, s.Fotos.Listar(m.Id).Select(f => f.Descripcion));
    }

    [Fact]
    public void EliminarFoto_BorraElArchivo_YValidaQueSeaDeEsaMascota()
    {
        var s = new Sistema();
        var m1 = s.NuevaMascota("A");
        var m2 = s.NuevaMascota("B");
        var foto = s.Fotos.Agregar(m1.Id, new[] { Png() })[0];

        Assert.Throws<RecursoNoEncontradoException>(() => s.Fotos.Eliminar(m2.Id, foto.Id));
        s.Fotos.Eliminar(m1.Id, foto.Id);

        Assert.Empty(s.Fotos.Listar(m1.Id));
        Assert.Contains(foto.Url, s.Storage.Eliminadas);
    }
}
