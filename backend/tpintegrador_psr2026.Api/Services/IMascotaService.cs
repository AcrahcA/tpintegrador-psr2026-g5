namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;

public interface IMascotaService
{
    List<Mascota> Get();
    Mascota? GetById(int id);
    Mascota? Post(Mascota mascota);
    bool PutEstado(int id, EstadoMascota nuevoEstado);
    bool AsignarCuidador(int mascotaId, int cuidadorId);
    bool EstaDisponibleParaAdopcion(int mascotaId);
    bool CumpleCondicionesSanitarias(int mascotaId);
    bool Delete(int id);
}
