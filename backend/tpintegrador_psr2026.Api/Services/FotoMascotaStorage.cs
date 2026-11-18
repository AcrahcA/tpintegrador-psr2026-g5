namespace tpintegrador_psr2026.Api.Services;

/// <summary>Guarda las fotografías de las mascotas en disco (carpeta configurable "Fotos:Directorio").</summary>
public interface IFotoMascotaStorage
{
    string Guardar(Stream contenido, string contentType);
    void Eliminar(string? fotoUrl);
    string? RutaFisica(string nombreArchivo);
}

public class FotoMascotaStorage : IFotoMascotaStorage
{
    public const string RutaPublica = "/api/Mascotas/imagenes/";
    public const long TamañoMaximo = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> Extensiones = new()
    {
        ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp"
    };

    private readonly string _directorio;

    public FotoMascotaStorage(IConfiguration configuration, IWebHostEnvironment env)
    {
        var configurado = configuration["Fotos:Directorio"];
        _directorio = string.IsNullOrWhiteSpace(configurado)
            ? Path.Combine(env.ContentRootPath, "uploads", "mascotas")
            : configurado;
        Directory.CreateDirectory(_directorio);
    }

    public static bool EsTipoPermitido(string? contentType) => contentType is not null && Extensiones.ContainsKey(contentType);

    public string Guardar(Stream contenido, string contentType)
    {
        var nombre = Guid.NewGuid().ToString("N") + Extensiones[contentType];
        using var destino = File.Create(Path.Combine(_directorio, nombre));
        contenido.CopyTo(destino);
        return RutaPublica + nombre;
    }

    public void Eliminar(string? fotoUrl)
    {
        if (string.IsNullOrEmpty(fotoUrl) || !fotoUrl.StartsWith(RutaPublica)) return;
        var ruta = RutaFisica(fotoUrl[RutaPublica.Length..]);
        if (ruta is not null && File.Exists(ruta)) File.Delete(ruta);
    }

    public string? RutaFisica(string nombreArchivo)
    {
        // Evita recorridos de directorio: sólo nombres generados por el sistema
        if (!System.Text.RegularExpressions.Regex.IsMatch(nombreArchivo, "^[0-9a-f]{32}\\.(jpg|png|webp)$")) return null;
        return Path.Combine(_directorio, nombreArchivo);
    }
}
