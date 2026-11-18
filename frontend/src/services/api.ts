import type { Command } from '../model/rules';
import type { DB, Aviso, Mascota, Tratamiento } from '../model/types';

// Solo el backend habla con PostgreSQL. Nunca guardar contraseñas en variables VITE_.
export const API_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5080').replace(/\/$/, '');
export class GuardadoParcialError extends Error {}

type Obj = Record<string, unknown>;
const value = (v: unknown) => String(v ?? '');
const numeric = (v: unknown) => Number(v ?? 0);
const nullable = (v: unknown) => v === undefined || v === null ? null : Number(v);
const asObj = (v: unknown): Obj => v && typeof v === 'object' && !Array.isArray(v) ? v as Obj : {};
const date = (v: unknown) => value(v).slice(0, 10);
const dateTime = (v: unknown) => {
  const text = value(v);
  const dt = text ? new Date(text.length === 10 ? `${text}T12:00:00Z` : text) : new Date();
  return Number.isNaN(dt.getTime()) ? new Date().toISOString() : dt.toISOString();
};

export async function request<T = unknown>(path: string, method = 'GET', body?: unknown): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body)
    });
  } catch {
    throw new Error(`No se pudo conectar con la API (${API_URL}). Verificá que el backend esté ejecutándose y que PostgreSQL esté disponible.`);
  }
  const raw = await response.text();
  let parsed: unknown = raw;
  try { parsed = raw ? JSON.parse(raw) : undefined; } catch { /* errores del backend en texto */ }
  if (!response.ok) {
    const info = asObj(parsed);
    const msg = value(info.detail || info.title || info.message || parsed || 'La operación no pudo completarse.');
    throw new Error(`Error ${response.status}: ${msg.slice(0, 420)}`);
  }
  return parsed as T;
}

const GET = (path: string) => request<Obj[]>(path);
// Nunca asignar fotos de otros animales: la URL viene de PostgreSQL (o null).
const petImage = (url: unknown) => {
  const relative = value(url);
  return /^\/api\/Mascotas\/imagenes\/[0-9a-f]{32}\.(jpg|png|webp)$/.test(relative)
    ? `${API_URL}${relative}` : null;
};

export async function uploadMascotaFoto(id: number, file: File): Promise<void> {
  if (!['image/jpeg','image/png','image/webp'].includes(file.type) || file.size === 0 || file.size > 5 * 1024 * 1024)
    throw new Error('Seleccioná una foto JPG, PNG o WebP de hasta 5 MB.');
  const body = new FormData();
  body.append('foto', file, file.name);
  let response: Response;
  try { response = await fetch(`${API_URL}/api/Mascotas/${id}/foto`, { method: 'POST', body }); }
  catch { throw new Error('No se pudo enviar la fotografía al backend.'); }
  if (!response.ok) throw new Error(`No se pudo guardar la fotografía (HTTP ${response.status}): ${(await response.text()).slice(0,300)}`);
}

export async function loadDatabase(): Promise<DB> {
  // No hay datos de seed ni localStorage: todos los registros proceden del servidor.
  const [refugio, mascotas, avisos, cuidadores, historiales, tratamientos, adoptantes, solicitudes, adopciones] = await Promise.all([
    request<Obj>('/api/Refugio'), GET('/api/Mascotas'), GET('/api/avisos-rescate'),
    GET('/api/Cuidadores'), GET('/api/historiales-sanitarios'), GET('/api/Tratamientos'),
    GET('/api/Adoptantes'), GET('/api/solicitudes-adopcion'), GET('/api/Adopciones')
  ]);
  const hMap = new Map(historiales.map(h => [numeric(h.id), numeric(h.mascotaId)]));
  return {
    version: 2,
    refugio: { nombre: value(refugio.nombre || 'Refugio'), capacidadMaxima: numeric(refugio.capacidadMaxima) },
    mascotas: mascotas.map(m => ({ id: numeric(m.id), nombre: value(m.nombre), especie: value(m.especie), raza: value(m.raza),
      edadAproximada: numeric(m.edadAproximada), sexo: value(m.sexo), tamano: value(m['tamaño'] ?? m.tamano),
      estado: value(m.estado) as Mascota['estado'], cuidadorId: nullable(m.cuidadorId), fechaIngreso: date(m.fechaIngreso),
      foto: petImage(m.fotoUrl), avisoRescateOrigenId: nullable(m.avisoRescateOrigenId)
    })),
    avisos: avisos.map(a => ({ id: numeric(a.id), descripcion: value(a.descripcion), tipoAnimal: value(a.tipoAnimal),
      latitud: numeric(a.latitud), longitud: numeric(a.longitud), direccion: value(a.direccion),
      contactoNombre: value(a.contactoNombre), contactoTelefono: value(a.contactoTelefono),
      estadoAparente: value(a.estadoAparente), nivelUrgencia: value(a.nivelUrgencia),
      estado: value(a.estado) as Aviso['estado'], fechaAviso: date(a.fechaAviso), mascotaGeneradaId: nullable(a.mascotaGeneradaId)
    })),
    cuidadores: cuidadores.map(c => ({ id: numeric(c.id), nombre: value(c.nombre),
      especializacion: value(c.especializacion), capacidadMaxima: numeric(c.capacidadMaxima) })),
    historiales: historiales.map(h => ({ id: numeric(h.id), mascotaId: numeric(h.mascotaId), observaciones: value(h.observaciones) })),
    tratamientos: tratamientos.map(t => ({ id: numeric(t.id), mascotaId: hMap.get(numeric(t.historialSanitarioId)) || 0,
      historialSanitarioId: numeric(t.historialSanitarioId), tipo: value(t.tipo), descripcion: value(t.descripcion),
      estado: value(t.estado) as Tratamiento['estado'], fechaInicio: date(t.fechaInicio), fechaFin: t.fechaFin ? date(t.fechaFin) : null
    })),
    adoptantes: adoptantes.map(a => ({ id: numeric(a.id), nombre: value(a.nombre), dni: value(a.dni),
      telefono: value(a.telefono), direccion: value(a.direccion) })),
    solicitudes: solicitudes.map(s => ({ id: numeric(s.id), adoptanteId: numeric(s.adoptanteId),
      mascotaId: numeric(s.mascotaId), estado: value(s.estado) as DB['solicitudes'][number]['estado'], fechaSolicitud: date(s.fechaSolicitud) })),
    adopciones: adopciones.map(a => ({ id: numeric(a.id), solicitudAdopcionId: numeric(a.solicitudAdopcionId),
      fechaAdopcion: date(a.fechaAdopcion), observaciones: value(a.observaciones) }))
  };
}

function mascotaDTO(v: Obj) {
  return { nombre: value(v.nombre).trim(), especie: value(v.especie), raza: value(v.raza) || null,
    edadAproximada: numeric(v.edadAproximada), sexo: value(v.sexo), 'tamaño': value(v.tamano), estado: 'Ingresada' };
}
function avisoDTO(v: Obj) {
  const latitude = numeric(v.latitud), longitude = numeric(v.longitud);
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude) || latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180 ||
      (latitude === 0 && longitude === 0)) throw new Error('Indicá una dirección y buscala en el mapa, o ingresá coordenadas válidas.');
  return { descripcion: value(v.descripcion), tipoAnimal: value(v.tipoAnimal), latitud: latitude, longitud: longitude,
    direccion: value(v.direccion).trim(), contactoNombre: value(v.contactoNombre), contactoTelefono: value(v.contactoTelefono),
    estadoAparente: value(v.estadoAparente), nivelUrgencia: value(v.nivelUrgencia) };
}
const adoptanteDTO = (v: Obj) => ({ nombre: value(v.nombre), dni: value(v.dni).replace(/\D/g, ''),
  telefono: value(v.telefono), direccion: value(v.direccion) });
const cuidadorDTO = (v: Obj) => ({ nombre: value(v.nombre), especializacion: value(v.especializacion), capacidadMaxima: numeric(v.capacidadMaxima) });
const historialDTO = (v: Obj) => ({ mascotaId: numeric(v.mascotaId), observaciones: value(v.observaciones) });
const solicitudDTO = (v: Obj) => ({ mascotaId: numeric(v.mascotaId), adoptanteId: numeric(v.adoptanteId), estado: 'Pendiente' });
const tratamientoDTO = (v: Obj, id: number) => ({ historialSanitarioId: id, tipo: value(v.tipo),
  descripcion: value(v.descripcion), fechaInicio: dateTime(v.fechaInicio), fechaFin: null, estado: 'Pendiente' });

export async function executeRemote(command: Command, db: DB): Promise<void> {
  switch (command.type) {
    case 'save': {
      const v = asObj(command.values), id = numeric(v.id), edit = id > 0;
      switch (command.collection) {
        case 'mascotas': {
          const pet = await request<Obj>(edit ? `/api/Mascotas/${id}` : '/api/Mascotas', edit ? 'PUT' : 'POST', mascotaDTO(v));
          const petId = edit ? id : numeric(pet.id);
          try {
            if (v.cuidadorId && numeric(v.cuidadorId)) {
              const existing = db.mascotas.find(m => m.id === id);
              if (!edit || existing?.cuidadorId !== numeric(v.cuidadorId))
                await request(`/api/Mascotas/${petId}/cuidador/${numeric(v.cuidadorId)}`, 'PUT');
            } else if (edit && db.mascotas.find(m => m.id === id)?.cuidadorId)
              await request(`/api/Mascotas/${petId}/cuidador`, 'DELETE');
            if (v.fotoArchivo instanceof File) await uploadMascotaFoto(petId, v.fotoArchivo);
            else if (edit && v.quitarFoto === true) await request(`/api/Mascotas/${petId}/foto`, 'DELETE');
          } catch (error) {
            throw new GuardadoParcialError(`Los datos de la mascota #${petId} se guardaron, pero hubo un problema con la fotografía o el cuidador. Abrí Editar para completar la operación. ${error instanceof Error ? error.message : ''}`);
          }
          return;
        }
        case 'avisos': await request(edit ? `/api/avisos-rescate/${id}` : '/api/avisos-rescate', edit ? 'PUT' : 'POST', avisoDTO(v)); return;
        case 'cuidadores': await request(edit ? `/api/Cuidadores/${id}` : '/api/Cuidadores', edit ? 'PUT' : 'POST', cuidadorDTO(v)); return;
        case 'historiales': await request(edit ? `/api/historiales-sanitarios/${id}` : '/api/historiales-sanitarios', edit ? 'PUT' : 'POST', historialDTO(v)); return;
        case 'adoptantes': await request(edit ? `/api/Adoptantes/${id}` : '/api/Adoptantes', edit ? 'PUT' : 'POST', adoptanteDTO(v)); return;
        case 'solicitudes': {
          if (edit) throw new Error('Las solicitudes se editan mediante cambios de estado.');
          await request('/api/solicitudes-adopcion', 'POST', solicitudDTO(v)); return;
        }
        case 'tratamientos': {
          const mascotaId = numeric(v.mascotaId);
          const existing = db.tratamientos.find(x => x.id === id);
          const historialId = existing?.historialSanitarioId || db.historiales.find(h => h.mascotaId === mascotaId)?.id || 0;
          await request(edit ? `/api/Tratamientos/${id}` : `/api/Tratamientos/mascotas/${mascotaId}`,
            edit ? 'PUT' : 'POST', tratamientoDTO(v, historialId)); return;
        }
      }
      return;
    }
    case 'state': {
      if (command.collection === 'mascotas' && command.state === 'Adoptada')
        throw new Error('La adopción se completa desde una solicitud aprobada, no cambiando el estado manualmente.');
      const route: Record<typeof command.collection, string> = { mascotas: 'Mascotas', avisos: 'avisos-rescate',
        tratamientos: 'Tratamientos', solicitudes: 'solicitudes-adopcion' };
      await request(`/api/${route[command.collection]}/${command.id}/estado`, 'PUT', command.state);
      return;
    }
    case 'assign': {
      if (command.caregiverId === null) await request(`/api/Mascotas/${command.id}/cuidador`, 'DELETE');
      else await request(`/api/Mascotas/${command.id}/cuidador/${command.caregiverId}`, 'PUT');
      return;
    }
    case 'intake': {
      const notice = db.avisos.find(a => a.id === command.noticeId);
      if (!notice || notice.estado !== 'Aceptado') throw new Error('El aviso debe estar aceptado para ingresar una mascota.');
      const m = mascotaDTO({ ...command.values, especie: notice.tipoAnimal });
      const pet = await request<Obj>(`/api/avisos-rescate/${notice.id}/mascotas`, 'POST', m);
      try {
        if (command.values.cuidadorId) await request(`/api/Mascotas/${numeric(pet.id)}/cuidador/${numeric(command.values.cuidadorId)}`, 'PUT');
        if (command.values.fotoArchivo instanceof File) await uploadMascotaFoto(numeric(pet.id), command.values.fotoArchivo);
      } catch (error) {
        throw new GuardadoParcialError(`La mascota #${numeric(pet.id)} se ingresó desde el aviso, pero hubo un problema con su foto o cuidador. Editá el registro para completar la operación. ${error instanceof Error ? error.message : ''}`);
      }
      return;
    }
    case 'adopt': {
      const solicitud = db.solicitudes.find(s => s.id === command.requestId);
      if (!solicitud || solicitud.estado !== 'Aprobada') throw new Error('Primero debés aprobar la solicitud de adopción.');
      await request('/api/Adopciones', 'POST', {
        solicitudAdopcionId: command.requestId, fechaAdopcion: new Date().toISOString(), observaciones: command.observations
      }); return;
    }
    case 'delete': {
      const routes = { mascotas: 'Mascotas', avisos: 'avisos-rescate', cuidadores: 'Cuidadores', tratamientos: 'Tratamientos',
        historiales: 'historiales-sanitarios', adoptantes: 'Adoptantes', solicitudes: 'solicitudes-adopcion' };
      await request(`/api/${routes[command.collection]}/${command.id}`, 'DELETE'); return;
    }
    case 'refugio': {
      await request('/api/Refugio', 'POST', { nombre: command.name, capacidadMaxima: command.capacity }); return;
    }
    case 'publicRequest': {
      await request('/api/solicitudes-adopcion/publica', 'POST', {
        mascotaId: command.petId,
        ...adoptanteDTO(command.person)
      }); return;
    }
  }
}
