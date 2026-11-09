namespace tpintegrador_psr2026.Api.DTOs;

public class AdoptanteRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}