namespace tpintegrador_psr2026.Api.Services;

using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;

/// <summary>Archivo recibido para agregar a la galería de una mascota.</summary>
public record ArchivoFoto(Stream Contenido, string ContentType, long Tamaño, string? Descripcion);

public interface IFotoMascotaService
{
    List<FotoMascota> Listar(int mascotaId);
    Dictionary<int, List<FotoMascota>> ListarPorMascotas(IEnumerable<int> mascotaIds);
    List<FotoMascota> Agregar(int mascotaId, IReadOnlyList<ArchivoFoto> archivos);
    FotoMascota Actualizar(int mascotaId, int fotoId, string? descripcion, bool principal);
    /// <summary>Mueve la foto una posición antes (-1) o después (+1) en la galería.</summary>
    List<FotoMascota> Mover(int mascotaId, int fotoId, int desplazamiento);
    void Eliminar(int mascotaId, int fotoId);
}

public class FotoMascotaService : IFotoMascotaService
{
    public const int MaximoPorCarga = 6;

    private readonly IFotoMascotaRepository _fotoRepository;
    private readonly IMascotaRepository _mascotaRepository;
    private readonly IFotoMascotaStorage _storage;

    public FotoMascotaService(IFotoMascotaRepository fotoRepository, IMascotaRepository mascotaRepository, IFotoMascotaStorage storage)
    {
        _fotoRepository = fotoRepository;
        _mascotaRepository = mascotaRepository;
        _storage = storage;
    }

    private Mascota ObtenerMascota(int mascotaId) =>
        _mascotaRepository.GetPorId(mascotaId) ?? throw new RecursoNoEncontradoException($"No existe la mascota #{mascotaId}.");

    private FotoMascota ObtenerFoto(int mascotaId, int fotoId)
    {
        var foto = _fotoRepository.GetPorId(fotoId);
        if (foto is null || foto.MascotaId != mascotaId)
            throw new RecursoNoEncontradoException($"La mascota #{mascotaId} no tiene la foto #{fotoId}.");
        return foto;
    }

    public List<FotoMascota> Listar(int mascotaId)
    {
        ObtenerMascota(mascotaId);
        return _fotoRepository.GetPorMascota(mascotaId);
    }

    public Dictionary<int, List<FotoMascota>> ListarPorMascotas(IEnumerable<int> mascotaIds) =>
        _fotoRepository.GetPorMascotas(mascotaIds);

    public List<FotoMascota> Agregar(int mascotaId, IReadOnlyList<ArchivoFoto> archivos)
    {
        var mascota = ObtenerMascota(mascotaId);
        if (archivos.Count == 0)
            throw new ReglaNegocioException("Seleccioná al menos una foto.");
        if (archivos.Count > MaximoPorCarga)
            throw new ReglaNegocioException($"Podés subir hasta {MaximoPorCarga} fotos por vez.");

        var existentes = _fotoRepository.GetPorMascota(mascotaId);
        if (existentes.Count + archivos.Count > FotoMascota.MaximoPorMascota)
            throw new ReglaNegocioException(
                $"Cada mascota puede tener hasta {FotoMascota.MaximoPorMascota} fotos (ya tiene {existentes.Count}).");

        // Se validan todos los archivos antes de guardar ninguno
        var descripciones = archivos.Select(a => FotoMascota.NormalizarDescripcion(a.Descripcion, mascota.Nombre)).ToList();
        foreach (var a in archivos) _storage.Validar(a.Contenido, a.ContentType, a.Tamaño);

        var orden = existentes.Count == 0 ? 0 : existentes.Max(f => f.Orden) + 1;
        var creadas = new List<FotoMascota>();
        var guardadas = new List<string>();
        try
        {
            for (var i = 0; i < archivos.Count; i++)
            {
                var url = _storage.Guardar(archivos[i].Contenido, archivos[i].ContentType);
                guardadas.Add(url);
                creadas.Add(_fotoRepository.Post(new FotoMascota(mascotaId, url, descripciones[i], orden++)));
            }
        }
        catch
        {
            foreach (var url in guardadas.Where(u => creadas.All(c => c.Url != u))) _storage.Eliminar(url);
            throw;
        }
        return creadas;
    }

    public FotoMascota Actualizar(int mascotaId, int fotoId, string? descripcion, bool principal)
    {
        var mascota = ObtenerMascota(mascotaId);
        var foto = ObtenerFoto(mascotaId, fotoId);
        if (descripcion is not null)
            foto.Descripcion = FotoMascota.NormalizarDescripcion(descripcion, mascota.Nombre);

        if (principal)
        {
            var orden = 1;
            foreach (var otra in _fotoRepository.GetPorMascota(mascotaId).Where(f => f.Id != fotoId))
                otra.Orden = orden++;
            foto.Orden = 0;
        }
        _fotoRepository.GuardarCambios();
        return foto;
    }

    public List<FotoMascota> Mover(int mascotaId, int fotoId, int desplazamiento)
    {
        ObtenerFoto(mascotaId, fotoId);
        var fotos = _fotoRepository.GetPorMascota(mascotaId);
        var indice = fotos.FindIndex(f => f.Id == fotoId);
        var destino = Math.Clamp(indice + Math.Sign(desplazamiento), 0, fotos.Count - 1);
        if (destino != indice)
        {
            var foto = fotos[indice];
            fotos.RemoveAt(indice);
            fotos.Insert(destino, foto);
        }
        for (var i = 0; i < fotos.Count; i++) fotos[i].Orden = i;
        _fotoRepository.GuardarCambios();
        return fotos;
    }

    public void Eliminar(int mascotaId, int fotoId)
    {
        var foto = ObtenerFoto(mascotaId, fotoId);
        _fotoRepository.Delete(foto.Id);
        _storage.Eliminar(foto.Url);
    }
}
