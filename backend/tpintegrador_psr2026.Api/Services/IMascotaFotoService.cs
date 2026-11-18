using Microsoft.AspNetCore.Http;

namespace tpintegrador_psr2026.Api.Services;

public interface IMascotaFotoService
{
    Task<string?> GuardarAsync(int mascotaId, IFormFile archivo, CancellationToken cancellationToken);
    Task<bool> QuitarAsync(int mascotaId, CancellationToken cancellationToken);
    Task<(string Ruta, string Tipo)?> BuscarArchivoAsync(string nombreArchivo);
    Task EliminarSiExisteAsync(string? fotoUrl);
}
