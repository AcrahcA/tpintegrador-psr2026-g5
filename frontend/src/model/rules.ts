import { AVISO_STATES, PET_STATES, REQ_STATES, TR_STATES, today, type DB, type Mascota, type Solicitud, type PetState, type TreatmentState, type RequestState, type NoticeState, type Aviso, type Cuidador, type Historial, type Adoptante, type Tratamiento } from './types.js';

export type CollectionName = 'mascotas'|'avisos'|'cuidadores'|'tratamientos'|'historiales'|'adoptantes'|'solicitudes';
export type Command =
 | { type:'save'; collection:CollectionName; values:Record<string,unknown> }
 | { type:'state'; collection:'mascotas'|'avisos'|'tratamientos'|'solicitudes'; id:number; state:string }
 | { type:'assign'; id:number; caregiverId:number|null }
 | { type:'intake'; noticeId:number; values:Record<string,unknown> }
 | { type:'adopt'; requestId:number; observations:string }
 | { type:'delete'; collection:CollectionName; id:number }
 | { type:'refugio'; name:string; capacity:number }
 | { type:'publicRequest'; petId:number; person:Record<string,unknown> };

const text = (v:unknown) => String(v??'').trim();
const number = (v:unknown) => Number(v);
function ensure(cond:unknown,message:string): asserts cond { if (!cond) throw new Error(message); }
const nextId = (records:{id:number}[]) => Math.max(0,...records.map(r=>r.id))+1;
const find = <T extends {id:number}>(records:T[],id:number,label='Registro'):T => { const x=records.find(r=>r.id===id);if(!x) throw new Error(`${label} no encontrado.`);return x; };
const occupied = (db:DB) => db.mascotas.filter(p=>p.estado!=='Adoptada').length;
export const occupancy = occupied;
export const caregiverLoad = (db:DB,id:number) => db.mascotas.filter(p=>p.cuidadorId===id&&p.estado!=='Adoptada').length;
export const petById = (db:DB,id:number) => db.mascotas.find(p=>p.id===id);
export const personById = (db:DB,id:number) => db.adoptantes.find(p=>p.id===id);
export const requestById = (db:DB,id:number) => db.solicitudes.find(p=>p.id===id);
export const isBlocking = (db:DB,id:number) => db.tratamientos.some(t=>t.mascotaId===id && (t.estado==='Pendiente'||t.estado==='EnCurso'));
export const adoptionFor = (db:DB,id:number) => db.adopciones.find(a=>db.solicitudes.find(s=>s.id===a.solicitudAdopcionId)?.mascotaId===id);
export const canRequest = (db:DB,id:number) => {const p=petById(db,id);return !!p&&p.estado==='DisponibleAdopcion'&&!isBlocking(db,id)&&!adoptionFor(db,id)&&!db.solicitudes.some(s=>s.mascotaId===id&&s.estado==='Aprobada');};
export const pendingRequests = (db:DB) => db.solicitudes.filter(s=>['Pendiente','EnEvaluacion'].includes(s.estado));
const checkName = (v:unknown,label:string) => {const x=text(v);ensure(x.length>=2,`${label}: ingresá al menos dos caracteres.`);return x;};
const choose = (v:unknown,items:readonly string[],label:string) => {const x=text(v);ensure(items.includes(x),`${label} inválido.`);return x;};
const positive = (v:unknown,label:string,zero=false) => { const n=number(v); ensure(Number.isFinite(n)&&(zero?n>=0:n>0),`Ingresá un valor válido para ${label}.`);return n; };
const validCoord = (n:unknown,min:number,max:number,name:string) => {const v=number(n);ensure(Number.isFinite(v)&&v>=min&&v<=max,`${name} debe estar entre ${min} y ${max}.`);return v;};
const checkCapacity = (db:DB, id:number|null,petId?:number) => {if(id===null)return;const c=find(db.cuidadores,id,'Cuidador');const count=db.mascotas.filter(p=>p.cuidadorId===id&&p.estado!=='Adoptada'&&p.id!==petId).length;ensure(count<c.capacidadMaxima,`${c.nombre} alcanzó su capacidad máxima (${c.capacidadMaxima}).`);};
const ensurePetCanBeAvailable = (db:DB,id:number) => {ensure(!isBlocking(db,id),'La mascota tiene tratamientos pendientes o en curso.');ensure(!adoptionFor(db,id),'La mascota ya fue adoptada.');};
function ensureHistory(db:DB,id:number):Historial {let h=db.historiales.find(x=>x.mascotaId===id);if(!h){h={id:nextId(db.historiales),mascotaId:id,observaciones:''};db.historiales.push(h);}return h;}
function setPetState(db:DB,id:number,state:string){
 const p=find(db.mascotas,id,'Mascota');const next=choose(state,PET_STATES,'Estado');
 ensure(p.estado!=='Adoptada','No se puede modificar el estado de una mascota adoptada.');
 ensure(next!=='Adoptada'&&next!=='Reservada','El estado Reservada se asigna al aprobar una solicitud y Adoptada al concretar la adopción.');
 ensure(!db.solicitudes.some(s=>s.mascotaId===id&&s.estado==='Aprobada'),'La mascota tiene una solicitud aprobada; revisala antes de modificar su estado.');
 if(next==='DisponibleAdopcion')ensurePetCanBeAvailable(db,id);
 p.estado=next as PetState;
}
function setNoticeState(db:DB,id:number,state:string){
 const n=find(db.avisos,id,'Aviso');const next=choose(state,AVISO_STATES,'Estado');
 const allowed:Record<string,string[]>={Reportado:['EnEvaluacion','Aceptado','Descartado'],EnEvaluacion:['Aceptado','Descartado','Reportado'],Aceptado:['EnEvaluacion','Descartado'],Atendido:[],Descartado:[]};
 ensure(n.estado===next||allowed[n.estado]?.includes(next),'Transición inválida del aviso. Para marcarlo atendido, ingresá la mascota desde el aviso aceptado.');n.estado=next as NoticeState;
}
function setTreatmentState(db:DB,id:number,state:string){
 const t=find(db.tratamientos,id,'Tratamiento');const next=choose(state,TR_STATES,'Estado');const p=find(db.mascotas,t.mascotaId,'Mascota');
 ensure(p.estado!=='Adoptada'||!['Pendiente','EnCurso'].includes(next),'No se puede iniciar un tratamiento nuevo sobre una mascota ya adoptada.');
 t.estado=next as TreatmentState;
 t.fechaFin=next==='Finalizado'?today():null;
 if(['Pendiente','EnCurso'].includes(next)&&p.estado==='DisponibleAdopcion')p.estado='EnTratamiento';
}
function setRequestState(db:DB,id:number,state:string){
 const s=find(db.solicitudes,id,'Solicitud'); const next=choose(state,REQ_STATES,'Estado');const p=find(db.mascotas,s.mascotaId,'Mascota');
 ensure(!adoptionFor(db,s.mascotaId),'La adopción ya se concretó; la solicitud forma parte del historial.');
 if(next===s.estado)return;
 if(next==='Aprobada'){
   ensure(['Pendiente','EnEvaluacion','Aprobada'].includes(s.estado),'No se puede aprobar una solicitud rechazada o cancelada.');
   ensure((p.estado==='DisponibleAdopcion'||(s.estado==='Aprobada'&&p.estado==='Reservada'))&&!isBlocking(db,p.id),'La mascota no está disponible o tiene tratamientos incompatibles.');
   ensure(!db.solicitudes.some(r=>r.id!==id&&r.mascotaId===p.id&&r.estado==='Aprobada'),'Ya existe una solicitud aprobada para esta mascota.');
   p.estado='Reservada';
 }else if(s.estado==='Aprobada'){
   ensure(['Cancelada','Rechazada'].includes(next),'Una solicitud aprobada solo puede cancelarse o rechazarse antes de la adopción.');
   if(p.estado==='Reservada')p.estado=isBlocking(db,p.id)?'EnTratamiento':'DisponibleAdopcion';
 }else if(['Rechazada','Cancelada'].includes(s.estado)){
   ensure(next===s.estado,'No se puede reabrir una solicitud cerrada.');
 }else{
   ensure(['Pendiente','EnEvaluacion','Rechazada','Cancelada'].includes(next),'Estado de solicitud inválido.');
 }
 s.estado=next as RequestState;
}
function doSave(db:DB,collection:CollectionName,v:Record<string,unknown>){
 const id=Number(v.id)||0;
 if(collection==='mascotas'){
   const existing=id?find(db.mascotas,id,'Mascota'):null;
   if(!existing)ensure(occupied(db)<db.refugio.capacidadMaxima,'El refugio no tiene plazas disponibles.');
   if(existing)ensure(existing.estado!=='Adoptada','No se pueden modificar los datos de una mascota ya adoptada.');
   const caregiverId=text(v.cuidadorId)?number(v.cuidadorId):null;
   checkCapacity(db,caregiverId,id||undefined);
   const p:Mascota={id:id||nextId(db.mascotas),nombre:checkName(v.nombre,'Nombre'),especie:checkName(v.especie,'Especie'),raza:text(v.raza),edadAproximada:positive(v.edadAproximada,'edad',true),sexo:choose(v.sexo,['Macho','Hembra'],'Sexo'),tamano:choose(v.tamano,['Pequenio','Mediano','Grande'],'Tamaño'),estado:existing?.estado||'Ingresada',cuidadorId:caregiverId,fechaIngreso:existing?.fechaIngreso||today(),foto:text(v.foto)||existing?.foto||'pet-maple.jpg',avisoRescateOrigenId:existing?.avisoRescateOrigenId??null};
   if(existing)Object.assign(existing,p);else db.mascotas.push(p);
   return;
 }
 if(collection==='avisos'){
   const existing=id?find(db.avisos,id,'Aviso'):null;
   ensure(!existing||!['Atendido','Descartado'].includes(existing.estado),'No se edita un aviso cerrado.');
   const n:Aviso={id:id||nextId(db.avisos),descripcion:checkName(v.descripcion,'Descripción'),tipoAnimal:choose(v.tipoAnimal,['Perro','Gato','Ave','Otro'],'Tipo de animal'),latitud:validCoord(v.latitud,-90,90,'Latitud'),longitud:validCoord(v.longitud,-180,180,'Longitud'),contactoNombre:checkName(v.contactoNombre,'Contacto'),contactoTelefono:text(v.contactoTelefono),estadoAparente:choose(v.estadoAparente,['Sano','Herido','Enfermo','Desnutrido','Critico'],'Estado aparente'),nivelUrgencia:choose(v.nivelUrgencia,['Baja','Media','Alta'],'Urgencia'),estado:existing?.estado||'Reportado',fechaAviso:existing?.fechaAviso||today(),mascotaGeneradaId:existing?.mascotaGeneradaId??null};
   if(existing)Object.assign(existing,n);else db.avisos.push(n);return;
 }
 if(collection==='cuidadores'){
   const max=positive(v.capacidadMaxima,'capacidad máxima');ensure(Number.isInteger(max),'La capacidad máxima debe ser entera.');
   const used=id?caregiverLoad(db,id):0;ensure(max>=used,`El cuidador tiene ${used} mascotas asignadas; no podés reducir su capacidad por debajo de esa cantidad.`);
   const c:Cuidador={id:id||nextId(db.cuidadores),nombre:checkName(v.nombre,'Nombre'),especializacion:text(v.especializacion),capacidadMaxima:max};
   if(id)Object.assign(find(db.cuidadores,id,'Cuidador'),c);else db.cuidadores.push(c);return;
 }
 if(collection==='historiales'){
   const pid=number(v.mascotaId);find(db.mascotas,pid,'Mascota');
   const existing=id?find(db.historiales,id,'Historial'):db.historiales.find(h=>h.mascotaId===pid);
   ensure(!existing||existing.mascotaId===pid,'No se puede cambiar la mascota de un historial.');
   if(existing){existing.observaciones=text(v.observaciones);return;}
   db.historiales.push({id:nextId(db.historiales),mascotaId:pid,observaciones:text(v.observaciones)});return;
 }
 if(collection==='tratamientos'){
   const existing=id?find(db.tratamientos,id,'Tratamiento'):null;
   const pid=number(v.mascotaId);const p=find(db.mascotas,pid,'Mascota');
   ensure(p.estado!=='Adoptada'||!!existing,'No se puede registrar un tratamiento nuevo sobre una mascota adoptada.');
   ensure(!existing||existing.mascotaId===pid,'No se permite transferir tratamientos entre mascotas.');
   const h=ensureHistory(db,pid);
   const status=existing?.estado||'Pendiente';
   const t:Tratamiento={id:id||nextId(db.tratamientos),mascotaId:pid,historialSanitarioId:h.id,tipo:choose(v.tipo,['Medicacion','Vacunacion','Curacion','Desparasitacion','RecuperacionPosquirurgica','ControlVeterinario'],'Tipo de tratamiento'),descripcion:checkName(v.descripcion,'Descripción'),estado:status,fechaInicio:text(v.fechaInicio)||existing?.fechaInicio||today(),fechaFin:existing?.fechaFin||null};
   if(existing)Object.assign(existing,t);else db.tratamientos.push(t);
   if(['Pendiente','EnCurso'].includes(t.estado)&&p.estado==='DisponibleAdopcion')p.estado='EnTratamiento';return;
 }
 if(collection==='adoptantes'){
   const dni=text(v.dni).replace(/[^0-9]/g,'');ensure(dni.length>=7&&dni.length<=9,'Ingresá un DNI válido (7 a 9 dígitos).');
   ensure(!db.adoptantes.some(a=>a.id!==id&&a.dni===dni),'Ya existe una persona adoptante con ese DNI.');
   const a:Adoptante={id:id||nextId(db.adoptantes),nombre:checkName(v.nombre,'Nombre'),dni,telefono:text(v.telefono),direccion:text(v.direccion)};
   if(id)Object.assign(find(db.adoptantes,id,'Adoptante'),a);else db.adoptantes.push(a);return;
 }
 if(collection==='solicitudes'){
   ensure(!id,'Las solicitudes se modifican mediante su estado, no editando su relación.');
   const mascotaId=number(v.mascotaId),adoptanteId=number(v.adoptanteId);find(db.adoptantes,adoptanteId,'Adoptante');
   ensure(canRequest(db,mascotaId),'La mascota no está disponible para nuevas solicitudes.');
   ensure(!db.solicitudes.some(r=>r.mascotaId===mascotaId&&r.adoptanteId===adoptanteId&&['Pendiente','EnEvaluacion','Aprobada'].includes(r.estado)),'La persona ya tiene una solicitud activa para esta mascota.');
   db.solicitudes.push({id:nextId(db.solicitudes),mascotaId,adoptanteId,fechaSolicitud:today(),estado:'Pendiente'});return;
 }
}
function doDelete(db:DB,collection:CollectionName,id:number){
 if(collection==='mascotas'){
   ensure(!db.historiales.some(x=>x.mascotaId===id)&&!db.tratamientos.some(x=>x.mascotaId===id)&&!db.solicitudes.some(x=>x.mascotaId===id)&&!db.avisos.some(x=>x.mascotaGeneradaId===id),'La mascota tiene historial o relaciones: no se puede eliminar.');
   ensure(find(db.mascotas,id,'Mascota').estado!=='Adoptada','Las mascotas adoptadas se conservan en el historial.');
 }else if(collection==='cuidadores')ensure(caregiverLoad(db,id)===0,'El cuidador tiene mascotas asignadas. Reasignalas antes de eliminar.');
 else if(collection==='adoptantes')ensure(!db.solicitudes.some(x=>x.adoptanteId===id),'El adoptante tiene solicitudes relacionadas.');
 else if(collection==='avisos')ensure(!find(db.avisos,id,'Aviso').mascotaGeneradaId,'El aviso generó una mascota y debe conservarse.');
 else if(collection==='historiales')ensure(!db.tratamientos.some(t=>t.historialSanitarioId===id)&&find(db.mascotas,find(db.historiales,id,'Historial').mascotaId).estado!=='Adoptada','El historial tiene tratamientos o corresponde a una mascota adoptada.');
 else if(collection==='solicitudes')ensure(!db.adopciones.some(a=>a.solicitudAdopcionId===id)&&!['Aprobada'].includes(find(db.solicitudes,id,'Solicitud').estado),'La solicitud está aprobada o vinculada a una adopción.');
 else if(collection==='tratamientos')ensure(!['Pendiente','EnCurso'].includes(find(db.tratamientos,id,'Tratamiento').estado),'Primero finalizá o suspendé el tratamiento.');
 const records=db[collection] as {id:number}[];const index=records.findIndex(x=>x.id===id);ensure(index!==-1,'No se encontró el registro.');records.splice(index,1);
}
export function execute(source:DB,cmd:Command):DB{
 const db:DB=structuredClone(source);
 switch(cmd.type){
 case 'save':doSave(db,cmd.collection,cmd.values);break;
 case 'delete':doDelete(db,cmd.collection,cmd.id);break;
 case 'state':
   if(cmd.collection==='mascotas')setPetState(db,cmd.id,cmd.state);
   else if(cmd.collection==='avisos')setNoticeState(db,cmd.id,cmd.state);
   else if(cmd.collection==='tratamientos')setTreatmentState(db,cmd.id,cmd.state);
   else setRequestState(db,cmd.id,cmd.state);
   break;
 case 'assign':{
   const p=find(db.mascotas,cmd.id,'Mascota');ensure(p.estado!=='Adoptada','No se puede asignar un cuidador a una mascota adoptada.');
   checkCapacity(db,cmd.caregiverId,p.id);p.cuidadorId=cmd.caregiverId;break;
 }
 case 'intake':{
   const n=find(db.avisos,cmd.noticeId,'Aviso');ensure(n.estado==='Aceptado','El aviso debe estar aceptado antes del ingreso.');
   ensure(!n.mascotaGeneradaId,'Este aviso ya generó una mascota.');
   ensure(occupied(db)<db.refugio.capacidadMaxima,'El refugio está completo.');
   doSave(db,'mascotas',{...cmd.values,especie:n.tipoAnimal,cuidadorId:cmd.values.cuidadorId||'',foto:cmd.values.foto||'pet-maple.jpg'});
   const p=db.mascotas[db.mascotas.length-1];p.avisoRescateOrigenId=n.id;n.mascotaGeneradaId=p.id;n.estado='Atendido';break;
 }
 case 'adopt':{
   const s=find(db.solicitudes,cmd.requestId,'Solicitud');const p=find(db.mascotas,s.mascotaId,'Mascota');
   ensure(s.estado==='Aprobada','La solicitud debe aprobarse antes de concretar la adopción.');
   ensure(p.estado==='Reservada','La mascota debe encontrarse reservada.');
   ensure(!isBlocking(db,p.id),'No se puede adoptar con tratamientos activos o pendientes.');
   ensure(!adoptionFor(db,p.id),'Ya existe una adopción definitiva para esta mascota.');
   ensureHistory(db,p.id);
   db.adopciones.push({id:nextId(db.adopciones),solicitudAdopcionId:s.id,fechaAdopcion:today(),observaciones:text(cmd.observations)});
   p.estado='Adoptada';p.cuidadorId=null;
   db.solicitudes.filter(r=>r.mascotaId===p.id&&r.id!==s.id&&['Pendiente','EnEvaluacion'].includes(r.estado)).forEach(r=>r.estado='Rechazada');
   break;
 }
 case 'publicRequest':{
   const p=find(db.mascotas,cmd.petId,'Mascota');ensure(canRequest(db,p.id),'La mascota no está disponible para nuevas solicitudes.');
   const dni=text(cmd.person.dni).replace(/[^0-9]/g,'');
   let person=db.adoptantes.find(a=>a.dni===dni);
   if(!person){doSave(db,'adoptantes',cmd.person);person=db.adoptantes[db.adoptantes.length-1];}
   ensure(!db.solicitudes.some(r=>r.mascotaId===p.id&&r.adoptanteId===person!.id&&['Pendiente','EnEvaluacion','Aprobada'].includes(r.estado)),'Ya existe una solicitud activa con este DNI para la mascota.');
   doSave(db,'solicitudes',{mascotaId:p.id,adoptanteId:person.id});break;
 }
 case 'refugio':{
   const cap=positive(cmd.capacity,'capacidad máxima');ensure(Number.isInteger(cap),'La capacidad debe ser entera.');
   ensure(cap>=occupied(db),`El refugio tiene ${occupied(db)} plazas ocupadas. No podés reducir la capacidad por debajo de ese número.`);
   db.refugio={nombre:checkName(cmd.name,'Nombre del refugio'),capacidadMaxima:cap};break;
 }
 }
 return db;
}
