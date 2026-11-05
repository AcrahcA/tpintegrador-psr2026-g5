using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api;
using tpintegrador_psr2026.Api.Data;
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
builder.Services.AddDbContext<RefugioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection")));

// -------------------------------------------------------------
// REPOSITORIOS (Scoped para trabajar correctamente con DbContext)
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
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<IAdoptanteService, AdoptanteService>();
builder.Services.AddScoped<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddScoped<IAdopcionService, AdopcionService>();
builder.Services.AddScoped<IHistorialSanitarioService, HistorialSanitarioService>();

var app = builder.Build();

// Configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    proyecto = "tpintegrador_psr2026",
    estado = "API conectada a PostgreSQL correctamente"
}));

app.Run();

public partial class Program;