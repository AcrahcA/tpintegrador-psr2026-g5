using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Data;

namespace tpintegrador_psr2026.Api.Services;

// Archivos fuera de frontend/ y backend/ para que no se pierdan al reemplazar esas carpetas.
// Guardar también patitas_uploads/ en los respaldos; no incluirlo en Git.
public sealed class MascotaFotoService : IMascotaFotoService
{
    public const long MaximoBytes = 5 * 1024 * 1024;
    private readonly RefugioDbContext _db;
    private readonly string _carpeta;
    private readonly ILogger<MascotaFotoService> _logger;

    public MascotaFotoService(RefugioDbContext db, IWebHostEnvironment environment, ILogger<MascotaFotoService> logger)
    {
        _db = db;
        _logger = logger;
        _carpeta = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "patitas_uploads", "mascotas"));
    }

    private static bool ExtensionSegura(string nombre) =>
        System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^[a-f0-9]{32}\.(jpg|png|webp)$",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private static (string Extension, string Mime)? DetectarFormato(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff)
            return (".jpg", "image/jpeg");
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[]{137, 80, 78, 71, 13, 10, 26, 10}))
            return (".png", "image/png");
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8))
            return (".webp", "image/webp");
        return null;
    }

    public async Task<string?> GuardarAsync(int mascotaId, IFormFile archivo, CancellationToken cancellationToken)
    {
        var mascota = await _db.Mascotas.FindAsync(new object[]{mascotaId}, cancellationToken);
        if (mascota is null) return null;
        if (archivo.Length < 12 || archivo.Length > MaximoBytes)
            throw new ArgumentException("La foto debe pesar entre 1 byte y 5 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp")
            throw new ArgumentException("Solo se permiten fotos JPG, PNG o WebP.");

        await using var origen = archivo.OpenReadStream();
        var encabezado = new byte[12];
        await origen.ReadExactlyAsync(encabezado, cancellationToken);
        var formato = DetectarFormato(encabezado);
        if (formato is null || (extension switch
            { ".jpeg" or ".jpg" => ".jpg", _ => extension }) != formato.Value.Extension)
            throw new ArgumentException("El contenido no corresponde a una imagen JPG, PNG o WebP válida.");

        Directory.CreateDirectory(_carpeta);
        var nombre = $"{Guid.NewGuid():N}{formato.Value.Extension}";
        var ruta = Path.Combine(_carpeta, nombre);
        string? anterior = null;
        try
        {
            await using (var destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await destino.WriteAsync(encabezado, cancellationToken);
                await origen.CopyToAsync(destino, cancellationToken);
            }
            anterior = mascota.FotoUrl;
            mascota.FotoUrl = $"/api/Mascotas/imagenes/{nombre}";
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(ruta)) File.Delete(ruta);
            throw;
        }
        // La foto nueva ya está confirmada en PostgreSQL: una falla limpiando
        // el archivo anterior no debe borrar la nueva foto ni deshacer el registro.
        await LimpiarSinInterrumpirAsync(anterior);
        return mascota.FotoUrl;
    }

    private async Task LimpiarSinInterrumpirAsync(string? ruta)
    {
        try { await EliminarSiExisteAsync(ruta); }
        catch (IOException ex) { _logger.LogWarning(ex, "No se pudo limpiar una foto anterior de mascota"); }
        catch (UnauthorizedAccessException ex) { _logger.LogWarning(ex, "Sin permisos para limpiar una foto anterior"); }
    }

    public async Task<bool> QuitarAsync(int mascotaId, CancellationToken cancellationToken)
    {
        var mascota = await _db.Mascotas.FindAsync(new object[]{mascotaId}, cancellationToken);
        if (mascota is null) return false;
        var vieja = mascota.FotoUrl;
        mascota.FotoUrl = null;
        await _db.SaveChangesAsync(cancellationToken);
        await LimpiarSinInterrumpirAsync(vieja);
        return true;
    }

    public Task<(string Ruta, string Tipo)?> BuscarArchivoAsync(string nombreArchivo)
    {
        if (!ExtensionSegura(nombreArchivo)) return Task.FromResult<(string, string)?>(null);
        var ruta = Path.Combine(_carpeta, nombreArchivo);
        if (!File.Exists(ruta)) return Task.FromResult<(string, string)?>(null);
        var tipo = Path.GetExtension(ruta) switch
        { ".jpg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", _ => "application/octet-stream" };
        return Task.FromResult<(string, string)?>( (ruta, tipo) );
    }

    public Task EliminarSiExisteAsync(string? fotoUrl)
    {
        var nombre = Path.GetFileName(fotoUrl);
        if (nombre is not null && ExtensionSegura(nombre))
        {
            var ruta = Path.Combine(_carpeta, nombre);
            if (File.Exists(ruta)) File.Delete(ruta);
        }
        return Task.CompletedTask;
    }
}
