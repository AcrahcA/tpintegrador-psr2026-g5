using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Services;

/// <summary>
/// Política mínima explícita para la práctica escolar: sin tratamiento pendiente/en curso,
/// historial existente y al menos un control veterinario o vacunación FINALIZADO.
/// No reemplaza un calendario real de vacunas definido por veterinarios.
/// </summary>
public static class ReglasSanitarias
{
    public static bool Cumple(int mascotaId,
        IEnumerable<HistorialSanitario> historiales, IEnumerable<Tratamiento> tratamientos)
    {
        var historial = historiales.FirstOrDefault(h => h.MascotaId == mascotaId);
        if (historial is null) return false;
        var atenciones = tratamientos.Where(t => t.HistorialSanitarioId == historial.Id).ToList();
        if (atenciones.Any(t => t.Estado is EstadoTratamiento.Pendiente or EstadoTratamiento.EnCurso))
            return false;
        return atenciones.Any(t => t.Estado == EstadoTratamiento.Finalizado &&
            t.Tipo is TipoTratamiento.Vacunacion or TipoTratamiento.ControlVeterinario);
    }
}
