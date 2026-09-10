using tpintegrador_psr2026.Api;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Controladores: Enums como Strings y prevención de referencias circulares
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
// REPOSITORIOS (Acceso a datos en memoria)
// -------------------------------------------------------------
builder.Services.AddSingleton<IRefugioRepository, RefugioRepository>();
builder.Services.AddSingleton<IAvisoRescateRepository, AvisoRescateRepository>();
builder.Services.AddSingleton<IMascotaRepository, MascotaRepository>();
builder.Services.AddSingleton<ICuidadorRepository, CuidadorRepository>();
builder.Services.AddSingleton<IHistorialSanitarioRepository, HistorialSanitarioRepository>();
builder.Services.AddSingleton<ITratamientoRepository, TratamientoRepository>();
builder.Services.AddSingleton<IAdoptanteRepository, AdoptanteRepository>();
builder.Services.AddSingleton<ISolicitudAdopcionRepository, SolicitudAdopcionRepository>();
builder.Services.AddSingleton<IAdopcionRepository, AdopcionRepository>();

// -------------------------------------------------------------
// SERVICIOS (Lógica y reglas de negocio)
// -------------------------------------------------------------
builder.Services.AddSingleton<IRefugioService, RefugioService>();
builder.Services.AddSingleton<IMascotaService, MascotaService>();
builder.Services.AddSingleton<IAvisoRescateService, AvisoRescateService>();
builder.Services.AddSingleton<ICuidadorService, CuidadorService>();
builder.Services.AddSingleton<ITratamientoService, TratamientoService>();
builder.Services.AddSingleton<IAdoptanteService, AdoptanteService>();
builder.Services.AddSingleton<ISolicitudAdopcionService, SolicitudAdopcionService>();
builder.Services.AddSingleton<IAdopcionService, AdopcionService>();
builder.Services.AddSingleton<IHistorialSanitarioRepository, HistorialSanitarioRepository>();
builder.Services.AddScoped<IHistorialSanitarioService, HistorialSanitarioService>();

var app = builder.Build();

// Configuración para Swagger en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Ruta raíz para rápida comprobación del servidor
app.MapGet("/", () => Results.Ok(new
{
    proyecto = "tpintegrador_psr2026",
    estado = "API funcionando"
}));

app.Run();

public partial class Program;