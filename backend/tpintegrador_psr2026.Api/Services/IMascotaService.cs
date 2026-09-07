namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IMascotaService
{
    List<Mascota> ObtenerMascotas();
    Mascota? BuscarMascota(int id);
    Mascota? IngresarMascota(Mascota mascota);
    bool CambiarEstado(int id, EstadoMascota nuevoEstado);
    bool AsignarCuidador(int mascotaId, int cuidadorId);
    bool EstaDisponibleParaAdopcion(int mascotaId);
    bool CumpleCondicionesSanitarias(int mascotaId);
    bool EliminarMascota(int id);
}
