export const PET_STATES = ['Ingresada','EnTratamiento','EnRecuperacion','DisponibleAdopcion','Reservada','Adoptada'] as const;
export const AVISO_STATES = ['Reportado','EnEvaluacion','Aceptado','Atendido','Descartado'] as const;
export const TR_STATES = ['Pendiente','EnCurso','Finalizado','Suspendido'] as const;
export const REQ_STATES = ['Pendiente','EnEvaluacion','Aprobada','Rechazada','Cancelada'] as const;
export const T_TYPES = ['Medicacion','Vacunacion','Curacion','Desparasitacion','RecuperacionPosquirurgica','ControlVeterinario'] as const;
export const ANIMALS = ['Perro','Gato','Ave','Otro'] as const;
export const SIZES = ['Pequenio','Mediano','Grande'] as const;
export const SEXES = ['Macho','Hembra'] as const;
export const APPARENT = ['Sano','Herido','Enfermo','Desnutrido','Critico'] as const;
export const URGENCIES = ['Baja','Media','Alta'] as const;
export type PetState = typeof PET_STATES[number];
export type NoticeState = typeof AVISO_STATES[number];
export type TreatmentState = typeof TR_STATES[number];
export type RequestState = typeof REQ_STATES[number];
export interface Mascota { id:number; nombre:string; especie:string; raza:string; edadAproximada:number; sexo:string; tamano:string; estado:PetState; cuidadorId:number|null; fechaIngreso:string; foto:string; avisoRescateOrigenId?:number|null; }
export interface Aviso { id:number; descripcion:string; tipoAnimal:string; contactoNombre:string; contactoTelefono:string; latitud:number; longitud:number; estadoAparente:string; nivelUrgencia:string; estado:NoticeState; fechaAviso:string; mascotaGeneradaId:number|null; }
export interface Cuidador { id:number; nombre:string; especializacion:string; capacidadMaxima:number; }
export interface Tratamiento { id:number; mascotaId:number; historialSanitarioId:number; tipo:string; descripcion:string; estado:TreatmentState; fechaInicio:string; fechaFin:string|null; }
export interface Historial { id:number; mascotaId:number; observaciones:string; }
export interface Adoptante { id:number; nombre:string; dni:string; telefono:string; direccion:string; }
export interface Solicitud { id:number; adoptanteId:number; mascotaId:number; estado:RequestState; fechaSolicitud:string; }
export interface Adopcion { id:number; solicitudAdopcionId:number; fechaAdopcion:string; observaciones:string; }
export interface Refugio { nombre:string; capacidadMaxima:number; }
export interface DB { version:2; refugio:Refugio; mascotas:Mascota[]; avisos:Aviso[]; cuidadores:Cuidador[]; tratamientos:Tratamiento[]; historiales:Historial[]; adoptantes:Adoptante[]; solicitudes:Solicitud[]; adopciones:Adopcion[]; }
export const labels:Record<string,string> = { Pequenio:'Pequeño',EnTratamiento:'En tratamiento',EnRecuperacion:'En recuperación',DisponibleAdopcion:'Disponible para adopción',EnEvaluacion:'En evaluación',EnCurso:'En curso',Medicacion:'Medicación',Vacunacion:'Vacunación',Curacion:'Curación',Desparasitacion:'Desparasitación',RecuperacionPosquirurgica:'Recuperación posquirúrgica',ControlVeterinario:'Control veterinario',Critico:'Crítico',Finalizado:'Finalizado',Suspendido:'Suspendido',Ingresada:'Ingresada',Reservada:'Reservada',Adoptada:'Adoptada',Aceptado:'Aceptado',Pendiente:'Pendiente',Aprobada:'Aprobada',Rechazada:'Rechazada',Cancelada:'Cancelada' };
export const display = (s:unknown) => labels[String(s)] || String(s ?? '—');
export const dateLabel = (s:string|null|undefined) => s ? s.slice(0,10).split('-').reverse().join('/') : '—';
export const today = () => new Date().toLocaleDateString('en-CA');
export const photo = (m?: { foto?:string } | null) => `/assets/img/${m?.foto || 'pet-maple.jpg'}`;
