namespace tpintegrador_psr2026.Api.Domain;

/// <summary>Se lanza cuando una operación viola una regla de negocio (HTTP 400).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

/// <summary>Se lanza cuando el recurso solicitado no existe (HTTP 404).</summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
}
