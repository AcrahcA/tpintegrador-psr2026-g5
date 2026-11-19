import type { EstadoAviso, EstadoMascota, EstadoSolicitud, EstadoTratamiento } from './types';

const ETIQUETAS: Record<string, string> = {
  Ingresada: 'Ingresada', EnTratamiento: 'En tratamiento', EnRecuperacion: 'En recuperación',
  DisponibleAdopcion: 'Disponible para adopción', Reservada: 'Reservada', Adoptada: 'Adoptada',
  Reportado: 'Reportado', EnEvaluacion: 'En evaluación', Aceptado: 'Aceptado', Atendido: 'Atendido', Descartado: 'Descartado',
  Pendiente: 'Pendiente', EnCurso: 'En curso', Finalizado: 'Finalizado', Suspendido: 'Suspendido',
  Aprobada: 'Aprobada', Rechazada: 'Rechazada', Cancelada: 'Cancelada',
  Perro: 'Perro', Gato: 'Gato', Ave: 'Ave', Otro: 'Otro',
  Sano: 'Sano', Herido: 'Herido', Enfermo: 'Enfermo', Desnutrido: 'Desnutrido', Critico: 'Crítico',
  Baja: 'Baja', Media: 'Media', Alta: 'Alta',
  Macho: 'Macho', Hembra: 'Hembra',
  Pequenio: 'Pequeño', Mediano: 'Mediano', Grande: 'Grande',
  Medicacion: 'Medicación', Vacunacion: 'Vacunación', Curacion: 'Curación', Desparasitacion: 'Desparasitación',
  RecuperacionPosquirurgica: 'Recuperación posquirúrgica', ControlVeterinario: 'Control veterinario'
};

export const etiqueta = (valor: string | null | undefined) => (valor ? ETIQUETAS[valor] ?? valor : '—');

export const plural: Record<string, string> = { Perro: 'Perros', Gato: 'Gatos', Ave: 'Aves', Otro: 'Otros' };

export function edad(años: number) {
  if (años <= 0) return 'Menos de 1 año';
  return años === 1 ? '1 año' : `${años} años`;
}

const formatoFecha = new Intl.DateTimeFormat('es-AR', { day: 'numeric', month: 'long', year: 'numeric' });
const formatoFechaCorta = new Intl.DateTimeFormat('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });

export const fecha = (iso: string | null | undefined) => (iso ? formatoFecha.format(new Date(iso)) : '—');
export const fechaCorta = (iso: string | null | undefined) => (iso ? formatoFechaCorta.format(new Date(iso)) : '—');

/** Tono visual de cada estado. El estado siempre se muestra también como texto (no sólo color). */
export type Tono = 'verde' | 'ambar' | 'rojo' | 'azul' | 'violeta' | 'gris';

export function tonoEstado(estado: EstadoMascota | EstadoAviso | EstadoTratamiento | EstadoSolicitud | string): Tono {
  switch (estado) {
    case 'DisponibleAdopcion': case 'Aceptado': case 'Aprobada': case 'Finalizado': case 'Atendido': return 'verde';
    case 'Ingresada': case 'Pendiente': case 'Reportado': case 'Reservada': return 'ambar';
    case 'EnTratamiento': case 'Rechazada': case 'Alta': case 'Critico': return 'rojo';
    case 'EnCurso': case 'EnEvaluacion': return 'azul';
    case 'EnRecuperacion': case 'Adoptada': return 'violeta';
    default: return 'gris';
  }
}

// Transiciones permitidas (espejo de las reglas del backend) para ofrecer sólo acciones válidas.
export const transicionesAviso: Record<EstadoAviso, EstadoAviso[]> = {
  Reportado: ['EnEvaluacion', 'Aceptado', 'Descartado'], EnEvaluacion: ['Aceptado', 'Descartado'],
  Aceptado: ['Descartado'], Atendido: [], Descartado: []
};
export const transicionesTratamiento: Record<EstadoTratamiento, EstadoTratamiento[]> = {
  Pendiente: ['EnCurso', 'Finalizado', 'Suspendido'], EnCurso: ['Finalizado', 'Suspendido'],
  Suspendido: ['EnCurso', 'Pendiente'], Finalizado: []
};
export const transicionesSolicitud: Record<EstadoSolicitud, EstadoSolicitud[]> = {
  Pendiente: ['EnEvaluacion', 'Aprobada', 'Rechazada', 'Cancelada'], EnEvaluacion: ['Aprobada', 'Rechazada', 'Cancelada'],
  Aprobada: ['Cancelada'], Rechazada: [], Cancelada: []
};
export const estadosManualesMascota: EstadoMascota[] = ['EnTratamiento', 'EnRecuperacion', 'DisponibleAdopcion'];

export const accionAviso: Partial<Record<EstadoAviso, string>> = {
  EnEvaluacion: 'Pasar a evaluación', Aceptado: 'Aceptar', Descartado: 'Descartar'
};
export const accionTratamiento: Partial<Record<EstadoTratamiento, string>> = {
  EnCurso: 'Iniciar', Finalizado: 'Finalizar', Suspendido: 'Suspender', Pendiente: 'Volver a pendiente'
};
export const accionSolicitud: Partial<Record<EstadoSolicitud, string>> = {
  EnEvaluacion: 'Evaluar', Aprobada: 'Aprobar', Rechazada: 'Rechazar', Cancelada: 'Cancelar'
};
