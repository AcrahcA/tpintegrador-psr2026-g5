using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class MascotaRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public int EdadAproximada { get; set; }
    public Sexo Sexo { get; set; }
    public Tamaño Tamaño { get; set; }
    public EstadoMascota Estado { get; set; } = EstadoMascota.Ingresada;
}