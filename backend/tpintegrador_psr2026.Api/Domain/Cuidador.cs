using System.Globalization;
using System.Text;

namespace tpintegrador_psr2026.Api.Domain;

public class Cuidador : IEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Texto libre con la experiencia del cuidador (p. ej. "Perros", "Gatos y aves", "General").
    /// Si menciona tipos de animal, sólo podrá atender esos tipos; si no, se considera generalista.
    /// </summary>
    public string? Especializacion { get; set; }
    public int CapacidadMaxima { get; set; }
    public bool Disponible { get; set; } = true;

    public int? RefugioId { get; set; }
    public Refugio? Refugio { get; set; }

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();

    protected Cuidador() { }

    public Cuidador(string nombre, int capacidadMaxima, string? especializacion = null, bool disponible = true)
    {
        Nombre = nombre;
        CapacidadMaxima = capacidadMaxima;
        Especializacion = especializacion;
        Disponible = disponible;
    }

    private static readonly Dictionary<TipoAnimal, string[]> PalabrasPorTipo = new()
    {
        [TipoAnimal.Perro] = new[] { "perro", "canino", "cachorro" },
        [TipoAnimal.Gato] = new[] { "gato", "felino" },
        [TipoAnimal.Ave] = new[] { "ave", "pajaro" },
        [TipoAnimal.Otro] = new[] { "otro", "exotico", "roedor", "reptil", "conejo" }
    };

    /// <summary>Tipos de animal que menciona la especialización. Vacío = generalista (atiende a todos).</summary>
    public IReadOnlyCollection<TipoAnimal> TiposEspecializados()
    {
        if (string.IsNullOrWhiteSpace(Especializacion)) return Array.Empty<TipoAnimal>();
        var texto = Normalizar(Especializacion);
        var palabras = texto.Split(new[] { ' ', ',', ';', '/', '-', '.', '+', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        return PalabrasPorTipo
            .Where(par => palabras.Any(p => par.Value.Any(clave => p.StartsWith(clave))))
            .Select(par => par.Key)
            .ToList();
    }

    public bool PuedeAtender(TipoAnimal especie)
    {
        var tipos = TiposEspecializados();
        return tipos.Count == 0 || tipos.Contains(especie);
    }

    public bool TieneCapacidad(int mascotasAsignadas) => mascotasAsignadas < CapacidadMaxima;

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
            throw new ReglaNegocioException("El nombre del cuidador es obligatorio.");
        if (CapacidadMaxima <= 0)
            throw new ReglaNegocioException("La capacidad máxima del cuidador debe ser mayor a cero.");
    }

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in descompuesto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString();
    }
}
