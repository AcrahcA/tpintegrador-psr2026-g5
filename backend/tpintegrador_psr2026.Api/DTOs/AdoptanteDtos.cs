using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class AdoptanteRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }

    public Adoptante ADominio() => new(Nombre, Dni, Telefono, Direccion, Email);
}

public record AdoptanteResponseDto(int Id, string Nombre, string Dni, string? Telefono, string? Email, string? Direccion)
{
    public static AdoptanteResponseDto Desde(Adoptante a) => new(a.Id, a.Nombre, a.Dni, a.Telefono, a.Email, a.Direccion);
}
