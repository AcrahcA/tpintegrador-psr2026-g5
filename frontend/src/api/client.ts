import type {
  Adopcion, Adoptante, AdoptanteDatos, Aviso, AvisoDatos, Capacidad, Cuidador, EstadoAviso, EstadoMascota,
  EstadoSolicitud, EstadoTratamiento, EvaluacionAdopcion, Foto, Historial, Mascota, MascotaDatos, Notificacion, Refugio,
  Sesion, Solicitud, TipoTratamiento, Tratamiento
} from './types';

// Sólo el backend habla con PostgreSQL. Nunca guardar contraseñas en variables VITE_.
export const API_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5080').replace(/\/$/, '');

export class ApiError extends Error {
  constructor(message: string, public status: number) { super(message); }
}

type Opciones = { method?: string; body?: unknown; form?: FormData; signal?: AbortSignal };

async function request<T>(path: string, { method = 'GET', body, form, signal }: Opciones = {}): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      signal,
      credentials: 'include',   // envía la cookie de sesión (HttpOnly) a la API
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: form ?? (body === undefined ? undefined : JSON.stringify(body))
    });
  } catch (e) {
    if (e instanceof DOMException && e.name === 'AbortError') throw e;
    throw new ApiError('No se pudo conectar con el servidor. Revisá tu conexión e intentá de nuevo.', 0);
  }

  const texto = await response.text();
  let datos: unknown = undefined;
  try { datos = texto ? JSON.parse(texto) : undefined; } catch { datos = texto; }

  if (!response.ok) throw new ApiError(mensajeDeError(datos, response.status), response.status);
  return datos as T;
}

function mensajeDeError(datos: unknown, status: number): string {
  if (datos && typeof datos === 'object') {
    const d = datos as { detail?: string; title?: string; errors?: Record<string, string[]> };
    if (d.errors) {
      const primeros = Object.values(d.errors).flat().slice(0, 3);
      return primeros.length ? `Hay datos con formato inválido: ${primeros.join(' ')}` : 'Hay datos con formato inválido.';
    }
    if (d.detail) return d.detail;
    if (d.title) return d.title;
  }
  if (typeof datos === 'string' && datos.trim()) return datos.slice(0, 300);
  if (status === 503) return 'El servidor no puede acceder a la base de datos en este momento.';
  return `La operación no pudo completarse (error ${status}).`;
}

/** Las fotos subidas viven en la API; las de ejemplo, en el propio frontend. */
export const fotoSrc = (url: string) => (url.startsWith('/api/') ? `${API_URL}${url}` : url);

const json = (method: string, body?: unknown) => ({ method, body });

export const api = {
  // Refugio
  refugio: () => request<Refugio>('/api/Refugio'),
  capacidad: () => request<Capacidad>('/api/Refugio/capacidad'),
  configurarRefugio: (nombre: string, capacidadMaxima: number) =>
    request<Refugio>('/api/Refugio', json('POST', { nombre, capacidadMaxima })),

  // Mascotas
  mascotas: (estado?: EstadoMascota, signal?: AbortSignal) =>
    request<Mascota[]>(estado ? `/api/Mascotas?estado=${estado}` : '/api/Mascotas', { signal }),
  mascota: (id: number) => request<Mascota>(`/api/Mascotas/${id}`),
  crearMascota: (datos: MascotaDatos) => request<Mascota>('/api/Mascotas', json('POST', datos)),
  actualizarMascota: (id: number, datos: MascotaDatos) => request<Mascota>(`/api/Mascotas/${id}`, json('PUT', datos)),
  cambiarEstadoMascota: (id: number, estado: EstadoMascota) => request<Mascota>(`/api/Mascotas/${id}/estado`, json('PUT', estado)),
  asignarCuidador: (id: number, cuidadorId: number) => request<Mascota>(`/api/Mascotas/${id}/cuidador/${cuidadorId}`, json('PUT')),
  quitarCuidador: (id: number) => request<Mascota>(`/api/Mascotas/${id}/cuidador`, json('DELETE')),
  eliminarMascota: (id: number) => request<void>(`/api/Mascotas/${id}`, json('DELETE')),
  evaluacionAdopcion: (id: number) => request<EvaluacionAdopcion>(`/api/Mascotas/${id}/disponibilidad-adopcion`),
  historial: (id: number) => request<Historial>(`/api/Mascotas/${id}/historial`),

  // Fotos
  subirFotos: (mascotaId: number, archivos: { archivo: File; descripcion: string }[]) => {
    const form = new FormData();
    for (const a of archivos) {
      form.append('fotos', a.archivo, a.archivo.name);
      form.append('descripciones', a.descripcion);
    }
    return request<Foto[]>(`/api/Mascotas/${mascotaId}/fotos`, { method: 'POST', form });
  },
  actualizarFoto: (mascotaId: number, fotoId: number, cambios: { descripcion?: string; principal?: boolean }) =>
    request<Foto>(`/api/Mascotas/${mascotaId}/fotos/${fotoId}`, json('PUT', cambios)),
  moverFoto: (mascotaId: number, fotoId: number, desplazamiento: -1 | 1) =>
    request<Foto[]>(`/api/Mascotas/${mascotaId}/fotos/${fotoId}/mover?desplazamiento=${desplazamiento}`, json('PUT')),
  eliminarFoto: (mascotaId: number, fotoId: number) => request<void>(`/api/Mascotas/${mascotaId}/fotos/${fotoId}`, json('DELETE')),

  // Avisos de rescate
  avisos: () => request<Aviso[]>('/api/avisos-rescate'),
  crearAviso: (datos: AvisoDatos) => request<Aviso>('/api/avisos-rescate', json('POST', datos)),
  cambiarEstadoAviso: (id: number, estado: EstadoAviso) => request<Aviso>(`/api/avisos-rescate/${id}/estado`, json('PUT', estado)),
  ingresarDesdeAviso: (id: number, datos: Partial<MascotaDatos>) =>
    request<Mascota>(`/api/avisos-rescate/${id}/mascotas`, json('POST', datos)),
  eliminarAviso: (id: number) => request<void>(`/api/avisos-rescate/${id}`, json('DELETE')),

  // Cuidadores
  cuidadores: () => request<Cuidador[]>('/api/Cuidadores'),
  crearCuidador: (d: Omit<Cuidador, 'id' | 'mascotasAsignadas'>) => request<Cuidador>('/api/Cuidadores', json('POST', d)),
  actualizarCuidador: (id: number, d: Omit<Cuidador, 'id' | 'mascotasAsignadas'>) => request<Cuidador>(`/api/Cuidadores/${id}`, json('PUT', d)),
  eliminarCuidador: (id: number) => request<void>(`/api/Cuidadores/${id}`, json('DELETE')),

  // Tratamientos e historial
  tratamientos: () => request<Tratamiento[]>('/api/Tratamientos'),
  crearTratamiento: (mascotaId: number, d: { tipo: TipoTratamiento; descripcion: string | null; fechaInicio: string | null; estado: EstadoTratamiento }) =>
    request<Tratamiento>(`/api/Tratamientos/mascotas/${mascotaId}`, json('POST', d)),
  cambiarEstadoTratamiento: (id: number, estado: EstadoTratamiento) => request<Tratamiento>(`/api/Tratamientos/${id}/estado`, json('PUT', estado)),
  eliminarTratamiento: (id: number) => request<void>(`/api/Tratamientos/${id}`, json('DELETE')),
  actualizarHistorial: (id: number, mascotaId: number, observaciones: string) =>
    request(`/api/historiales-sanitarios/${id}`, json('PUT', { mascotaId, observaciones })),

  // Adoptantes, solicitudes y adopciones
  adoptantes: () => request<Adoptante[]>('/api/Adoptantes'),
  crearAdoptante: (d: AdoptanteDatos) => request<Adoptante>('/api/Adoptantes', json('POST', d)),
  actualizarAdoptante: (id: number, d: AdoptanteDatos) => request<Adoptante>(`/api/Adoptantes/${id}`, json('PUT', d)),
  eliminarAdoptante: (id: number) => request<void>(`/api/Adoptantes/${id}`, json('DELETE')),
  solicitudes: () => request<Solicitud[]>('/api/solicitudes-adopcion'),
  crearSolicitud: (adoptanteId: number, mascotaId: number) =>
    request<Solicitud>('/api/solicitudes-adopcion', json('POST', { adoptanteId, mascotaId })),
  solicitudPublica: (mascotaId: number, adoptante: AdoptanteDatos) =>
    request<Solicitud>('/api/solicitudes-adopcion/publica', json('POST', { mascotaId, ...adoptante })),
  cambiarEstadoSolicitud: (id: number, estado: EstadoSolicitud) =>
    request<Solicitud>(`/api/solicitudes-adopcion/${id}/estado`, json('PUT', estado)),
  adopciones: () => request<Adopcion[]>('/api/Adopciones'),
  registrarAdopcion: (solicitudAdopcionId: number, observaciones: string | null) =>
    request<Adopcion>('/api/Adopciones', json('POST', { solicitudAdopcionId, observaciones })),

  // Rechazos con motivo opcional (le llega a la persona en su buzón)
  rechazarSolicitud: (id: number, motivo: string | null) =>
    request<Solicitud>(`/api/solicitudes-adopcion/${id}/rechazar`, json('PUT', { motivo })),
  cancelarSolicitud: (id: number, motivo: string | null) =>
    request<Solicitud>(`/api/solicitudes-adopcion/${id}/cancelar`, json('PUT', { motivo })),
  descartarAviso: (id: number, motivo: string | null) =>
    request<Aviso>(`/api/avisos-rescate/${id}/descartar`, json('PUT', { motivo })),

  // Sitio público (sin sesión)
  mascotasDisponibles: () => request<Mascota[]>('/api/Mascotas/disponibles'),
  estadisticas: () => request<{ disponibles: number; adopciones: number }>('/api/Refugio/estadisticas'),

  // Cuentas
  sesion: () => request<Sesion>('/api/auth/yo'),
  login: (email: string, contraseña: string) => request<Sesion>('/api/auth/login', json('POST', { email, contraseña })),
  registro: (d: { nombre: string; email: string; contraseña: string; telefono: string | null }) =>
    request<Sesion>('/api/auth/registro', json('POST', d)),
  logout: () => request<void>('/api/auth/logout', json('POST')),

  // Buzón
  buzon: () => request<Notificacion[]>('/api/buzon'),
  buzonNoLeidas: () => request<{ cantidad: number }>('/api/buzon/no-leidas'),
  marcarLeida: (id: number) => request<Notificacion>(`/api/buzon/${id}/leida`, json('PUT')),
  marcarTodasLeidas: () => request<void>('/api/buzon/leer-todas', json('PUT'))
};
