namespace tpintegrador_psr2026.Api.Domain;

public static class Fechas
{
    /// <summary>Normaliza una fecha a UTC (las fechas sin zona se interpretan como UTC).</summary>
    public static DateTime AUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
    };
}
