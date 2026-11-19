// Tipos que devuelve la API (backend ASP.NET Core). Los enums viajan como texto.

export const ESTADOS_MASCOTA = ['Ingresada', 'EnTratamiento', 'EnRecuperacion', 'DisponibleAdopcion', 'Reservada', 'Adoptada'] as const;
export const ESTADOS_AVISO = ['Reportado', 'EnEvaluacion', 'Aceptado', 'Atendido', 'Descartado'] as const;
export const ESTADOS_TRATAMIENTO = ['Pendiente', 'EnCurso', 'Finalizado', 'Suspendido'] as const;
export const ESTADOS_SOLICITUD = ['Pendiente', 'EnEvaluacion', 'Aprobada', 'Rechazada', 'Cancelada'] as const;
export const TIPOS_ANIMAL = ['Perro', 'Gato', 'Ave', 'Otro'] as const;
export const ESTADOS_APARENTES = ['Sano', 'Herido', 'Enfermo', 'Desnutrido', 'Critico'] as const;
export const NIVELES_URGENCIA = ['Baja', 'Media', 'Alta'] as const;
export const SEXOS = ['Macho', 'Hembra'] as const;
export const TAMAÑOS = ['Pequenio', 'Mediano', 'Grande'] as const;
export const TIPOS_TRATAMIENTO = ['Medicacion', 'Vacunacion', 'Curacion', 'Desparasitacion', 'RecuperacionPosquirurgica', 'ControlVeterinario'] as const;

export type EstadoMascota = typeof ESTADOS_MASCOTA[number];
export type EstadoAviso = typeof ESTADOS_AVISO[number];
export type EstadoTratamiento = typeof ESTADOS_TRATAMIENTO[number];
export type EstadoSolicitud = typeof ESTADOS_SOLICITUD[number];
export type TipoAnimal = typeof TIPOS_ANIMAL[number];
export type EstadoAparente = typeof ESTADOS_APARENTES[number];
export type NivelUrgencia = typeof NIVELES_URGENCIA[number];
export type Sexo = typeof SEXOS[number];
export type Tamaño = typeof TAMAÑOS[number];
export type TipoTratamiento = typeof TIPOS_TRATAMIENTO[number];

export interface Foto { id: number; url: string; descripcion: string; orden: number; }

export interface Mascota {
  id: number; nombre: string; especie: TipoAnimal; raza: string | null; edadAproximada: number;
  sexo: Sexo; tamaño: Tamaño; fechaIngreso: string; estado: EstadoMascota;
  cuidadorId: number | null; avisoRescateOrigenId: number | null; fotoUrl: string | null; fotos: Foto[];
}

export interface MascotaDatos {
  nombre: string; especie: TipoAnimal; raza: string | null; edadAproximada: number; sexo: Sexo; tamaño: Tamaño;
}

export interface Aviso {
  id: number; descripcion: string; tipoAnimal: TipoAnimal; latitud: number; longitud: number; direccion: string | null;
  fechaAviso: string; contactoNombre: string; contactoTelefono: string | null; estadoAparente: EstadoAparente;
  nivelUrgencia: NivelUrgencia; estado: EstadoAviso; mascotaGeneradaId: number | null;
}

export interface AvisoDatos {
  descripcion: string; tipoAnimal: TipoAnimal; latitud: number; longitud: number; direccion: string | null;
  contactoNombre: string; contactoTelefono: string | null; estadoAparente: EstadoAparente; nivelUrgencia: NivelUrgencia;
}

export interface Cuidador {
  id: number; nombre: string; especializacion: string | null; capacidadMaxima: number; disponible: boolean; mascotasAsignadas: number;
}

export interface Tratamiento {
  id: number; historialSanitarioId: number; mascotaId: number; tipo: TipoTratamiento; descripcion: string | null;
  fechaInicio: string; fechaFin: string | null; estado: EstadoTratamiento;
}

export interface Historial {
  id: number; mascotaId: number; observaciones: string | null; cumpleCondicionesSanitarias: boolean;
  motivos: string[]; tratamientos: Tratamiento[];
}

export interface Adoptante { id: number; nombre: string; dni: string; telefono: string | null; email: string | null; direccion: string | null; }
export interface AdoptanteDatos { nombre: string; dni: string; telefono: string | null; email: string | null; direccion: string | null; }

export interface Solicitud { id: number; adoptanteId: number; mascotaId: number; fechaSolicitud: string; estado: EstadoSolicitud; }

export interface Adopcion {
  id: number; solicitudAdopcionId: number; mascotaId: number; adoptanteId: number; fechaAdopcion: string; observaciones: string | null;
}

export interface Refugio { id: number; nombre: string; capacidadMaxima: number; }
export interface Capacidad { capacidadMaxima: number; mascotasActuales: number; plazasDisponibles: number; tieneLugarDisponible: boolean; }

export interface EvaluacionAdopcion {
  mascotaId: number; estado: EstadoMascota; cumpleCondicionesSanitarias: boolean; disponibleParaAdopcion: boolean; motivos: string[];
}
