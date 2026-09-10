using System.Text.Json.Serialization;

namespace tpintegrador_psr2026.Api.Domain;

public class Adoptante
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    // Al usar [JsonIgnore] simple, Swagger oculta todo el árbol de solicitudes en el POST/PUT
    [JsonIgnore]
    public ICollection<SolicitudAdopcion> Solicitudes { get; set; } = new List<SolicitudAdopcion>();
}