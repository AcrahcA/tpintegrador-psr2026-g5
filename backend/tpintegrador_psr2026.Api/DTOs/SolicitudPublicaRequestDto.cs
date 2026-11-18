namespace tpintegrador_psr2026.Api.DTOs;

public class SolicitudPublicaRequestDto
{
    public int MascotaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Direccion { get; set; }
}
