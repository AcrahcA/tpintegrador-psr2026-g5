using Microsoft.EntityFrameworkCore;
using tpintegrador_psr2026.Api.Domain;

namespace tpintegrador_psr2026.Api.Data
{
    public class RefugioDbContext : DbContext
    {
        public RefugioDbContext(DbContextOptions<RefugioDbContext> options) : base(options)
        {
        }

        public DbSet<Refugio> Refugios { get; set; }
        public DbSet<Cuidador> Cuidadores { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<HistorialSanitario> HistorialesSanitarios { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<AvisoRescate> AvisosRescate { get; set; }
        public DbSet<Adoptante> Adoptantes { get; set; }
        public DbSet<SolicitudAdopcion> SolicitudesAdopcion { get; set; }
        public DbSet<Adopcion> Adopciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo de Enums nativos de PostgreSQL
            modelBuilder.HasPostgresEnum<EstadoMascotaEnum>();
            modelBuilder.HasPostgresEnum<SexoEnum>();
            modelBuilder.HasPostgresEnum<TamanoEnum>();
            modelBuilder.HasPostgresEnum<EstadoSolicitudEnum>();
            modelBuilder.HasPostgresEnum<EstadoAvisoEnum>();
            modelBuilder.HasPostgresEnum<TipoAnimalEnum>();
            modelBuilder.HasPostgresEnum<EstadoAparenteEnum>();
            modelBuilder.HasPostgresEnum<NivelUrgenciaEnum>();
            modelBuilder.HasPostgresEnum<TipoTratamientoEnum>();
            modelBuilder.HasPostgresEnum<EstadoTratamientoEnum>();
        }
    }

    // Definición de los Enums sincronizados con el dominio
    public enum EstadoMascotaEnum { Ingresada, EnTratamiento, DisponibleAdopcion, Reservada, Adoptada }
    public enum SexoEnum { Hembra, Macho }
    public enum TamanoEnum { Pequeno, Mediano, Grande }
    public enum EstadoSolicitudEnum { Pendiente, EnEvaluacion, Aprobada, Rechazada, Cancelada }
    public enum EstadoAvisoEnum { Reportado, En_Evaluacion, Aceptado, Atendido, Descartado }
    public enum TipoAnimalEnum { Gato, Perro, Ave, Otro }
    public enum EstadoAparenteEnum { Sano, Herido, Enfermo, Desnutrido, Critico }
    public enum NivelUrgenciaEnum { Baja, Media, Alta }
    public enum TipoTratamientoEnum { Medicacion, Curacion, Vacunacion, Desparasitacion, RecuperacionPosQuirurgica, ControlVeterinario }
    public enum EstadoTratamientoEnum { Pendiente, EnCurso, Finalizado, Suspendido }
}