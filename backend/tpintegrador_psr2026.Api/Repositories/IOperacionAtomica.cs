namespace tpintegrador_psr2026.Api.Repositories;

// Coordina varios Repositories en una misma transacción PostgreSQL.
public interface IOperacionAtomica
{
    ITransaccionRefugio Iniciar();
}

public interface ITransaccionRefugio : IDisposable
{
    void Confirmar();
}
