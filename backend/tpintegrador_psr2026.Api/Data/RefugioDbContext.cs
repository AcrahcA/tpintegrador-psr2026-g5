using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.NameTranslation;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Data;

public class RefugioDbContext : DbContext
{
    public RefugioDbContext(DbContextOptions<RefugioDbContext> options) : base(options) { }

    public DbSet<Refugio> Refugios => Set<Refugio>();
    public DbSet<Cuidador> Cuidadores => Set<Cuidador>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<HistorialSanitario> HistorialesSanitarios => Set<HistorialSanitario>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<AvisoRescate> AvisosRescate => Set<AvisoRescate>();
    public DbSet<Adoptante> Adoptantes => Set<Adoptante>();
    public DbSet<SolicitudAdopcion> SolicitudesAdopcion => Set<SolicitudAdopcion>();
    public DbSet<Adopcion> Adopciones => Set<Adopcion>();
    public DbSet<FotoMascota> FotosMascotas => Set<FotoMascota>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    /// <summary>
    /// Registra los enums de C# como tipos ENUM nativos de PostgreSQL (database/refugio_db.sql).
    /// Los valores se guardan con el mismo nombre que en C# (sin traducir a snake_case).
    /// </summary>
    // Instancia única: EF compara las opciones entre requests y un traductor nuevo
    // por request obligaría a crear un service provider interno cada vez.
    private static readonly NpgsqlNullNameTranslator SinTraduccion = new();

    public static void MapearEnums(NpgsqlDbContextOptionsBuilder npgsql)
    {
        var sinTraduccion = SinTraduccion;
        npgsql.MapEnum<EstadoAviso>("EstadoAvisoEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<EstadoMascota>("EstadoMascotaEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<EstadoTratamiento>("EstadoTratamientoEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<EstadoSolicitud>("EstadoSolicitudEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<TipoAnimal>("TipoAnimalEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<EstadoAparente>("EstadoAparenteEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<NivelUrgencia>("NivelUrgenciaEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<Sexo>("SexoEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<Tamaño>("TamanoEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<TipoTratamiento>("TipoTratamientoEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<RolUsuario>("RolUsuarioEnum", nameTranslator: sinTraduccion);
        npgsql.MapEnum<TipoNotificacion>("TipoNotificacionEnum", nameTranslator: sinTraduccion);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // PostgreSQL "timestamp with time zone" sólo acepta UTC: se normalizan todas las fechas.
        configurationBuilder.Properties<DateTime>().HaveConversion<FechaUtcConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<FechaUtcNullableConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Refugio>(e =>
        {
            e.ToTable("Refugios");
            e.Property(r => r.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Cuidador>(e =>
        {
            e.ToTable("Cuidadores");
            e.Property(c => c.Nombre).HasMaxLength(100);
            e.Property(c => c.Especializacion).HasMaxLength(100);
            e.HasOne(c => c.Refugio).WithMany(r => r.Cuidadores)
                .HasForeignKey(c => c.RefugioId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Mascota>(e =>
        {
            e.ToTable("Mascotas");
            e.Property(m => m.Nombre).HasMaxLength(100);
            e.Property(m => m.Raza).HasMaxLength(50);
            e.HasOne(m => m.Refugio).WithMany(r => r.Mascotas)
                .HasForeignKey(m => m.RefugioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Cuidador).WithMany(c => c.Mascotas)
                .HasForeignKey(m => m.CuidadorId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<AvisoRescate>().WithMany()
                .HasForeignKey(m => m.AvisoRescateOrigenId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(m => m.Estado);
            e.HasIndex(m => m.CuidadorId);
        });

        modelBuilder.Entity<HistorialSanitario>(e =>
        {
            e.ToTable("HistorialesSanitarios");
            e.HasOne(h => h.Mascota).WithOne(m => m.HistorialSanitario)
                .HasForeignKey<HistorialSanitario>(h => h.MascotaId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => h.MascotaId).IsUnique();
        });

        modelBuilder.Entity<Tratamiento>(e =>
        {
            e.ToTable("Tratamientos");
            e.HasOne(t => t.HistorialSanitario).WithMany(h => h.Tratamientos)
                .HasForeignKey(t => t.HistorialSanitarioId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(t => t.EstaActivo);
        });

        modelBuilder.Entity<AvisoRescate>(e =>
        {
            e.ToTable("AvisosRescate");
            e.Property(a => a.ContactoNombre).HasMaxLength(100);
            e.Property(a => a.ContactoTelefono).HasMaxLength(50);
            e.Property(a => a.Direccion).HasMaxLength(200);
            e.HasOne<Mascota>().WithMany()
                .HasForeignKey(a => a.MascotaGeneradaId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => a.Estado);
            e.HasOne<Usuario>().WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.SetNull);
            e.Ignore(a => a.EstaPendiente);
            e.Ignore(a => a.EstaCerrado);
        });

        modelBuilder.Entity<Adoptante>(e =>
        {
            e.ToTable("Adoptantes");
            e.Property(a => a.Nombre).HasMaxLength(100);
            e.Property(a => a.Dni).HasMaxLength(20);
            e.Property(a => a.Telefono).HasMaxLength(50);
            e.Property(a => a.Email).HasMaxLength(100);
            e.Property(a => a.Direccion).HasMaxLength(200);
            e.HasIndex(a => a.Dni).IsUnique();
            e.HasOne<Usuario>().WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(a => a.UsuarioId).IsUnique();
        });

        modelBuilder.Entity<SolicitudAdopcion>(e =>
        {
            e.ToTable("SolicitudesAdopcion");
            e.HasOne(s => s.Adoptante).WithMany(a => a.Solicitudes)
                .HasForeignKey(s => s.AdoptanteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Mascota).WithMany()
                .HasForeignKey(s => s.MascotaId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => s.Estado);
            e.Ignore(s => s.EstaPendiente);
            e.Ignore(s => s.EstaVigente);
        });

        modelBuilder.Entity<Adopcion>(e =>
        {
            e.ToTable("Adopciones");
            e.HasOne(a => a.SolicitudAdopcion).WithOne(s => s.Adopcion)
                .HasForeignKey<Adopcion>(a => a.SolicitudAdopcionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Mascota>().WithMany().HasForeignKey(a => a.MascotaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Adoptante>().WithMany().HasForeignKey(a => a.AdoptanteId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => a.SolicitudAdopcionId).IsUnique();
            e.HasIndex(a => a.MascotaId).IsUnique(); // una mascota no puede tener dos adopciones definitivas
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.Property(u => u.Nombre).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(150);
            e.Property(u => u.PasswordHash).HasMaxLength(200);
            e.Property(u => u.Telefono).HasMaxLength(50);
            e.HasIndex(u => u.Email).IsUnique();
            e.Ignore(u => u.EsAdministrador);
        });

        modelBuilder.Entity<Notificacion>(e =>
        {
            e.ToTable("Notificaciones");
            e.Property(n => n.Titulo).HasMaxLength(200);
            e.Property(n => n.Motivo).HasMaxLength(Notificacion.LargoMaximoMotivo);
            e.HasOne(n => n.Usuario).WithMany(u => u.Notificaciones)
                .HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<SolicitudAdopcion>().WithMany().HasForeignKey(n => n.SolicitudAdopcionId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne<AvisoRescate>().WithMany().HasForeignKey(n => n.AvisoRescateId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(n => new { n.UsuarioId, n.Leida });
        });

        modelBuilder.Entity<FotoMascota>(e =>
        {
            e.ToTable("FotosMascotas");
            e.Property(f => f.Url).HasMaxLength(300);
            e.Property(f => f.Descripcion).HasMaxLength(FotoMascota.LargoMaximoDescripcion);
            e.HasOne(f => f.Mascota).WithMany(m => m.Fotos)
                .HasForeignKey(f => f.MascotaId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(f => new { f.MascotaId, f.Orden });
        });

        modelBuilder.Entity<Mascota>().Ignore(m => m.EstaAlojada).Ignore(m => m.FueAdoptada);
    }

    private static DateTime AUtc(DateTime fecha) => Fechas.AUtc(fecha);

    private class FechaUtcConverter : ValueConverter<DateTime, DateTime>
    {
        public FechaUtcConverter() : base(v => AUtc(v), v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }

    private class FechaUtcNullableConverter : ValueConverter<DateTime?, DateTime?>
    {
        public FechaUtcNullableConverter() : base(
            v => v.HasValue ? AUtc(v.Value) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v) { }
    }
}
