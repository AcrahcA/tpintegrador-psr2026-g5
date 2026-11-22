namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services.Seguridad;

public interface IUsuarioService
{
    /// <summary>Crea una cuenta común (rol Usuario). Los administradores se asignan desde la base de datos.</summary>
    Usuario Registrar(string nombre, string email, string contraseña, string? telefono);
    /// <summary>Devuelve el usuario si el correo y la contraseña son correctos.</summary>
    Usuario Autenticar(string email, string contraseña);
    Usuario? GetById(int id);
}

public class UsuarioService : IUsuarioService
{
    private const string CredencialesInvalidas = "El correo o la contraseña no son correctos.";
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository) => _usuarioRepository = usuarioRepository;

    public Usuario Registrar(string nombre, string email, string contraseña, string? telefono)
    {
        Usuario.ValidarDatos(nombre, email);
        Usuario.ValidarContraseña(contraseña);
        if (_usuarioRepository.GetPorEmail(email) is not null)
            throw new ReglaNegocioException("Ya existe una cuenta con ese correo electrónico. Iniciá sesión.");

        return _usuarioRepository.Post(new Usuario(nombre, email, Contraseñas.Hashear(contraseña), telefono));
    }

    public Usuario Autenticar(string email, string contraseña)
    {
        var usuario = _usuarioRepository.GetPorEmail(email ?? string.Empty);
        // Mismo mensaje si no existe la cuenta o si la contraseña es incorrecta (no revela qué correos están registrados)
        if (usuario is null || string.IsNullOrEmpty(contraseña) || !Contraseñas.Verificar(contraseña, usuario.PasswordHash))
            throw new ReglaNegocioException(CredencialesInvalidas);
        return usuario;
    }

    public Usuario? GetById(int id) => _usuarioRepository.GetPorId(id);
}
