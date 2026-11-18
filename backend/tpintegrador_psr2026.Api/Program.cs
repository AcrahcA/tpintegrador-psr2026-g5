using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Domain;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<EnumSchemaFilter>();
});

// -------------------------------------------------------------
// BASE DE DATOS (PostgreSQL con EF Core)
// -------------------------------------------------------------
var conexion = builder.Configuration.GetConnectionString("PostgreSQLConnection");
if (string.IsNullOrWhiteSpace(conexion) || conexion.Contains("TU_CONTRASEÑA") || conexion.Contains("TU_USUARIO"))
    throw new InvalidOperationException("Configurá ConnectionStrings__PostgreSQLConnection mediante una variable de entorno local.");
builder.Services.AddDbContext<RefugioDbContext>(options =>
    options.UseNpgsql(conexion));

// -------------------------------------------------------------
// REPOSITORIOS (Scoped para trabajar correctamente con DbContext)
// -------------------------------------------------------------
builder.Services.AddScoped<IOperacionAtomica, OperacionAtomica>();
builder.Services.AddScoped<IRefugioRepository, RefugioRepository>();
builder.Services.AddScoped<IAvisoRescateRepository, AvisoRescateRepository>();
builder.Services.AddScoped<IMascotaRepository, MascotaRepository>();
builder.Services.AddScoped<ICuidadorRepository, CuidadorRepository>();
builder.Services.AddScoped<IHistorialSanitarioRepository, HistorialSanitarioRepository>();
builder.Services.AddScoped<ITratamientoRepository, TratamientoRepository>();
builder.Services.AddScoped<IAdoptanteRepository, AdoptanteRepository>();
builder.Services.AddScoped<ISolicitudAdopcionRepository, SolicitudAdopcionRepository>();
builder.Services.AddScoped<IAdopcionRepository, AdopcionRepository>();

// -------------------------------------------------------------
// SERVICIOS
// -------------------------------------------------------------
builder.Services.AddScoped<IRefugioService, RefugioService>();
builder.Services.AddScoped<IMascotaService, MascotaService>();
builder.Services.AddScoped<IAvisoRescateService, AvisoRescateService>();
builder.Services.AddScoped<ICuidadorService, CuidadorService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<IAdoptanteService, AdoptanteService>();
builder.Services.AddScoped<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddScoped<IAdopcionService, AdopcionService>();
builder.Services.AddScoped<IHistorialSanitarioService, HistorialSanitarioService>();
builder.Services.AddScoped<IMascotaFotoService, MascotaFotoService>();

// Solo permitimos el frontend local durante el desarrollo.
builder.Services.AddCors(options => options.AddPolicy("PatitasLocal", policy =>
    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
          .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Errores de reglas de negocio claros para React; errores inesperados siguen registrándose.
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (ArgumentException ex)
    {
        if (context.Response.HasStarted) throw;
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { title = "Datos no válidos", detail = ex.Message });
    }
    catch (OperacionNoPermitidaException ex)
    {
        if (context.Response.HasStarted) throw;
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { title = "Operación no disponible", detail = ex.Message });
    }
});

// Inicializa el esquema en una base VACÍA de desarrollo. No reemplaza migraciones en producción.
if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:EnsureCreatedOnDevelopment"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RefugioDbContext>();
    db.Database.EnsureCreated();
    // Para una base existente, agrega la nueva columna sin eliminar registros previos.
    db.Database.ExecuteSqlRaw("ALTER TABLE IF EXISTS \"AvisosRescate\" ADD COLUMN IF NOT EXISTS \"Direccion\" text NULL;");
    db.Database.ExecuteSqlRaw("ALTER TABLE IF EXISTS \"Mascotas\" ADD COLUMN IF NOT EXISTS \"FotoUrl\" text NULL;");
}


// Configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PatitasLocal");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/health", async (RefugioDbContext db) =>
{
    try
    {
        return await db.Database.CanConnectAsync()
            ? (IResult)Results.Ok(new { api = "ok", database = "PostgreSQL accesible" })
            : Results.Json(new { api = "ok", database = "No accesible" }, statusCode: 503);
    }
    catch
    {
        return Results.Json(new { api = "ok", database = "No accesible" }, statusCode: 503);
    }
});

app.MapGet("/", () => Results.Ok(new
{
    proyecto = "tpintegrador_psr2026",
    estado = "API iniciada; consultar /api/health para comprobar la base de datos"
}));

app.Run();

public partial class Program;