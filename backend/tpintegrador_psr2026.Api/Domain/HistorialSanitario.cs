using System.Text.Json.Serialization;

namespace tpintegrador_psr2026.Api.Domain;

public class HistorialSanitario
{
    [JsonIgnore] // El ID lo autogenera el sistema al crear el historial
    public int Id { get; set; }
    
    // Composición: Pertenece a una única Mascota
    public int MascotaId { get; set; }

    [JsonIgnore] // Evita ciclo de referencias con Mascota
    public Mascota? Mascota { get; set; }

    public string? Observaciones { get; set; }

    // No se exige enviar tratamientos al crear el historial vacio
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}