using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api;

/// <summary>
/// Traduce las excepciones a respuestas ProblemDetails:
/// regla de negocio -> 400, recurso inexistente -> 404, conflicto en la base -> 409, base caída -> 503.
/// </summary>
public class ManejadorErrores : IExceptionHandler
{
    private readonly ILogger<ManejadorErrores> _logger;
    private readonly IHostEnvironment _env;

    public ManejadorErrores(ILogger<ManejadorErrores> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo, detalle) = exception switch
        {
            ReglaNegocioException e => (StatusCodes.Status400BadRequest, "Operación no permitida", e.Message),
            RecursoNoEncontradoException e => (StatusCodes.Status404NotFound, "Recurso inexistente", e.Message),
            DbUpdateException { InnerException: PostgresException pg } when pg.SqlState == PostgresErrorCodes.UniqueViolation =>
                (StatusCodes.Status409Conflict, "Registro duplicado", "La operación viola una restricción de unicidad (" + pg.ConstraintName + ")."),
            DbUpdateException { InnerException: PostgresException pg } when pg.SqlState == PostgresErrorCodes.ForeignKeyViolation =>
                (StatusCodes.Status409Conflict, "Registro relacionado", "El registro está relacionado con otros datos (" + pg.ConstraintName + ")."),
            DbUpdateException e => (StatusCodes.Status409Conflict, "No se pudo guardar", e.InnerException?.Message ?? e.Message),
            NpgsqlException or InvalidOperationException { InnerException: NpgsqlException } =>
                (StatusCodes.Status503ServiceUnavailable, "Base de datos no disponible",
                 "No se pudo conectar con PostgreSQL. Verificá la cadena de conexión 'PostgreSQLConnection' y que el servidor esté iniciado."),
            _ => (StatusCodes.Status500InternalServerError, "Error interno",
                  _env.IsDevelopment() ? exception.Message : "Ocurrió un error inesperado.")
        };

        if (status >= 500) _logger.LogError(exception, "Error no controlado");

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = titulo, Detail = detalle },
            cancellationToken);
        return true;
    }
}
