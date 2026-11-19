using Xunit;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Tests;

public class AdopcionTests
{
    [Fact]
    public void SoloSePuedenSolicitarMascotasDisponibles()
    {
        var s = new Sistema();
        var adoptante = s.NuevoAdoptante();
        var noDisponible = s.NuevaMascota();

        Assert.Throws<ReglaNegocioException>(() => s.Solicitudes.Crear(adoptante.Id, noDisponible.Id));
    }

    [Fact]
    public void VariasSolicitudes_SoloUnaSeAprueba_YLaMascotaQuedaReservada()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var s1 = s.Solicitudes.Crear(s.NuevoAdoptante("30111222").Id, m.Id);
        var s2 = s.Solicitudes.Crear(s.NuevoAdoptante("30111333").Id, m.Id);

        s.Solicitudes.Aprobar(s1.Id);

        Assert.Equal(EstadoMascota.Reservada, m.Estado);
        Assert.Equal(EstadoSolicitud.Rechazada, s2.Estado);
        Assert.Throws<ReglaNegocioException>(() => s.Solicitudes.Aprobar(s2.Id));
    }

    [Fact]
    public void LaAdopcionRequiereSolicitudAprobada()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var sol = s.Solicitudes.Crear(s.NuevoAdoptante().Id, m.Id);

        var ex = Assert.Throws<ReglaNegocioException>(() => s.Adopciones.Registrar(sol.Id, null));
        Assert.Contains("aprobada", ex.Message);
    }

    [Fact]
    public void AlCompletarLaAdopcion_LaMascotaQuedaAdoptada_YSeConservaSuHistorial()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var adoptante = s.NuevoAdoptante();
        var sol = s.Solicitudes.Crear(adoptante.Id, m.Id);
        s.Solicitudes.Aprobar(sol.Id);

        var adopcion = s.Adopciones.Registrar(sol.Id, "Todo ok");

        Assert.Equal(EstadoMascota.Adoptada, m.Estado);
        Assert.Equal(adoptante.Id, adopcion.AdoptanteId);
        Assert.Equal(m.Id, adopcion.MascotaId);
        Assert.Contains(s.Mascotas.Get(), x => x.Id == m.Id);                // sigue registrada
        Assert.Equal(2, s.Tratamientos.GetByMascotaId(m.Id).Count);          // conserva su historial
        Assert.Throws<ReglaNegocioException>(() => s.Solicitudes.Crear(s.NuevoAdoptante("40111222").Id, m.Id));
    }

    [Fact]
    public void NoPuedenExistirDosAdopcionesDeLaMismaMascota()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var sol = s.Solicitudes.Crear(s.NuevoAdoptante().Id, m.Id);
        s.Solicitudes.Aprobar(sol.Id);
        s.Adopciones.Registrar(sol.Id, null);

        Assert.Throws<ReglaNegocioException>(() => s.Adopciones.Registrar(sol.Id, null));
        Assert.Single(s.Db.Adopciones);
    }

    [Fact]
    public void NoSeAdopta_SiAparecioUnTratamientoActivoDuranteLaReserva()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var sol = s.Solicitudes.Crear(s.NuevoAdoptante().Id, m.Id);
        s.Solicitudes.Aprobar(sol.Id);
        s.Tratamientos.Registrar(m.Id, new Tratamiento(TipoTratamiento.Medicacion));

        Assert.Throws<ReglaNegocioException>(() => s.Adopciones.Registrar(sol.Id, null));
        Assert.NotEqual(EstadoMascota.Adoptada, m.Estado);
    }

    [Fact]
    public void CancelarUnaSolicitudAprobada_LiberaLaReserva()
    {
        var s = new Sistema();
        var m = s.MascotaDisponible();
        var sol = s.Solicitudes.Crear(s.NuevoAdoptante().Id, m.Id);
        s.Solicitudes.Aprobar(sol.Id);

        s.Solicitudes.CambiarEstado(sol.Id, EstadoSolicitud.Cancelada);

        Assert.Equal(EstadoMascota.DisponibleAdopcion, m.Estado);
    }

    [Fact]
    public void AdoptanteConDniDuplicado_EsRechazado()
    {
        var s = new Sistema();
        s.NuevoAdoptante("32111222");
        Assert.Throws<ReglaNegocioException>(() => s.NuevoAdoptante("32.111.222"));
    }
}
