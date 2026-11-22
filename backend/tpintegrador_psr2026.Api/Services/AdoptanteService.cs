namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

public interface IAdoptanteService
{
    List<Adoptante> Get();
    Adoptante? GetById(int id);
    Adoptante ObtenerPorId(int id);
    Adoptante Registrar(Adoptante adoptante);
    /// <summary>Devuelve el adoptante con ese DNI (actualizando sus datos de contacto) o lo registra.</summary>
    Adoptante ObtenerORegistrar(Adoptante datos);
    /// <summary>Datos de adoptante de una cuenta (los crea o vincula por DNI si hace falta).</summary>
    Adoptante ObtenerORegistrarParaUsuario(int usuarioId, Adoptante datos);
    Adoptante? GetPorUsuario(int usuarioId);
    Adoptante Actualizar(int id, Adoptante datos);
    void Eliminar(int id);
}

public class AdoptanteService : IAdoptanteService
{
    private readonly IAdoptanteRepository _adoptanteRepository;
    private readonly ISolicitudAdopcionRepository _solicitudRepository;

    public AdoptanteService(IAdoptanteRepository adoptanteRepository, ISolicitudAdopcionRepository solicitudRepository)
    {
        _adoptanteRepository = adoptanteRepository;
        _solicitudRepository = solicitudRepository;
    }

    public List<Adoptante> Get() => _adoptanteRepository.Get();

    public Adoptante? GetById(int id) => _adoptanteRepository.GetPorId(id);

    public Adoptante ObtenerPorId(int id) =>
        GetById(id) ?? throw new RecursoNoEncontradoException($"No existe el adoptante #{id}.");

    private static void Normalizar(Adoptante a)
    {
        a.Nombre = a.Nombre?.Trim() ?? string.Empty;
        a.Telefono = string.IsNullOrWhiteSpace(a.Telefono) ? null : a.Telefono.Trim();
        a.Email = string.IsNullOrWhiteSpace(a.Email) ? null : a.Email.Trim();
        a.Direccion = string.IsNullOrWhiteSpace(a.Direccion) ? null : a.Direccion.Trim();
        a.Validar();
    }

    public Adoptante Registrar(Adoptante adoptante)
    {
        Normalizar(adoptante);
        if (_adoptanteRepository.GetPorDni(adoptante.Dni) is not null)
            throw new ReglaNegocioException($"Ya existe un adoptante registrado con el DNI {adoptante.Dni}.");
        return _adoptanteRepository.Post(adoptante);
    }

    public Adoptante ObtenerORegistrar(Adoptante datos)
    {
        Normalizar(datos);
        var existente = _adoptanteRepository.GetPorDni(datos.Dni);
        if (existente is null) return _adoptanteRepository.Post(datos);

        existente.Nombre = datos.Nombre;
        existente.Telefono = datos.Telefono ?? existente.Telefono;
        existente.Email = datos.Email ?? existente.Email;
        existente.Direccion = datos.Direccion ?? existente.Direccion;
        _adoptanteRepository.Put(existente.Id, existente);
        return existente;
    }

    public Adoptante? GetPorUsuario(int usuarioId) => _adoptanteRepository.GetPorUsuario(usuarioId);

    public Adoptante ObtenerORegistrarParaUsuario(int usuarioId, Adoptante datos)
    {
        Normalizar(datos);
        var porDni = _adoptanteRepository.GetPorDni(datos.Dni);
        var propio = _adoptanteRepository.GetPorUsuario(usuarioId);

        // Un DNI pertenece a una sola cuenta
        if (porDni is not null && porDni.UsuarioId is not null && porDni.UsuarioId != usuarioId)
            throw new ReglaNegocioException("Ese DNI ya está asociado a otra cuenta. Si es tuyo, comunicate con el refugio.");
        if (propio is not null && porDni is not null && porDni.Id != propio.Id)
            throw new ReglaNegocioException("Ese DNI ya está registrado para otra persona. Revisá el número ingresado.");

        // Si el refugio ya la había registrado por DNI (sin cuenta), se vincula a esta cuenta
        var adoptante = propio ?? porDni;
        if (adoptante is null)
        {
            datos.UsuarioId = usuarioId;
            return _adoptanteRepository.Post(datos);
        }

        adoptante.UsuarioId = usuarioId;
        adoptante.Nombre = datos.Nombre;
        adoptante.Dni = datos.Dni;
        adoptante.Telefono = datos.Telefono ?? adoptante.Telefono;
        adoptante.Email = datos.Email ?? adoptante.Email;
        adoptante.Direccion = datos.Direccion ?? adoptante.Direccion;
        _adoptanteRepository.Put(adoptante.Id, adoptante);
        return adoptante;
    }

    public Adoptante Actualizar(int id, Adoptante datos)
    {
        var adoptante = ObtenerPorId(id);
        Normalizar(datos);
        var otro = _adoptanteRepository.GetPorDni(datos.Dni);
        if (otro is not null && otro.Id != id)
            throw new ReglaNegocioException($"Ya existe otro adoptante con el DNI {datos.Dni}.");

        adoptante.Nombre = datos.Nombre;
        adoptante.Dni = datos.Dni;
        adoptante.Telefono = datos.Telefono;
        adoptante.Email = datos.Email;
        adoptante.Direccion = datos.Direccion;
        _adoptanteRepository.Put(id, adoptante);
        return adoptante;
    }

    public void Eliminar(int id)
    {
        ObtenerPorId(id);
        if (_solicitudRepository.GetPorAdoptante(id).Count > 0)
            throw new ReglaNegocioException("El adoptante tiene solicitudes de adopción registradas y se conserva en el historial.");
        _adoptanteRepository.Delete(id);
    }
}
