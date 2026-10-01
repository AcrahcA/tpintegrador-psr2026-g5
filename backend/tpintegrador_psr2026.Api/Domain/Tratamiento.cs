using System.Text.Json.Serialization;

namespace tpintegrador_psr2026.Api.Domain;

public class Tratamiento
{
    [JsonIgnore] // Se autogenera al guardar
    public int Id { get; set; }

    public int HistorialSanitarioId { get; set; }
    public TipoTratamiento Tipo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public EstadoTratamiento Estado { get; set; }

    // CONSTRUCTOR (mismo patrón que Adopcion.cs y Mascota.cs)
    public Tratamiento(
        int historialSanitarioId,
        TipoTratamiento tipo,
        string? descripcion = null,
        DateTime fechaInicio = default,
        DateTime? fechaFin = null,
        EstadoTratamiento estado = EstadoTratamiento.Pendiente)
    {
        this.HistorialSanitarioId = historialSanitarioId;
        this.Tipo = tipo;
        this.Descripcion = descripcion;
        this.FechaInicio = fechaInicio != default ? fechaInicio : DateTime.Now;
        this.FechaFin = fechaFin;
        this.Estado = estado;
    }
}