using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using tpintegrador_psr2026.Api.Data;

namespace tpintegrador_psr2026.Api.Repositories;

public sealed class OperacionAtomica : IOperacionAtomica
{
    private readonly RefugioDbContext _db;
    public OperacionAtomica(RefugioDbContext db) => _db = db;

    public ITransaccionRefugio Iniciar() =>
        new Transaccion(_db.Database.BeginTransaction(IsolationLevel.Serializable));

    private sealed class Transaccion : ITransaccionRefugio
    {
        private readonly IDbContextTransaction _transaccion;
        public Transaccion(IDbContextTransaction transaccion) => _transaccion = transaccion;
        public void Confirmar() => _transaccion.Commit();
        public void Dispose() => _transaccion.Dispose(); // Sin Confirmar => rollback.
    }
}
