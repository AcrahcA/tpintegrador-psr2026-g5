namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

/// <summary>Guarda las fotografías de las mascotas en disco (carpeta configurable "Fotos:Directorio").</summary>
public interface IFotoMascotaStorage
{
    /// <summary>Verifica tipo, tamaño y contenido real del archivo (no sólo la extensión).</summary>
    void Validar(Stream contenido, string contentType, long tamaño);
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

    public void Validar(Stream contenido, string contentType, long tamaño)
    {
        if (!Extensiones.ContainsKey(contentType ?? string.Empty) || tamaño <= 0 || tamaño > TamañoMaximo)
            throw new ReglaNegocioException("Cada foto debe ser JPG, PNG o WebP y pesar hasta 5 MB.");

        var cabecera = new byte[12];
        var leidos = contenido.Read(cabecera, 0, cabecera.Length);
        if (contenido.CanSeek) contenido.Position = 0;

        var valido = contentType switch
        {
            "image/jpeg" => leidos >= 3 && cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF,
            "image/png" => leidos >= 8 && cabecera.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => leidos >= 12 && cabecera[0] == 'R' && cabecera[1] == 'I' && cabecera[2] == 'F' && cabecera[3] == 'F'
                            && cabecera[8] == 'W' && cabecera[9] == 'E' && cabecera[10] == 'B' && cabecera[11] == 'P',
            _ => false
        };
        if (!valido)
            throw new ReglaNegocioException("El archivo no es una imagen válida.");
    }

    public string Guardar(Stream contenido, string contentType)
    {
        var nombre = Guid.NewGuid().ToString("N") + Extensiones[contentType];
        if (contenido.CanSeek) contenido.Position = 0;
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
