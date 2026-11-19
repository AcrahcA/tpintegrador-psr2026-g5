namespace tpintegrador_psr2026.Api.Domain;

/// <summary>
/// Fotografía de una mascota. Una mascota puede tener varias; la de menor orden es la principal.
/// La descripción es el texto alternativo que leen los lectores de pantalla (WCAG 1.1.1).
/// </summary>
public class FotoMascota : IEntidad
{
    public const int MaximoPorMascota = 12;
    public const int LargoMaximoDescripcion = 250;

    public int Id { get; set; }
    public int MascotaId { get; set; }
    public Mascota? Mascota { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public DateTime FechaCarga { get; set; }

    protected FotoMascota() { }

    public FotoMascota(int mascotaId, string url, string descripcion, int orden)
    {
        MascotaId = mascotaId;
        Url = url;
        Descripcion = descripcion;
        Orden = orden;
        FechaCarga = DateTime.UtcNow;
    }

    public static string NormalizarDescripcion(string? descripcion, string nombreMascota)
    {
        var texto = string.IsNullOrWhiteSpace(descripcion) ? $"Foto de {nombreMascota}" : descripcion.Trim();
        if (texto.Length > LargoMaximoDescripcion)
            throw new ReglaNegocioException($"La descripción de la foto no puede superar los {LargoMaximoDescripcion} caracteres.");
        return texto;
    }
}
