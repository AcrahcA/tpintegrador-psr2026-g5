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
    string? FotoUrl)
{
    public static MascotaResponseDto Desde(Mascota m) => new(
        m.Id, m.Nombre, m.Especie, m.Raza, m.EdadAproximada, m.Sexo, m.Tamaño,
        m.FechaIngreso, m.Estado, m.CuidadorId, m.AvisoRescateOrigenId, m.FotoUrl);
}
