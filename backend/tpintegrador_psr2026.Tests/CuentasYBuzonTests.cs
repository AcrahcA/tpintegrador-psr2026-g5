using Xunit;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services.Seguridad;

namespace tpintegrador_psr2026.Tests;

public class CuentasYBuzonTests
{
    private static Adoptante Datos(string dni = "40111222") => new("Sofía Díaz", dni, "11 5555");

    [Fact]
    public void LaContraseña_SeGuardaComoHash_YSeVerifica()
    {
        var hash = Contraseñas.Hashear("Patitas2026");

        Assert.DoesNotContain("Patitas2026", hash);
        Assert.True(Contraseñas.Verificar("Patitas2026", hash));
        Assert.False(Contraseñas.Verificar("patitas2026", hash));
        Assert.NotEqual(hash, Contraseñas.Hashear("Patitas2026")); // sal aleatoria
    }

    [Fact]
    public void Registro_CreaCuentasComunes_ConCorreoUnicoYContraseñaSegura()
    {
        var s = new Sistema();

        Assert.Throws<ReglaNegocioException>(() => s.Usuarios.Registrar("Sofía", "sofia@example.com", "corta", null));
        var u = s.Usuarios.Registrar("Sofía", "Sofia@Example.com", "Segura2026", null);

        Assert.Equal(RolUsuario.Usuario, u.Rol);               // nunca se registra un administrador
        Assert.Equal("sofia@example.com", u.Email);
        Assert.Throws<ReglaNegocioException>(() => s.Usuarios.Registrar("Otra", "SOFIA@example.com", "Segura2026", null));
    }

    [Fact]
    public void Login_ConContraseñaIncorrecta_DaElMismoMensajeQueUnCorreoInexistente()
    {
        var s = new Sistema();
        s.Usuarios.Registrar("Sofía", "sofia@example.com", "Segura2026", null);

        var malaClave = Assert.Throws<ReglaNegocioException>(() => s.Usuarios.Autenticar("sofia@example.com", "Otra2026x"));
        var noExiste = Assert.Throws<ReglaNegocioException>(() => s.Usuarios.Autenticar("nadie@example.com", "Segura2026"));

        Assert.Equal(malaClave.Message, noExiste.Message);
        Assert.Equal("Sofía", s.Usuarios.Autenticar(" SOFIA@example.com ", "Segura2026").Nombre);
    }

    [Fact]
    public void RechazarSolicitud_DejaUnMensajeEnElBuzon_ConElMotivoOpcional()
    {
        var s = new Sistema();
        var u = s.Usuarios.Registrar("Sofía", "sofia@example.com", "Segura2026", null);
        var m = s.MascotaDisponible("Luna");
        var sol = s.Solicitudes.CrearPublica(u.Id, Datos(), m.Id);

        s.Solicitudes.Rechazar(sol.Id, "  Buscamos una familia con patio.  ");

        var buzon = s.Notificaciones.Buzon(u.Id);
        var aviso = Assert.Single(buzon);
        Assert.Equal(TipoNotificacion.SolicitudRechazada, aviso.Tipo);
        Assert.Contains("Luna", aviso.Titulo);
        Assert.Equal("Buscamos una familia con patio.", aviso.Motivo);
        Assert.False(aviso.Leida);
    }

    [Fact]
    public void RechazarSinMotivo_TambienNotifica()
    {
        var s = new Sistema();
        var u = s.Usuarios.Registrar("Sofía", "sofia@example.com", "Segura2026", null);
        var sol = s.Solicitudes.CrearPublica(u.Id, Datos(), s.MascotaDisponible().Id);

        s.Solicitudes.Rechazar(sol.Id);

        Assert.Null(Assert.Single(s.Notificaciones.Buzon(u.Id)).Motivo);
    }

    [Fact]
    public void AlAprobarOtraSolicitud_LasDemasPersonasRecibenElRechazoAutomatico()
    {
        var s = new Sistema();
        var a = s.Usuarios.Registrar("Ana", "a@example.com", "Segura2026", null);
        var b = s.Usuarios.Registrar("Beto", "b@example.com", "Segura2026", null);
        var m = s.MascotaDisponible("Miel");
        var solA = s.Solicitudes.CrearPublica(a.Id, Datos("30000001"), m.Id);
        s.Solicitudes.CrearPublica(b.Id, Datos("30000002"), m.Id);

        s.Solicitudes.Aprobar(solA.Id);

        Assert.Equal(TipoNotificacion.SolicitudAprobada, Assert.Single(s.Notificaciones.Buzon(a.Id)).Tipo);
        var rechazo = Assert.Single(s.Notificaciones.Buzon(b.Id));
        Assert.Equal(TipoNotificacion.SolicitudRechazada, rechazo.Tipo);
        Assert.Contains("otra familia", rechazo.Motivo);
    }

    [Fact]
    public void DescartarUnAviso_NotificaSoloSiSeReportoConCuenta()
    {
        var s = new Sistema();
        var u = s.Usuarios.Registrar("Sofía", "sofia@example.com", "Segura2026", null);
        AvisoRescate Nuevo() => new("Perro atado", TipoAnimal.Perro, -34.6, -58.4, "Sofía", EstadoAparente.Sano, NivelUrgencia.Baja);
        var conCuenta = s.Avisos.Registrar(Nuevo(), u.Id);
        var anonimo = s.Avisos.Registrar(Nuevo());

        s.Avisos.Descartar(conCuenta.Id, "Ya lo rescató otra organización.");
        s.Avisos.Descartar(anonimo.Id);

        var n = Assert.Single(s.Db.Notificaciones);
        Assert.Equal(u.Id, n.UsuarioId);
        Assert.Equal(TipoNotificacion.AvisoDescartado, n.Tipo);
        Assert.Equal("Ya lo rescató otra organización.", n.Motivo);
    }

    [Fact]
    public void CadaPersonaSoloPuedeLeerSusPropiosMensajes()
    {
        var s = new Sistema();
        var a = s.Usuarios.Registrar("Ana", "a@example.com", "Segura2026", null);
        var b = s.Usuarios.Registrar("Beto", "b@example.com", "Segura2026", null);
        s.Notificaciones.Notificar(a.Id, TipoNotificacion.AvisoAceptado, "Hola", "Mensaje");
        var id = s.Db.Notificaciones.Single().Id;

        Assert.Throws<RecursoNoEncontradoException>(() => s.Notificaciones.MarcarLeida(b.Id, id));
        Assert.True(s.Notificaciones.MarcarLeida(a.Id, id).Leida);
        Assert.Equal(0, s.Notificaciones.ContarNoLeidas(a.Id));
    }

    [Fact]
    public void UnDniNoPuedeUsarseDesdeDosCuentas()
    {
        var s = new Sistema();
        var a = s.Usuarios.Registrar("Ana", "a@example.com", "Segura2026", null);
        var b = s.Usuarios.Registrar("Beto", "b@example.com", "Segura2026", null);
        var m = s.MascotaDisponible();
        s.Solicitudes.CrearPublica(a.Id, Datos("30111222"), m.Id);

        Assert.Throws<ReglaNegocioException>(() => s.Solicitudes.CrearPublica(b.Id, Datos("30111222"), m.Id));
    }

    [Fact]
    public void ElMotivoTieneUnLargoMaximo()
    {
        var s = new Sistema();
        var u = s.Usuarios.Registrar("Sofía", "sofia@example.com", "Segura2026", null);
        var sol = s.Solicitudes.CrearPublica(u.Id, Datos(), s.MascotaDisponible().Id);

        Assert.Throws<ReglaNegocioException>(() => s.Solicitudes.Rechazar(sol.Id, new string('x', 501)));
        Assert.Equal(EstadoSolicitud.Pendiente, sol.Estado);
    }
}
