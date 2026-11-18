namespace tpintegrador_psr2026.Api.Domain;

public sealed class OperacionNoPermitidaException : Exception
{
    public OperacionNoPermitidaException(string mensaje) : base(mensaje) { }
}
