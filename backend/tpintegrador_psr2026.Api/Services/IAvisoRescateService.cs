namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IAvisoRescateService
{
    List<AvisoRescate> ObtenerAvisos();
    List<AvisoRescate> ObtenerPendientes();
    AvisoRescate? BuscarAviso(int id);
    AvisoRescate RegistrarAviso(AvisoRescate aviso);
    bool CambiarEstado(int id, EstadoAviso nuevoEstado);
    Mascota? AtenderAvisoYGenerarMascota(int avisoId, Mascota datosMascota);
    bool EliminarAviso(int id);
}