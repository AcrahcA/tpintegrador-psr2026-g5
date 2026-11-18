namespace tpintegrador_psr2026.Api.Repositories;

using tpintegrador_psr2026.Api.Domain;

public interface IAvisoRescateRepository : IRepositorio<AvisoRescate>
{
    List<AvisoRescate> GetPorEstado(EstadoAviso estado);
    /// <summary>Avisos todavía no resueltos (Reportado o EnEvaluacion), los más urgentes primero.</summary>
    List<AvisoRescate> GetPendientes();
}
