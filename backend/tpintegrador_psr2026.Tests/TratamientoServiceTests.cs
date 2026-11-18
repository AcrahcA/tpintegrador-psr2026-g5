using Xunit;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Tests;

public class TratamientoServiceTests
{
    [Fact]
    public void TratamientoActivo_PoneLaMascotaEnTratamiento_YAlFinalizarPasaARecuperacion()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();

        var t = s.Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.Medicacion, "Antibiótico"));
        Assert.Equal(EstadoMascota.EnTratamiento, m.Estado);

        s.Tratamientos.CambiarEstado(t.Id, EstadoTratamiento.EnCurso);
        s.Tratamientos.CambiarEstado(t.Id, EstadoTratamiento.Finalizado);

        Assert.Equal(EstadoMascota.EnRecuperacion, m.Estado);
        Assert.NotNull(t.FechaFin);
    }

    [Fact]
    public void MascotaDisponible_ConNuevoTratamientoActivo_DejaDeEstarDisponible()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();

        s.Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.Curacion));

        Assert.Equal(EstadoMascota.EnTratamiento, m.Estado);
        Assert.False(s.Mascotas.EvaluarAdopcion(m.Id).DisponibleParaAdopcion);
    }

    [Fact]
    public void TratamientoFinalizado_NoPuedeReabrirse()
    {
        var s = new Sistema();
        var m = s.NuevaMascota();
        var t = s.Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.Vacunacion));
        s.Tratamientos.CambiarEstado(t.Id, EstadoTratamiento.Finalizado);

        Assert.Throws<ReglaNegocioException>(() => s.Tratamientos.CambiarEstado(t.Id, EstadoTratamiento.EnCurso));
    }

    [Fact]
    public void ConsultarTratamientosDeUnaMascota()
    {
        var s = new Sistema();
        var a = s.NuevaMascota("A");
        var b = s.NuevaMascota("B");
        s.Tratamientos.Registrar(a.Id, new Tratamiento(TipoTratamiento.Vacunacion));
        s.Tratamientos.Registrar(a.Id, new Tratamiento(TipoTratamiento.Desparasitacion));
        s.Tratamientos.Registrar(b.Id, new Tratamiento(TipoTratamiento.Medicacion));

        Assert.Equal(2, s.Tratamientos.GetByMascotaId(a.Id).Count);
    }
}
