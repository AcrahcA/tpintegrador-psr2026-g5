using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.DTOs;

public class AvisoRescateRequestDto
{
    public string Descripcion { get; set; } = string.Empty;
    public TipoAnimal TipoAnimal { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string ContactoNombre { get; set; } = string.Empty;
    public string? ContactoTelefono { get; set; }
    public EstadoAparente EstadoAparente { get; set; }
    public NivelUrgencia NivelUrgencia { get; set; }
}