namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IAvisoRescateService
{
    List<AvisoRescate> Get();
    List<AvisoRescate> GetPendientes();
    AvisoRescate? GetById(int id);
    AvisoRescate Post(AvisoRescate aviso);
    bool PutEstado(int id, EstadoAviso nuevoEstado);
    Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota);
    bool Delete(int id);
}