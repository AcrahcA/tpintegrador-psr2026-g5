using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api;
using tpintegrador_psr2026.Api.Data;
using tpintegrador_psr2026.Api.Repositories;
using tpintegrador_psr2026.Api.Services;
using tpintegrador_psr2026.Api.Services.Seguridad;

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
// CORS: el frontend (Vite) consume la API desde el navegador y envía la cookie de sesión
// -------------------------------------------------------------
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
               ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:4173" };
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// -------------------------------------------------------------
// INICIO DE SESIÓN: cookie HttpOnly + roles (Usuario / Administrador)
// -------------------------------------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "patitas.sesion";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        // Es una API: en lugar de redirigir a una página de login, responde 401 / 403
        options.Events.OnRedirectToLogin = ctx => EscribirProblema(ctx.Response, 401, "Iniciá sesión", "Tenés que iniciar sesión para hacer esto.");
        options.Events.OnRedirectToAccessDenied = ctx => EscribirProblema(ctx.Response, 403, "Sin permiso", "Esta acción es sólo para el equipo del refugio.");
        // En cada pedido se vuelve a leer el rol desde la base: si un usuario pasa a ser administrador
        // (o deja de serlo) el cambio vale de inmediato, y si la cuenta se borra la sesión deja de valer.
        options.Events.OnValidatePrincipal = async ctx =>
        {
            var id = ctx.Principal?.UsuarioId();
            var usuario = id is null ? null : ctx.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>().GetPorId(id.Value);
            if (usuario is null) { ctx.RejectPrincipal(); await Sesion.CerrarAsync(ctx.HttpContext); return; }
            if (!ctx.Principal!.IsInRole(usuario.Rol.ToString()) || ctx.Principal.Identity?.Name != usuario.Nombre)
            {
                ctx.ReplacePrincipal(Sesion.CrearPrincipal(usuario));
                ctx.ShouldRenew = true;
            }
        };
    });
builder.Services.AddAuthorization();

// Límite de intentos para registro e inicio de sesión (evita probar contraseñas a la fuerza)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.OnRejected = (ctx, _) => new ValueTask(EscribirProblema(ctx.HttpContext.Response, 429, "Demasiados intentos",
        "Hiciste demasiados intentos seguidos. Esperá un minuto y volvé a probar."));
});

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
builder.Services.AddScoped<IFotoMascotaRepository, FotoMascotaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();

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
builder.Services.AddScoped<IFotoMascotaService, FotoMascotaService>();
builder.Services.AddScoped<tpintegrador_psr2026.Api.DTOs.MascotaDtoMapper>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
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

static Task EscribirProblema(HttpResponse response, int status, string titulo, string detalle)
{
    response.StatusCode = status;
    return response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = titulo, Detail = detalle });
}

public partial class Program;
