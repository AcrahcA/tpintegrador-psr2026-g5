import type { DB } from './types';

// Este módulo sólo calcula información para la interfaz; las reglas definitivas
// y las modificaciones de datos pertenecen a Services de ASP.NET Core.
export type CollectionName = 'mascotas'|'avisos'|'cuidadores'|'tratamientos'|'historiales'|'adoptantes'|'solicitudes';
export type Command =
 | {type:'save'; collection:CollectionName; values:Record<string,unknown>}
 | {type:'state'; collection:'mascotas'|'avisos'|'tratamientos'|'solicitudes'; id:number; state:string}
 | {type:'assign'; id:number; caregiverId:number|null}
 | {type:'intake'; noticeId:number; values:Record<string,unknown>}
 | {type:'adopt'; requestId:number; observations:string}
 | {type:'delete'; collection:CollectionName; id:number}
 | {type:'refugio'; name:string; capacity:number}
 | {type:'publicRequest'; petId:number; person:Record<string,unknown>};

export const occupancy = (db:DB) => db.mascotas.filter(m=>m.estado!=='Adoptada').length;
export const caregiverLoad = (db:DB,id:number) => db.mascotas.filter(m=>m.cuidadorId===id && m.estado!=='Adoptada').length;
export const petById = (db:DB,id:number) => db.mascotas.find(m=>m.id===id);
export const personById = (db:DB,id:number) => db.adoptantes.find(a=>a.id===id);
export const requestById = (db:DB,id:number) => db.solicitudes.find(s=>s.id===id);
export const isBlocking = (db:DB,id:number) => db.tratamientos.some(t=>t.mascotaId===id&&(t.estado==='Pendiente'||t.estado==='EnCurso'));
// Mismo criterio mínimo documentado en ReglasSanitarias.cs (la API decide siempre).
export const sanitaryReady = (db:DB,id:number) => {
 const history=db.historiales.find(h=>h.mascotaId===id);
 return !!history && db.tratamientos.some(t=>t.historialSanitarioId===history.id &&
   t.estado==='Finalizado' && (t.tipo==='Vacunacion'||t.tipo==='ControlVeterinario')) && !isBlocking(db,id);
};
export const adoptionFor = (db:DB,id:number) => db.adopciones.find(a=>db.solicitudes.find(s=>s.id===a.solicitudAdopcionId)?.mascotaId===id);
export const canRequest = (db:DB,id:number) => {
 const m=petById(db,id);
 return !!m && m.estado==='DisponibleAdopcion' && sanitaryReady(db,id) && !adoptionFor(db,id)
   && !db.solicitudes.some(s=>s.mascotaId===id && s.estado==='Aprobada');
};
export const pendingRequests = (db:DB) => db.solicitudes.filter(s=>s.estado==='Pendiente'||s.estado==='EnEvaluacion');
