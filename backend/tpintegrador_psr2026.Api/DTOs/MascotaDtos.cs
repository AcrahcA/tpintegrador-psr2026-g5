using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class MascotaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Obligatoria al registrar. Al ingresar desde un aviso, si se omite se usa el tipo de animal del aviso.</summary>
    public TipoAnimal? Especie { get; set; }
    public string? Raza { get; set; }
    public int EdadAproximada { get; set; }
    public Sexo Sexo { get; set; }
    public Tamaño Tamaño { get; set; }

    public Mascota ADominio(TipoAnimal? especiePorDefecto = null)
    {
        var especie = Especie ?? especiePorDefecto
                      ?? throw new ReglaNegocioException("La especie de la mascota es obligatoria.");
        return new Mascota(Nombre?.Trim() ?? string.Empty, especie, EdadAproximada, Sexo, Tamaño,
            string.IsNullOrWhiteSpace(Raza) ? null : Raza.Trim());
    }
}

/// <summary>Foto de la galería. "Descripcion" es el texto alternativo de la imagen.</summary>
public record FotoMascotaDto(int Id, string Url, string Descripcion, int Orden)
{
    public static FotoMascotaDto Desde(FotoMascota f) => new(f.Id, f.Url, f.Descripcion, f.Orden);
}

public class FotoMascotaUpdateDto
{
    public string? Descripcion { get; set; }
    /// <summary>true = pasa a ser la foto principal (primera de la galería).</summary>
    public bool Principal { get; set; }
}

public record MascotaResponseDto(
    int Id,
    string Nombre,
    TipoAnimal Especie,
    string? Raza,
    int EdadAproximada,
    Sexo Sexo,
    Tamaño Tamaño,
    DateTime FechaIngreso,
    EstadoMascota Estado,
    int? CuidadorId,
    int? AvisoRescateOrigenId,
    string? FotoUrl,
    List<FotoMascotaDto> Fotos)
{
    public static MascotaResponseDto Desde(Mascota m, IEnumerable<FotoMascota>? fotos = null)
    {
        var lista = (fotos ?? Enumerable.Empty<FotoMascota>()).OrderBy(f => f.Orden).Select(FotoMascotaDto.Desde).ToList();
        return new(m.Id, m.Nombre, m.Especie, m.Raza, m.EdadAproximada, m.Sexo, m.Tamaño,
            m.FechaIngreso, m.Estado, m.CuidadorId, m.AvisoRescateOrigenId, lista.FirstOrDefault()?.Url, lista);
    }
}
