namespace tpintegrador_psr2026.Api.Domain;

/// <summary>
/// Condiciones sanitarias que debe cumplir una mascota para ser habilitada para adopción:
/// no tener tratamientos activos y poseer las vacunas / controles requeridos ya finalizados.
/// </summary>
public static class RequisitosSanitarios
{
    public static readonly IReadOnlyList<TipoTratamiento> TratamientosRequeridos = new[]
    {
        TipoTratamiento.Vacunacion,
        TipoTratamiento.ControlVeterinario
    };

    public static List<string> Evaluar(IEnumerable<Tratamiento> tratamientos)
    {
        var lista = tratamientos.ToList();
        var motivos = new List<string>();

        var activos = lista.Where(t => t.EstaActivo).ToList();
        if (activos.Count > 0)
            motivos.Add($"Tiene {activos.Count} tratamiento(s) pendiente(s) o en curso: {string.Join(", ", activos.Select(t => t.Tipo))}.");

        foreach (var requerido in TratamientosRequeridos)
            if (!lista.Any(t => t.Tipo == requerido && t.Estado == EstadoTratamiento.Finalizado))
                motivos.Add($"No posee un tratamiento de tipo {requerido} finalizado.");

        return motivos;
    }

    public static bool Cumple(IEnumerable<Tratamiento> tratamientos) => Evaluar(tratamientos).Count == 0;
}
