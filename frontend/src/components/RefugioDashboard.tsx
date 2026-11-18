import type { ReactNode } from 'react';
import { Icon } from './Icon';
import { Badge } from './UI';
import { dateLabel, display, photo, type DB } from '../model/types';
import { canRequest, occupancy, pendingRequests, personById, petById } from '../model/rules';

export type DashboardTarget = 'mascotas' | 'avisos' | 'solicitudes' | 'tratamientos' | 'adopciones';
export interface LocalTask { id: string; title: string; date: string; time: string; category: string; notes: string }
interface Props { db: DB; tasks: LocalTask[]; onNavigate: (target: DashboardTarget) => void; onNewTask: () => void; onNewIntake: () => void }

type Priority = { id: string; title: string; detail: string; status: string; color: 'urgent'|'neutral'|'soft'; image?: string; target: DashboardTarget };

const dayISO = (d: Date) => `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;
export function DashboardDate() {
 const date = new Date();
 return <>{new Intl.DateTimeFormat('es-AR',{weekday:'long',day:'numeric',month:'long',year:'numeric'}).format(date)}</>;
}
export function DashboardGreeting() {
 const date = new Date(); const weekday = new Intl.DateTimeFormat('es-AR',{weekday:'long'}).format(date);
 return <>Buen {weekday}, equipo</>;
}
function DashboardCard({children,title,subtitle,extra,className=''}:{children:ReactNode;title:string;subtitle?:string;extra?:ReactNode;className?:string}) {
 return <section className={`nido-card ${className}`}><header className="nido-card-head"><div><h2>{title}</h2>{subtitle&&<p>{subtitle}</p>}</div>{extra}</header>{children}</section>;
}
function LabelPill({text,color='neutral'}:{text:string;color?:Priority['color']}) {
 return <span className={`nido-pill nido-pill-${color}`}>{text}</span>;
}
export function RefugioDashboard({db,tasks,onNavigate,onNewTask,onNewIntake}:Props) {
 const occupied = occupancy(db);
 const available = db.mascotas.filter(m=>canRequest(db,m.id)).length;
 const pending = pendingRequests(db);
 const today = dayISO(new Date());
 const agenda = tasks.filter(t=>t.date===today).sort((a,b)=>a.time.localeCompare(b.time));
 const activeTreatments = db.tratamientos.filter(t=>t.estado==='Pendiente'||t.estado==='EnCurso');
 const activeNotices = db.avisos.filter(a=>a.estado!=='Descartado'&&a.estado!=='Atendido');
 const priorities:Priority[] = [
   ...activeNotices.filter(n=>n.nivelUrgencia==='Alta').map(n=>({id:`aviso-${n.id}`,title:`Aviso #${n.id} · ${n.tipoAnimal}`,detail:`Rescate · ${n.descripcion}`,status:'Urgente',color:'urgent' as const,target:'avisos' as const})),
   ...activeTreatments.map(t=>({id:`trat-${t.id}`,title:`${petById(db,t.mascotaId)?.nombre||'Mascota'} · ${display(t.tipo)}`,detail:`Salud · ${t.descripcion}`,status:display(t.estado),color:t.estado==='Pendiente'?'urgent' as const:'neutral' as const,image:photo(petById(db,t.mascotaId)),target:'tratamientos' as const})),
   ...pending.map(s=>({id:`sol-${s.id}`,title:`${petById(db,s.mascotaId)?.nombre||'Mascota'} · Revisar solicitud`,detail:`Solicitud #${s.id} · ${personById(db,s.adoptanteId)?.nombre||'Adoptante'}`,status:display(s.estado),color:'neutral' as const,image:photo(petById(db,s.mascotaId)),target:'solicitudes' as const})),
   ...activeNotices.filter(n=>n.nivelUrgencia!=='Alta').map(n=>({id:`aviso-${n.id}`,title:`Aviso #${n.id} · ${n.tipoAnimal}`,detail:`Rescate · ${n.descripcion}`,status:display(n.estado),color:'soft' as const,target:'avisos' as const}))
 ].slice(0,4);
 const recentRequests = [...db.solicitudes].sort((a,b)=>b.fechaSolicitud.localeCompare(a.fechaSolicitud)).slice(0,4);
 const recentPets = [...db.mascotas].sort((a,b)=>b.fechaIngreso.localeCompare(a.fechaIngreso)).slice(0,2);
 const recentAdoptions = [...db.adopciones].sort((a,b)=>b.fechaAdopcion.localeCompare(a.fechaAdopcion)).slice(0,1);
 const recentActivity = [
   ...recentPets.map(m=>({id:`p-${m.id}`,title:`${m.nombre} ingresó al refugio`,date:m.fechaIngreso,icon:'arrow',target:'mascotas' as const})),
   ...recentAdoptions.map(a=>({id:`a-${a.id}`,title:`Se concretó la adopción de ${petById(db,db.solicitudes.find(s=>s.id===a.solicitudAdopcionId)?.mascotaId||-1)?.nombre||'una mascota'}`,date:a.fechaAdopcion,icon:'check',target:'adopciones' as const}))
 ].sort((a,b)=>b.date.localeCompare(a.date)).slice(0,3);
 const capacity = Math.max(1,db.refugio.capacidadMaxima);
 return <div className="nido-dashboard">
  <div className="nido-welcome">
   <div><h1><DashboardGreeting/></h1><p><DashboardDate/> <span aria-hidden="true">·</span> Estas son las novedades del refugio.</p></div>
   <div className="nido-welcome-actions"><button className="nido-primary-btn" onClick={onNewIntake} type="button"><Icon name="plus" size={21}/> Registrar ingreso</button><button className="nido-outline-btn" onClick={onNewTask} type="button">Nueva tarea</button></div>
  </div>
  <div className="nido-kpis" aria-label="Indicadores del refugio">
   <div className="nido-kpi"><span>EN CUIDADO</span><strong>{occupied}</strong><small>Mascotas actualmente en el refugio</small></div>
   <div className="nido-kpi"><span>LISTOS PARA ADOPCIÓN</span><strong>{available}</strong><small>Disponibles y sin tratamientos bloqueantes</small></div>
   <div className="nido-kpi"><span>SOLICITUDES SIN REVISAR</span><strong className="nido-accent-number">{pending.length}</strong><small>Pendientes o en evaluación</small></div>
   <div className="nido-kpi"><span>TAREAS DE HOY</span><strong>{agenda.length}</strong><small>Agenda local del panel de prueba</small></div>
  </div>
  <div className="nido-dashboard-grid">
    <div className="nido-dashboard-left">
     <DashboardCard title="Prioridades de seguimiento" subtitle={`${priorities.length} asuntos para revisar`} extra={<button className="nido-text-action" type="button" onClick={()=>onNavigate('tratamientos')}>Ver tratamientos <Icon name="arrow" size={17}/></button>}>
      <div className="nido-priority-list">{priorities.length?priorities.map(item=><div className="nido-priority" key={item.id}>
       <div className="nido-priority-avatar">{item.image?<img src={item.image} alt=""/>:<Icon name="pin" size={24}/>}</div>
       <div className="nido-priority-copy"><strong>{item.title}</strong><small>{item.detail}</small></div>
       <LabelPill text={item.status} color={item.color}/>
       <button type="button" className="nido-small-btn" onClick={()=>onNavigate(item.target)} aria-label={`Abrir ${item.title}`}>Abrir</button>
      </div>):<div className="nido-empty">No hay tareas prioritarias pendientes.</div>}</div>
     </DashboardCard>
     <DashboardCard title="Solicitudes de adopción" className="nido-requests-card" extra={<button type="button" className="nido-text-action" onClick={()=>onNavigate('solicitudes')}>Ver todas <Icon name="arrow" size={17}/></button>}>
      {recentRequests.length?<div className="nido-table-scroll"><table className="nido-table"><thead><tr><th>Solicitante</th><th>Mascota</th><th>Estado</th><th>Recibida</th><th>Acción</th></tr></thead><tbody>
       {recentRequests.map(s=>{const pet=petById(db,s.mascotaId);return <tr key={s.id}><td>{personById(db,s.adoptanteId)?.nombre||'—'}</td><td><span className="nido-pet-cell">{pet&&<img src={photo(pet)} alt=""/>}{pet?.nombre||'—'}</span></td><td><Badge status={s.estado}/></td><td>{dateLabel(s.fechaSolicitud)}</td><td><button type="button" className="nido-small-btn" onClick={()=>onNavigate('solicitudes')}>Revisar</button></td></tr>})}
      </tbody></table></div>:<div className="nido-empty">Todavía no hay solicitudes registradas.</div>}
     </DashboardCard>
    </div>
    <div className="nido-dashboard-right">
     <DashboardCard title="Agenda de hoy" subtitle={new Intl.DateTimeFormat('es-AR',{weekday:'long',day:'numeric'}).format(new Date())} extra={<button type="button" className="nido-icon-link" onClick={onNewTask} aria-label="Añadir tarea a la agenda"><Icon name="plus" size={19}/></button>}>
       {agenda.length?<div className="nido-agenda-list">{agenda.map((task,i)=><div className="nido-agenda-item" key={task.id}><div className={`nido-agenda-dot ${i===0?'first':''}`}/><strong>{task.time||'—'}</strong><span>{task.title}<small>{task.category}{task.notes?` · ${task.notes}`:''}</small></span></div>)}</div>:<div className="nido-empty agenda-empty"><Icon name="calendar" size={26}/><span>No hay turnos programados para hoy.</span><button className="nido-inline-link" onClick={onNewTask} type="button">Agregar tarea</button></div>}
     </DashboardCard>
     <DashboardCard title="Ocupación del refugio" className="nido-capacity-card">
      <div className="nido-capacity-numbers"><strong>{occupied} de {db.refugio.capacidadMaxima} espacios</strong><span>{Math.max(0,db.refugio.capacidadMaxima-occupied)} libres</span></div>
      <div className="nido-capacity-track" role="progressbar" aria-valuenow={occupied} aria-valuemin={0} aria-valuemax={capacity} aria-label="Ocupación del refugio"><span style={{width:`${Math.min(100,Math.round(occupied/capacity*100))}%`}}/></div>
     </DashboardCard>
     <DashboardCard title="Movimiento reciente">
      <div className="nido-activity-list">{recentActivity.map(item=><button className="nido-activity" type="button" key={item.id} onClick={()=>onNavigate(item.target)}><span className="nido-activity-icon"><Icon name={item.icon} size={20}/></span><span><strong>{item.title}</strong><small>{dateLabel(item.date)}</small></span></button>)}</div>
     </DashboardCard>
    </div>
  </div>
 </div>;
}
