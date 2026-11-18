namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public class AvisoRescateService : IAvisoRescateService
{
    private readonly IAvisoRescateRepository _avisos;
    private readonly IMascotaRepository _mascotas;
    private readonly IRefugioRepository _refugio;
    private readonly IOperacionAtomica? _atomica;
    public AvisoRescateService(IAvisoRescateRepository avisoRepository, IMascotaRepository mascotaRepository,
        IRefugioRepository refugioRepository, IOperacionAtomica? operacionAtomica = null)
    { _avisos = avisoRepository; _mascotas = mascotaRepository; _refugio = refugioRepository; _atomica = operacionAtomica; }

    public List<AvisoRescate> Get() => _avisos.Get();
    public List<AvisoRescate> GetPendientes() => _avisos.Get().Where(a =>
        a.Estado is EstadoAviso.Reportado or EstadoAviso.EnEvaluacion).ToList();
    public AvisoRescate? GetById(int id) => _avisos.GetPorId(id);

    private static bool DatosValidos(AvisoRescate a) =>
        !string.IsNullOrWhiteSpace(a.Descripcion) && !string.IsNullOrWhiteSpace(a.ContactoNombre) &&
        double.IsFinite(a.Latitud) && a.Latitud >= -90 && a.Latitud <= 90 &&
        double.IsFinite(a.Longitud) && a.Longitud >= -180 && a.Longitud <= 180 &&
        !(a.Latitud == 0 && a.Longitud == 0);

    public AvisoRescate Post(AvisoRescate aviso)
    {
        if (!DatosValidos(aviso)) throw new ArgumentException("Completá la descripción, el contacto y una ubicación válida.");
        aviso.FechaAviso = DateTime.UtcNow;
        aviso.Estado = EstadoAviso.Reportado;
        aviso.MascotaGeneradaId = null;
        return _avisos.Post(aviso);
    }
    public bool Actualizar(int id, AvisoRescate datos)
    {
        var actual = GetById(id);
        if (actual is null || actual.Estado is EstadoAviso.Atendido or EstadoAviso.Descartado || !DatosValidos(datos)) return false;
        actual.Descripcion = datos.Descripcion;
        actual.TipoAnimal = datos.TipoAnimal;
        actual.Latitud = datos.Latitud;
        actual.Longitud = datos.Longitud;
        actual.Direccion = datos.Direccion;
        actual.ContactoNombre = datos.ContactoNombre;
        actual.ContactoTelefono = datos.ContactoTelefono;
        actual.EstadoAparente = datos.EstadoAparente;
        actual.NivelUrgencia = datos.NivelUrgencia;
        return _avisos.Put(id, actual);
    }
    public bool PutEstado(int id, EstadoAviso estado)
    {
        var aviso = GetById(id);
        if (aviso is null) return false;
        if (aviso.Estado == estado) return true;
        // No se puede marcar Atendido sin generar la mascota ni reabrir casos cerrados.
        var valida = (aviso.Estado, estado) switch
        {
            (EstadoAviso.Reportado, EstadoAviso.EnEvaluacion or EstadoAviso.Aceptado or EstadoAviso.Descartado) => true,
            (EstadoAviso.EnEvaluacion, EstadoAviso.Aceptado or EstadoAviso.Descartado) => true,
            (EstadoAviso.Aceptado, EstadoAviso.Descartado) => true,
            _ => false
        };
        if (!valida) return false;
        aviso.Estado = estado;
        return _avisos.Put(id, aviso);
    }
    public Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datos)
    {
        using var transaccion = _atomica?.Iniciar();
        var aviso = GetById(avisoId);
        if (aviso is null || aviso.Estado != EstadoAviso.Aceptado || aviso.MascotaGeneradaId is not null) return null;
        var refugio = _refugio.Get().FirstOrDefault();
        if (refugio is not null && _mascotas.Get().Count(m => m.Estado != EstadoMascota.Adoptada) >= refugio.CapacidadMaxima)
            return null;
        if (string.IsNullOrWhiteSpace(datos.Nombre) || datos.EdadAproximada < 0) return null;
        datos.FechaIngreso = DateTime.UtcNow;
        datos.Estado = EstadoMascota.Ingresada;
        datos.AvisoRescateOrigenId = aviso.Id;
        datos.RefugioId = refugio?.Id;
        datos.CuidadorId = null;
        datos.FotoUrl = null;
        var creada = _mascotas.Post(datos);
        aviso.MascotaGeneradaId = creada.Id;
        aviso.Estado = EstadoAviso.Atendido;
        if (!_avisos.Put(aviso.Id, aviso))
            throw new OperacionNoPermitidaException("No se pudo vincular la mascota con el aviso. Se revirtió la operación.");
        transaccion?.Confirmar();
        return creada;
    }
    public bool Delete(int id)
    {
        var aviso = GetById(id);
        if (aviso is null || aviso.Estado == EstadoAviso.Atendido || aviso.MascotaGeneradaId is not null) return false;
        return _avisos.Delete(id);
    }
}
