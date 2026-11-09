namespace tpintegrador_psr2026.Api.DTOs;

public class CuidadorRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Especializacion { get; set; }
    public int CapacidadMaxima { get; set; }
}