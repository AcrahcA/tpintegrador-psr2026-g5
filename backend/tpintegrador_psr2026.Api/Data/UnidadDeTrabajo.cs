namespace tpintegrador_psr2026.Api.Data;

/// <summary>
/// Ejecuta operaciones que modifican varias entidades (adopción, aprobación, ingreso desde aviso)
/// dentro de una única transacción: o se guardan todos los cambios o ninguno.
/// </summary>
public interface IUnidadDeTrabajo
{
    T EjecutarEnTransaccion<T>(Func<T> operacion);
}

public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly RefugioDbContext _context;

    public UnidadDeTrabajo(RefugioDbContext context) => _context = context;

    public T EjecutarEnTransaccion<T>(Func<T> operacion)
    {
        if (_context.Database.CurrentTransaction is not null) return operacion();

        using var transaccion = _context.Database.BeginTransaction();
        try
        {
            var resultado = operacion();
            transaccion.Commit();
            return resultado;
        }
        catch
        {
            transaccion.Rollback();
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
