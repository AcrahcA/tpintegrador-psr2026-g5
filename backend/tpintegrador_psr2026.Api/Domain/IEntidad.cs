namespace tpintegrador_psr2026.Api.Domain;

/// <summary>Toda entidad persistida se identifica por un Id numérico.</summary>
public interface IEntidad
{
    int Id { get; set; }
}
