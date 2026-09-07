namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface ITratamientoService
{
    List<Tratamiento> ObtenerTratamientos();
    List<Tratamiento> ObtenerTratamientosDeMascota(int mascotaId);
    Tratamiento? BuscarTratamiento(int id);
    Tratamiento? RegistrarTratamiento(int mascotaId, Tratamiento tratamiento);
    bool CambiarEstado(int id, EstadoTratamiento nuevoEstado);
    bool EliminarTratamiento(int id);
}
