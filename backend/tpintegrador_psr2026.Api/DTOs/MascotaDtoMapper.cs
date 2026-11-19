using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Services;

namespace tpintegrador_psr2026.Api.DTOs;

/// <summary>Arma las respuestas de mascotas incluyendo su galería de fotos.</summary>
public class MascotaDtoMapper
{
    private readonly IFotoMascotaService _fotos;

    public MascotaDtoMapper(IFotoMascotaService fotos) => _fotos = fotos;

    public MascotaResponseDto Uno(Mascota m) => MascotaResponseDto.Desde(m, _fotos.ListarPorMascotas(new[] { m.Id }).GetValueOrDefault(m.Id));

    public List<MascotaResponseDto> Muchos(IEnumerable<Mascota> mascotas)
    {
        var lista = mascotas.ToList();
        var fotos = _fotos.ListarPorMascotas(lista.Select(m => m.Id));
        return lista.Select(m => MascotaResponseDto.Desde(m, fotos.GetValueOrDefault(m.Id))).ToList();
    }
}
