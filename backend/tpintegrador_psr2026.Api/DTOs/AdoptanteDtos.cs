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

/// <summary>TieneCuenta: si la persona usa una cuenta del sitio (recibe los avisos en su buzón).</summary>
public record AdoptanteResponseDto(int Id, string Nombre, string Dni, string? Telefono, string? Email, string? Direccion, bool TieneCuenta)
{
    public static AdoptanteResponseDto Desde(Adoptante a) => new(a.Id, a.Nombre, a.Dni, a.Telefono, a.Email, a.Direccion, a.UsuarioId is not null);
}
