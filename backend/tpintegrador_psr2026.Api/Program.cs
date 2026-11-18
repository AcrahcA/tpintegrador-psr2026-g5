using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SchemaFilter<EnumSchemaFilter>());

builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();

// -------------------------------------------------------------
// CORS: permite que el frontend (Vite) consuma la API desde el navegador
// -------------------------------------------------------------
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
               ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4173" };
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

// -------------------------------------------------------------
// BASE DE DATOS (PostgreSQL con EF Core, enums nativos)
// -------------------------------------------------------------
builder.Services.AddDbContext<RefugioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"), RefugioDbContext.MapearEnums));
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

// -------------------------------------------------------------
// REPOSITORIOS
// -------------------------------------------------------------
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
builder.Services.AddScoped<IHistorialSanitarioService, HistorialSanitarioService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<IAdoptanteService, AdoptanteService>();
builder.Services.AddScoped<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddScoped<IAdopcionService, AdopcionService>();
builder.Services.AddSingleton<IFotoMascotaStorage, FotoMascotaStorage>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { proyecto = "tpintegrador_psr2026", swagger = "/swagger" }));

app.MapGet("/api/health", (RefugioDbContext db) =>
{
    var conectada = false;
    try { conectada = db.Database.CanConnect(); } catch { /* se informa abajo */ }
    return conectada
        ? Results.Ok(new { estado = "ok", baseDeDatos = "conectada" })
        : Results.Json(new { estado = "error", baseDeDatos = "sin conexión" }, statusCode: 503);
});

app.Run();

public partial class Program;
