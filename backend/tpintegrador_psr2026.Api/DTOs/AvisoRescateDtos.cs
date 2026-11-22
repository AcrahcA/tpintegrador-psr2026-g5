using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class AvisoRescateRequestDto
{
    public string Descripcion { get; set; } = string.Empty;
    public TipoAnimal TipoAnimal { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string? Direccion { get; set; }
    public string ContactoNombre { get; set; } = string.Empty;
    public string? ContactoTelefono { get; set; }
    public EstadoAparente EstadoAparente { get; set; }
    public NivelUrgencia NivelUrgencia { get; set; }

    public AvisoRescate ADominio() => new(
        Descripcion?.Trim() ?? string.Empty, TipoAnimal, Latitud, Longitud,
        ContactoNombre?.Trim() ?? string.Empty, EstadoAparente, NivelUrgencia,
        string.IsNullOrWhiteSpace(ContactoTelefono) ? null : ContactoTelefono.Trim(),
        string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim());
}

public record AvisoRescateResponseDto(
    int Id,
    string Descripcion,
    TipoAnimal TipoAnimal,
    double Latitud,
    double Longitud,
    string? Direccion,
    DateTime FechaAviso,
    string ContactoNombre,
    string? ContactoTelefono,
    EstadoAparente EstadoAparente,
    NivelUrgencia NivelUrgencia,
    EstadoAviso Estado,
    int? MascotaGeneradaId,
    bool TieneCuenta)
{
    public static AvisoRescateResponseDto Desde(AvisoRescate a) => new(
        a.Id, a.Descripcion, a.TipoAnimal, a.Latitud, a.Longitud, a.Direccion, a.FechaAviso,
        a.ContactoNombre, a.ContactoTelefono, a.EstadoAparente, a.NivelUrgencia, a.Estado, a.MascotaGeneradaId,
        a.UsuarioId is not null);
}
