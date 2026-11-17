import { useState, type ReactNode } from 'react';
import { useRefugio } from '../store/RefugioContext';
import { Icon } from '../components/Icon';
import { Badge, Confirm, Empty, EntityName, EntityTable, EntryForm, Modal, TableActions, Toast, type FieldSpec } from '../components/UI';
import { ANIMALS, APPARENT, AVISO_STATES, PET_STATES, REQ_STATES, SEXES, SIZES, TR_STATES, T_TYPES, URGENCIES, dateLabel, display, photo, today, type DB, type Mascota } from '../model/types';
import { adoptionFor, canRequest, caregiverLoad, isBlocking, occupancy, pendingRequests, personById, petById, requestById, type CollectionName } from '../model/rules';

type View = 'resumen'|'mascotas'|'avisos'|'cuidadores'|'tratamientos'|'historiales'|'adoptantes'|'solicitudes'|'adopciones'|'refugio';
type Popup = {kind:'edit';collection:CollectionName;data?:Record<string,unknown>}|{kind:'state';collection:'mascotas'|'avisos'|'tratamientos'|'solicitudes';id:number;current:string}|{kind:'assign';id:number}|{kind:'intake';noticeId:number}|{kind:'adopt';requestId?:number}|{kind:'refugio'}|{kind:'detail';title:string;data:Record<string,unknown>;kindName?:string}|{kind:'delete';collection:CollectionName;id:number;label:string}|{kind:'reset'};
const META:Record<View,{title:string;desc:string;icon:string;section:string}>={
 resumen:{title:'Resumen general',desc:'Actividad del refugio y tareas prioritarias.',icon:'grid',section:'GENERAL'},
 mascotas:{title:'Mascotas',desc:'Registro, estado, disponibilidad y cuidadores responsables.',icon:'heart',section:'GENERAL'},
 avisos:{title:'Avisos de rescate',desc:'Recepción, seguimiento e ingreso de animales.',icon:'pin',section:'GENERAL'},
 cuidadores:{title:'Cuidadores',desc:'Personal responsable y capacidad de atención.',icon:'users',section:'OPERACIONES'},
 tratamientos:{title:'Tratamientos',desc:'Controles veterinarios y seguimiento de procedimientos.',icon:'briefcase',section:'OPERACIONES'},
 historiales:{title:'Historial sanitario',desc:'Observaciones y antecedentes de cada mascota.',icon:'clipboard',section:'OPERACIONES'},
 adoptantes:{title:'Adoptantes',desc:'Personas registradas e información de contacto.',icon:'users',section:'ADOPCIONES'},
 solicitudes:{title:'Solicitudes de adopción',desc:'Evaluación, aprobación y cierre de solicitudes.',icon:'file',section:'ADOPCIONES'},
 adopciones:{title:'Adopciones',desc:'Adopciones concretadas e historial de operaciones.',icon:'shield',section:'ADOPCIONES'},
 refugio:{title:'Configuración del refugio',desc:'Datos del establecimiento y capacidad disponible.',icon:'building',section:'CONFIGURACIÓN'}
};
const NAV_GROUPS:View[][]=[['resumen','mascotas','avisos'],['cuidadores','tratamientos','historiales'],['adoptantes','solicitudes','adopciones'],['refugio']];
const BUTTONS:Partial<Record<View,string>>={mascotas:'Registrar mascota',avisos:'Registrar aviso',cuidadores:'Nuevo cuidador',tratamientos:'Nuevo tratamiento',historiales:'Crear historial',adoptantes:'Nuevo adoptante',solicitudes:'Nueva solicitud',adopciones:'Concretar adopción',refugio:'Editar refugio'};
const IMG_FILES=['pet-maple.jpg','pet-luna.jpg','pet-otis.jpg','pet-olive.jpg','pet-biscuit.jpg','pet-mochi.jpg','pet-chip.jpg','pet-walter.jpg','pet-saffron.jpg','pet-pepper.jpg','pet-nova.jpg','pet-juniper.jpg'];
const opt=(items:{id:number;nombre:string}[])=>items.map(x=>({value:String(x.id),label:`${x.nombre} · #${x.id}`}));
const petOpts=(db:DB)=>opt(db.mascotas);
function editFields(view:CollectionName,db:DB,edit?:Record<string,unknown>):FieldSpec[]{
 const pets=petOpts(db);const caretakers=opt(db.cuidadores);const adoptants=opt(db.adoptantes); const existing=!!edit?.id;
 switch(view){
 case 'mascotas':return [
  {name:'nombre',label:'Nombre'},{name:'especie',label:'Especie',type:'select',options:ANIMALS},{name:'raza',label:'Raza',required:false},
  {name:'edadAproximada',label:'Edad aproximada (años)',type:'number',min:0},{name:'sexo',label:'Sexo',type:'select',options:SEXES},
  {name:'tamano',label:'Tamaño',type:'select',options:SIZES},
  {name:'cuidadorId',label:'Cuidador responsable',type:'select',options:[{value:'',label:'Sin asignar'},...caretakers],required:false},
  {name:'foto',label:'Fotografía de referencia',type:'select',options:IMG_FILES.map(f=>({value:f,label:f.replace('.jpg','').replace('pet-','')})),required:false}
 ];
 case 'avisos':return [
  {name:'descripcion',label:'Descripción del aviso',type:'textarea',full:true},{name:'tipoAnimal',label:'Animal observado',type:'select',options:ANIMALS},
  {name:'contactoNombre',label:'Nombre de contacto'},{name:'contactoTelefono',label:'Teléfono',type:'tel',required:false},
  {name:'latitud',label:'Latitud',type:'number',step:'any',min:-90,max:90},{name:'longitud',label:'Longitud',type:'number',step:'any',min:-180,max:180},
  {name:'estadoAparente',label:'Estado aparente',type:'select',options:APPARENT},{name:'nivelUrgencia',label:'Urgencia',type:'select',options:URGENCIES}
 ];
 case 'cuidadores':return [{name:'nombre',label:'Nombre y apellido'},{name:'especializacion',label:'Especialización',required:false},{name:'capacidadMaxima',label:'Capacidad máxima',type:'number',min:1}];
 case 'tratamientos':return [
  {name:'mascotaId',label:'Mascota',type:'select',options:pets,disabled:existing},
  {name:'tipo',label:'Tipo de tratamiento',type:'select',options:T_TYPES},{name:'fechaInicio',label:'Fecha de inicio',type:'date',required:false},
  {name:'descripcion',label:'Descripción',type:'textarea',full:true}
 ];
 case 'historiales':return [{name:'mascotaId',label:'Mascota',type:'select',options:pets,disabled:existing},{name:'observaciones',label:'Observaciones sanitarias',type:'textarea',full:true,required:false}];
 case 'adoptantes':return [{name:'nombre',label:'Nombre completo'},{name:'dni',label:'DNI'},{name:'telefono',label:'Teléfono',type:'tel',required:false},{name:'direccion',label:'Dirección',required:false}];
 case 'solicitudes':return [
  {name:'mascotaId',label:'Mascota disponible',type:'select',options:opt(db.mascotas.filter(m=>canRequest(db,m.id)))},
  {name:'adoptanteId',label:'Adoptante',type:'select',options:adoptants}
 ];
 }
 return [];
}
const LABEL_PROPS:Record<string,string>={id:'Identificador',nombre:'Nombre',especie:'Especie',raza:'Raza',edadAproximada:'Edad aproximada',sexo:'Sexo',tamano:'Tamaño',estado:'Estado',cuidadorId:'Cuidador',fechaIngreso:'Fecha de ingreso',descripcion:'Descripción',tipoAnimal:'Tipo de animal',contactoNombre:'Contacto',contactoTelefono:'Teléfono de contacto',latitud:'Latitud',longitud:'Longitud',estadoAparente:'Estado aparente',nivelUrgencia:'Urgencia',fechaAviso:'Fecha del aviso',mascotaGeneradaId:'Mascota generada',especializacion:'Especialización',capacidadMaxima:'Capacidad máxima',mascotaId:'Mascota',tipo:'Tipo',fechaInicio:'Fecha de inicio',fechaFin:'Fecha de finalización',observaciones:'Observaciones',dni:'DNI',telefono:'Teléfono',direccion:'Dirección',adoptanteId:'Adoptante',fechaSolicitud:'Fecha de solicitud',solicitudAdopcionId:'Solicitud de origen',fechaAdopcion:'Fecha de adopción',avisoRescateOrigenId:'Aviso de origen'};
function Section({title,children,subtitle}:{title:string;children:ReactNode;subtitle?:string}){return <section className="content-card"><div className="card-header-pt"><div><h2>{title}</h2>{subtitle&&<p>{subtitle}</p>}</div></div><div className="card-body-pt">{children}</div></section>}
function Stat({title,count,icon,tone,description}:{title:string;count:number;icon:string;tone?:string;description:string}){return <div className="stat-card"><div className="stat-top"><span className="stat-caption">{title}</span><span className={`stat-icon ${tone||''}`}><Icon name={icon}/></span></div><div className="stat-number">{count}</div><div className="stat-foot">{description}</div></div>}
export function AdminApp(){
 const {db,run,reset}=useRefugio();const [view,setView]=useState<View>('resumen');const [query,setQuery]=useState('');const [stateFilter,setStateFilter]=useState('Todos');const [popup,setPopup]=useState<Popup|null>(null);const [mobile,setMobile]=useState(false);
 const occupied=occupancy(db),available=db.mascotas.filter(p=>canRequest(db,p.id)).length,pending=pendingRequests(db);
 const navigate=(to:View)=>{setView(to);setQuery('');setStateFilter('Todos');setMobile(false);window.scrollTo({top:0,behavior:'smooth'});};
 const openCreate=()=>{if(view==='resumen')return;if(view==='refugio')setPopup({kind:'refugio'});else if(view==='adopciones')setPopup({kind:'adopt'});else setPopup({kind:'edit',collection:view as CollectionName});};
 const action=(command:Parameters<typeof run>[0],message?:string)=>{const ok=run(command,message);if(ok)setPopup(null);return ok;};
 const record=(c:CollectionName,id:number)=>((db[c] as {id:number}[]).find(x=>x.id===id) as unknown as Record<string,unknown>);
 const detail=(title:string, data:Record<string,unknown>,kindName='')=>setPopup({kind:'detail',title,data,kindName});
 const edit=(collection:CollectionName,id:number)=>setPopup({kind:'edit',collection,data:record(collection,id)});
 const del=(collection:CollectionName,id:number,label:string)=>setPopup({kind:'delete',collection,id,label});
 const change=(collection:'mascotas'|'avisos'|'tratamientos'|'solicitudes',id:number,current:string)=>setPopup({kind:'state',collection,id,current});
 const actions=(collection:CollectionName,id:number,label:string,includeEdit=true)=>[
  {label:'Ver',icon:'eye',onClick:()=>detail(label,record(collection,id),collection)},
  ...(includeEdit?[{label:'Editar',icon:'edit',onClick:()=>edit(collection,id)}]:[]),
  {label:'Eliminar',icon:'trash',danger:true,onClick:()=>del(collection,id,label)}
 ];
 const filters:Record<string,readonly string[]>={mascotas:PET_STATES,avisos:AVISO_STATES,tratamientos:TR_STATES,solicitudes:REQ_STATES};
 const listing=<T extends object>(xs:T[])=>xs.filter(x=>{
  const matchesText=JSON.stringify(x).normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase().includes(query.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase());
  return matchesText&&(stateFilter==='Todos'||(x as Record<string,unknown>).estado===stateFilter);
 });
 const table=(headers:string[],rows:ReactNode[][])=><section className="content-card"><div className="table-toolbar"><span className="table-count">{rows.length} {rows.length===1?'registro':'registros'} · {META[view].title}</span><div className="toolbar-inputs"><div className="search-admin"><Icon name="search" size={16}/><input value={query} onChange={e=>setQuery(e.target.value)} className="pt-control" aria-label="Buscar registros" placeholder="Buscar registros…"/></div>{filters[view]&&<select className="pt-control" aria-label="Filtrar por estado" value={stateFilter} onChange={e=>setStateFilter(e.target.value)}><option value="Todos">Todos los estados</option>{filters[view].map(s=><option value={s} key={s}>{display(s)}</option>)}</select>}</div></div><EntityTable headers={headers} rows={rows}/></section>;
 const statusButton=(collection:'mascotas'|'avisos'|'tratamientos'|'solicitudes',id:number,current:string)=>({label:'Estado',icon:'refresh',onClick:()=>change(collection,id,current)});
 function currentPage():ReactNode{
 switch(view){
 case 'resumen':return <>
   <div className="kpi-grid"><Stat title="Mascotas registradas" count={db.mascotas.length} icon="heart" description="Incluye mascotas adoptadas"/><Stat title="Disponibles para adoptar" count={available} icon="shield" tone="blue" description="Habilitadas para solicitar"/><Stat title="Solicitudes por revisar" count={pending.length} icon="file" tone="orange" description="Pendientes o en evaluación"/><Stat title="Adopciones realizadas" count={db.adopciones.length} icon="check" tone="pink" description="Se conserva el historial"/></div>
   <div className="summary-grid"><div className="panel-hero"><div><small>GESTIÓN INTEGRAL</small><h2>Un lugar para cada etapa del cuidado.</h2><p>Coordiná ingresos, tratamientos, cuidadores y adopciones desde un mismo panel.</p><button type="button" onClick={()=>navigate('mascotas')}>Explorar mascotas <Icon name="arrow" size={15}/></button></div><Icon name="heart" size={104}/></div>
    <Section title="Ocupación del refugio"><div className="cap-head"><div><span className="cap-count">{occupied}</span><span className="cap-total"> / {db.refugio.capacidadMaxima} plazas</span></div><span className="mini-pill">{Math.round(occupied/db.refugio.capacidadMaxima*100)}% ocupado</span></div><div className="pt-progress"><span style={{width:`${Math.min(100,occupied/db.refugio.capacidadMaxima*100)}%`}}/></div><p className="muted-note">Las mascotas adoptadas liberan una plaza y permanecen en el historial.</p><div className="meta-row"><span>Plazas disponibles</span><strong>{db.refugio.capacidadMaxima-occupied}</strong></div><div className="meta-row"><span>Cuidadores</span><strong>{db.cuidadores.length}</strong></div></Section>
   </div>
   <div className="summary-grid"><Section title="Solicitudes recientes"><div className="small-list">{pending.length?pending.slice().reverse().slice(0,5).map(s=><div className="small-list-item" key={s.id}><span>{personById(db,s.adoptanteId)?.nombre||'Adoptante'} <small>· {petById(db,s.mascotaId)?.nombre}</small></span><Badge status={s.estado}/><button className="cta-inline" onClick={()=>navigate('solicitudes')}>Ver</button></div>):<Empty message="No hay solicitudes pendientes"/>}</div></Section>
    <Section title="Rescates por atender"><div className="small-list">{db.avisos.filter(a=>!['Descartado','Atendido'].includes(a.estado)).slice(0,5).map(a=><div className="small-list-item" key={a.id}><span>Aviso #{a.id} <small>· {a.tipoAnimal} · {a.nivelUrgencia}</small></span><Badge status={a.estado}/><button className="cta-inline" onClick={()=>navigate('avisos')}>Ver</button></div>)}</div></Section>
   </div><div className="panel-side"><Section title="Salud y tratamientos"><div className="meta-row"><span>Tratamientos abiertos</span><strong>{db.tratamientos.filter(t=>['Pendiente','EnCurso'].includes(t.estado)).length}</strong></div><div className="meta-row"><span>Mascotas en tratamiento</span><strong>{db.mascotas.filter(p=>p.estado==='EnTratamiento').length}</strong></div><button className="btn-pt secondary panel-quick" onClick={()=>navigate('tratamientos')}>Ver seguimiento <Icon name="arrow" size={16}/></button></Section><Section title="Actividad de rescates"><div className="meta-row"><span>Avisos activos</span><strong>{db.avisos.filter(a=>!['Atendido','Descartado'].includes(a.estado)).length}</strong></div><div className="meta-row"><span>Ingresos desde avisos</span><strong>{db.avisos.filter(a=>a.mascotaGeneradaId).length}</strong></div><button className="btn-pt secondary panel-quick" onClick={()=>navigate('avisos')}>Ver avisos <Icon name="arrow" size={16}/></button></Section></div>
 </>;
 case 'mascotas':return table(['Mascota','Especie / edad','Cuidador','Estado','Ingreso','Acciones'],listing(db.mascotas).map(m=>[
  <EntityName name={m.nombre} sub={`${m.raza||m.especie} · #${m.id}`} img={photo(m)}/>,`${m.especie} · ${m.edadAproximada} años`,db.cuidadores.find(c=>c.id===m.cuidadorId)?.nombre||'Sin asignar',<Badge status={m.estado}/>,dateLabel(m.fechaIngreso),
  <TableActions actions={[...actions('mascotas',m.id,m.nombre),{label:'Asignar',icon:'users',disabled:m.estado==='Adoptada',onClick:()=>setPopup({kind:'assign',id:m.id})},{...statusButton('mascotas',m.id,m.estado),disabled:m.estado==='Adoptada'}]}/>
 ]));
 case 'avisos':return table(['Aviso','Contacto','Ubicación','Urgencia','Estado','Acciones'],listing(db.avisos).map(a=>[
  <EntityName name={`Aviso #${a.id}`} sub={`${a.tipoAnimal} · ${a.estadoAparente}`}/>,a.contactoNombre,`${a.latitud.toFixed(3)}, ${a.longitud.toFixed(3)}`,<Badge status={a.nivelUrgencia}/>,<Badge status={a.estado}/>,
  <TableActions actions={[...actions('avisos',a.id,`Aviso #${a.id}`,!['Descartado','Atendido'].includes(a.estado)),{...statusButton('avisos',a.id,a.estado),disabled:['Atendido','Descartado'].includes(a.estado)},{label:'Ingresar mascota',icon:'plus',disabled:a.estado!=='Aceptado'||!!a.mascotaGeneradaId,onClick:()=>setPopup({kind:'intake',noticeId:a.id})}]}/>
 ]));
 case 'cuidadores':return <>{table(['Cuidador','Especialización','Asignadas','Capacidad','Acciones'],listing(db.cuidadores).map(c=>[
  <EntityName name={c.nombre} sub={`Cuidador #${c.id}`}/>,c.especializacion||'—',caregiverLoad(db,c.id),<div className="capacity-inline"><strong>{caregiverLoad(db,c.id)}/{c.capacidadMaxima}</strong><div className="pt-progress"><span style={{width:`${Math.min(100,100*caregiverLoad(db,c.id)/c.capacidadMaxima)}%`}}/></div></div>,<TableActions actions={actions('cuidadores',c.id,c.nombre)}/>
 ]))}<p className="admin-bottom-help">Para asignar una mascota, abrí la sección Mascotas y utilizá la acción «Asignar».</p></>;
 case 'tratamientos':return table(['Mascota','Tratamiento','Inicio / fin','Estado','Acciones'],listing(db.tratamientos.map(t=>({...t,mascotaNombre:petById(db,t.mascotaId)?.nombre||''}))).map(t=>[
  <EntityName name={petById(db,t.mascotaId)?.nombre||'Mascota'} sub={`Tratamiento #${t.id}`} img={photo(petById(db,t.mascotaId))}/>,<div><strong>{display(t.tipo)}</strong><small className="table-detail">{t.descripcion}</small></div>,<span>{dateLabel(t.fechaInicio)}<small className="table-detail">{dateLabel(t.fechaFin)}</small></span>,<Badge status={t.estado}/>,<TableActions actions={[...actions('tratamientos',t.id,`Tratamiento #${t.id}`),statusButton('tratamientos',t.id,t.estado)]}/>
 ]));
 case 'historiales':return table(['Mascota','Observaciones','Tratamientos','Acciones'],listing(db.historiales.map(h=>({...h,nombreMascota:petById(db,h.mascotaId)?.nombre||''}))).map(h=>[
  <EntityName name={petById(db,h.mascotaId)?.nombre||'Mascota'} sub={`Historial #${h.id}`} img={photo(petById(db,h.mascotaId))}/>,h.observaciones||'Sin observaciones',db.tratamientos.filter(t=>t.historialSanitarioId===h.id).length,<TableActions actions={actions('historiales',h.id,`Historial #${h.id}`)}/>
 ]));
 case 'adoptantes':return table(['Adoptante','DNI','Contacto','Solicitudes','Acciones'],listing(db.adoptantes).map(a=>[
  <EntityName name={a.nombre} sub={`Persona #${a.id}`}/>,a.dni,<span>{a.telefono||'Sin teléfono'}<small className="table-detail">{a.direccion||''}</small></span>,db.solicitudes.filter(s=>s.adoptanteId===a.id).length,<TableActions actions={actions('adoptantes',a.id,a.nombre)}/>
 ]));
 case 'solicitudes':return table(['Adoptante','Mascota','Fecha','Estado','Acciones'],listing(db.solicitudes.map(s=>({...s,mascotaNombre:petById(db,s.mascotaId)?.nombre||'',adoptanteNombre:personById(db,s.adoptanteId)?.nombre||''}))).map(s=>[
  <EntityName name={personById(db,s.adoptanteId)?.nombre||'Adoptante'} sub={`Solicitud #${s.id}`}/>,<EntityName name={petById(db,s.mascotaId)?.nombre||'Mascota'} img={photo(petById(db,s.mascotaId))}/>,dateLabel(s.fechaSolicitud),<Badge status={s.estado}/>,<TableActions actions={[...actions('solicitudes',s.id,`Solicitud #${s.id}`,false),{...statusButton('solicitudes',s.id,s.estado),disabled:!!adoptionFor(db,s.mascotaId)||['Rechazada','Cancelada'].includes(s.estado)},{label:'Concretar',icon:'check',disabled:s.estado!=='Aprobada'||!!adoptionFor(db,s.mascotaId),onClick:()=>setPopup({kind:'adopt',requestId:s.id})}]}/>
 ]));
 case 'adopciones':return table(['Registro','Mascota','Adoptante','Fecha','Detalle'],listing(db.adopciones.map(a=>({...a,mascota:petById(db,requestById(db,a.solicitudAdopcionId)?.mascotaId||-1)?.nombre||'',adoptanteNombre:personById(db,requestById(db,a.solicitudAdopcionId)?.adoptanteId||-1)?.nombre||''}))).map(a=>{
  const s=requestById(db,a.solicitudAdopcionId);const p=petById(db,s?.mascotaId||-1);
  return [<EntityName name={`Adopción #${a.id}`} sub={`Solicitud #${a.solicitudAdopcionId}`}/>,<EntityName name={p?.nombre||'Mascota'} img={photo(p)}/>,personById(db,s?.adoptanteId||-1)?.nombre||'—',dateLabel(a.fechaAdopcion),<TableActions actions={[{label:'Ver',icon:'eye',onClick:()=>detail(`Adopción #${a.id}`,a,'adopciones')}]}/>];
 }));
 case 'refugio':return <div className="summary-grid"><Section title="Datos del establecimiento"><div className="refugio-heading"><div className="refugio-mark"><Icon name="house" size={28}/></div><div><h3>{db.refugio.nombre}</h3><p>Gestión local · datos de demostración</p></div></div><div className="meta-row"><span>Capacidad máxima</span><strong>{db.refugio.capacidadMaxima} plazas</strong></div><div className="meta-row"><span>Ocupación actual</span><strong>{occupied} mascotas</strong></div><div className="meta-row"><span>Plazas disponibles</span><strong>{db.refugio.capacidadMaxima-occupied}</strong></div><div className="pt-progress panel-quick"><span style={{width:`${Math.min(100,occupied/db.refugio.capacidadMaxima*100)}%`}}/></div><button className="btn-pt primary panel-quick" onClick={()=>setPopup({kind:'refugio'})}><Icon name="edit" size={14}/> Editar datos</button></Section><Section title="Información importante"><p className="muted-note">La capacidad se aplica a mascotas no adoptadas. No se puede reducir por debajo de la ocupación actual.</p><p className="muted-note">Los registros de prueba permanecen en este navegador mientras no se borren los datos locales.</p><button className="btn-pt danger panel-quick" onClick={()=>setPopup({kind:'reset'})}><Icon name="refresh" size={14}/> Restaurar demostración</button></Section></div>;
 }
 }
 const headline=popup?.kind==='edit'?(popup.data?'Editar':'Registrar')+' '+({mascotas:'mascota',avisos:'aviso de rescate',cuidadores:'cuidador',tratamientos:'tratamiento',historiales:'historial sanitario',adoptantes:'adoptante',solicitudes:'solicitud de adopción'}[popup.collection]):'';
 function modal():ReactNode{
  if(!popup)return null;
  if(popup.kind==='edit')return <Modal title={headline} onClose={()=>setPopup(null)} wide><EntryForm key={`${popup.collection}-${popup.data?.id||'new'}`} fields={editFields(popup.collection,db,popup.data)} values={popup.data} onClose={()=>setPopup(null)} submitLabel={popup.data?'Guardar cambios':'Registrar'} onSubmit={values=>action({type:'save',collection:popup.collection,values:{...values,...(popup.data?.id?{id:popup.data.id}:{}),...(popup.collection==='tratamientos'&&popup.data?.id?{mascotaId:popup.data.mascotaId}:{}),...(popup.collection==='historiales'&&popup.data?.id?{mascotaId:popup.data.mascotaId}:{})}},'Registro guardado correctamente.')} hint={popup.collection==='solicitudes'?'Solo se pueden crear solicitudes para mascotas disponibles, sin tratamientos activos y no adoptadas.':undefined}/></Modal>;
  if(popup.kind==='state'){
   const states={mascotas:PET_STATES,avisos:AVISO_STATES,tratamientos:TR_STATES,solicitudes:REQ_STATES}[popup.collection];
   const choices=popup.collection==='avisos'?states.filter(x=>x!=='Atendido'):popup.collection==='mascotas'?states.filter(x=>x!=='Adoptada'&&x!=='Reservada'):states;
   return <Modal title="Actualizar estado" onClose={()=>setPopup(null)}><EntryForm key={`${popup.collection}-${popup.id}`} fields={[{name:'estado',label:'Nuevo estado',type:'select',options:choices}]} values={{estado:popup.current}} onClose={()=>setPopup(null)} onSubmit={v=>action({type:'state',collection:popup.collection,id:popup.id,state:v.estado},'Estado actualizado.')} hint="Se aplican las reglas del refugio antes de aceptar el cambio."/></Modal>;
  }
  if(popup.kind==='assign')return <Modal title="Asignar cuidador" onClose={()=>setPopup(null)}><EntryForm fields={[{name:'cuidadorId',label:'Cuidador responsable',type:'select',options:[{value:'',label:'Sin asignar'},...opt(db.cuidadores)],required:false}]} values={{cuidadorId:petById(db,popup.id)?.cuidadorId||''}} onClose={()=>setPopup(null)} onSubmit={v=>action({type:'assign',id:popup.id,caregiverId:v.cuidadorId?Number(v.cuidadorId):null},'Asignación actualizada.')} hint="El sistema controla la capacidad máxima de cada cuidador."/></Modal>;
  if(popup.kind==='intake'){
   const notice=db.avisos.find(n=>n.id===popup.noticeId);
   return <Modal title={`Ingresar mascota desde aviso #${popup.noticeId}`} onClose={()=>setPopup(null)} wide><EntryForm fields={editFields('mascotas',db).filter(f=>f.name!=='especie')} values={{edadAproximada:1,tamano:'Mediano',sexo:'Hembra',foto:notice?.tipoAnimal==='Gato'?'pet-luna.jpg':'pet-maple.jpg'}} onClose={()=>setPopup(null)} submitLabel="Confirmar ingreso" hint={`Aviso aceptado. Especie informada: ${notice?.tipoAnimal||'—'}. Se creará una mascota y el aviso quedará atendido.`} onSubmit={v=>action({type:'intake',noticeId:popup.noticeId,values:v},'Mascota ingresada desde el aviso.')} /></Modal>;
  }
  if(popup.kind==='adopt'){
   const approved=db.solicitudes.filter(s=>s.estado==='Aprobada'&&!adoptionFor(db,s.mascotaId));
   return <Modal title="Concretar adopción" onClose={()=>setPopup(null)}><EntryForm fields={[{name:'solicitudId',label:'Solicitud aprobada',type:'select',options:approved.map(s=>({value:String(s.id),label:`#${s.id} · ${petById(db,s.mascotaId)?.nombre} · ${personById(db,s.adoptanteId)?.nombre}`}))},{name:'observaciones',label:'Observaciones',type:'textarea',required:false,full:true}]} values={{solicitudId:popup.requestId??'',observaciones:''}} onClose={()=>setPopup(null)} submitLabel="Concretar adopción" hint="La solicitud debe estar aprobada y la mascota reservada. Una adopción definitiva no puede duplicarse." onSubmit={v=>action({type:'adopt',requestId:Number(v.solicitudId),observations:v.observaciones},'Adopción concretada. Historial conservado.')}/></Modal>;
  }
  if(popup.kind==='refugio')return <Modal title="Editar refugio" onClose={()=>setPopup(null)}><EntryForm fields={[{name:'nombre',label:'Nombre del refugio'},{name:'capacidadMaxima',label:'Capacidad máxima',type:'number',min:1}]} values={{nombre:db.refugio.nombre,capacidadMaxima:db.refugio.capacidadMaxima}} onClose={()=>setPopup(null)} onSubmit={v=>action({type:'refugio',name:v.nombre,capacity:Number(v.capacidadMaxima)},'Configuración guardada.')}/></Modal>;
  if(popup.kind==='delete')return <Confirm title={`Eliminar ${popup.label}`} description="Esta acción elimina el registro de la demostración. Se bloqueará si tiene relaciones o historial que deban conservarse." onCancel={()=>setPopup(null)} onConfirm={()=>action({type:'delete',collection:popup.collection,id:popup.id},'Registro eliminado.')}/>;
  if(popup.kind==='reset')return <Confirm title="Restaurar la demostración" description="Se eliminarán los cambios hechos en este navegador y se restablecerán todos los registros de ejemplo." onCancel={()=>setPopup(null)} onConfirm={()=>{reset();setPopup(null);setQuery('');setStateFilter('Todos');}}/>;
  if(popup.kind==='detail')return <Modal title={popup.title} description="Información registrada en el sistema de prueba" onClose={()=>setPopup(null)} wide>{popup.kindName==='mascotas'&&<img className="detail-pet-photo" src={photo(popup.data as unknown as Mascota)} alt={`Fotografía de ${popup.data.nombre}`}/>}<div className="detail-properties">{Object.entries(popup.data).filter(([k])=>k!=='foto'&&!k.startsWith('nombreMascota')).map(([key,val])=><div key={key}><span>{LABEL_PROPS[key]||key}</span><strong>{key.endsWith('Id')&&key==='mascotaId'?petById(db,Number(val))?.nombre||'—':key==='cuidadorId'?db.cuidadores.find(x=>x.id===val)?.nombre||'—':key==='adoptanteId'?personById(db,Number(val))?.nombre||'—':key==='solicitudAdopcionId'?`Solicitud #${val}`:key==='estado'?display(val):key.startsWith('fecha')?dateLabel(String(val||'')):val==null?'—':String(val)}</strong></div>)}</div>{popup.kindName==='mascotas'&&<div className="related-block"><h3>Seguimiento</h3><p>Tratamientos: {db.tratamientos.filter(t=>t.mascotaId===popup.data.id).length} · Solicitudes: {db.solicitudes.filter(s=>s.mascotaId===popup.data.id).length}</p><p>{isBlocking(db,Number(popup.data.id))?'Tiene tratamientos abiertos.':'Sin tratamientos pendientes o en curso.'}</p></div>}{popup.kindName==='cuidadores'&&<div className="related-block"><h3>Mascotas asignadas</h3><p>{db.mascotas.filter(m=>m.cuidadorId===popup.data.id&&m.estado!=='Adoptada').map(m=>m.nombre).join(' · ')||'No tiene mascotas asignadas'}</p></div>}{popup.kindName==='historiales'&&<div className="related-block"><h3>Tratamientos registrados</h3><p>{db.tratamientos.filter(t=>t.historialSanitarioId===popup.data.id).map(t=>`${display(t.tipo)} (${display(t.estado)})`).join(' · ')||'Sin tratamientos'}</p></div>}{popup.kindName==='solicitudes'&&<div className="related-block"><h3>Relaciones</h3><p>Mascota: {petById(db,Number(popup.data.mascotaId))?.nombre||'—'} · Adoptante: {personById(db,Number(popup.data.adoptanteId))?.nombre||'—'}</p></div>}</Modal>;
 return null;
 }
 return <div className="admin-shell">
  {mobile&&<button className="sidebar-overlay" aria-label="Cerrar menú" onClick={()=>setMobile(false)}/>}
  <aside className={`sneat-menu ${mobile?'open':''}`} aria-label="Navegación administrativa"><div className="brand-block"><a href="/index.html" className="admin-brand"><span className="brand-emblem"><Icon name="house" size={23}/></span><span><strong>patitas<span className="brand-dot">.</span></strong><small>GESTIÓN DEL REFUGIO</small></span></a><button className="icon-action mobile-close" onClick={()=>setMobile(false)} aria-label="Cerrar menú"><Icon name="x"/></button></div>
  <nav className="admin-nav">{NAV_GROUPS.map((group,i)=><div key={i}><div className="nav-head">{META[group[0]].section}</div>{group.map(v=><button type="button" className={`nav-link ${view===v?'active':''}`} key={v} onClick={()=>navigate(v)}><Icon name={META[v].icon} size={18}/><span>{v==='resumen'?'Resumen':v==='avisos'?'Avisos de rescate':v==='historiales'?'Historial sanitario':v==='refugio'?'Refugio':v==='solicitudes'?'Solicitudes':META[v].title}</span>{v==='solicitudes'&&pending.length>0&&<span className="nav-count">{pending.length}</span>}</button>)}</div>)}</nav>
  <div className="admin-sidebar-foot"><div className="environment-card"><span className="environment-dot"/><div><strong>Modo demostración</strong><small>Datos guardados en este navegador</small></div></div><a className="back-public" href="/index.html"><Icon name="arrow" size={15}/> Sitio público</a></div></aside>
  <div className="admin-main-column"><header className="topbar"><button className="icon-action hamburger" aria-label="Abrir menú" onClick={()=>setMobile(true)}><Icon name="menu" size={22}/></button><div className="breadcrumbs">Administración <Icon name="chevron" size={15}/><strong>{META[view].title}</strong></div><div className="topbar-actions"><span className="environment-chip"><span className="environment-dot"/> Demo local</span><a href="/index.html" className="top-public"><Icon name="logout" size={14}/> Ver sitio</a><span className="avatar-circle">RP</span></div></header>
  <main className="admin-main" id="main"><div className="page-header"><div><div className="overline">REFUGIO PATITAS <span className="overline-sep">/</span> ADMINISTRACIÓN</div><h1>{META[view].title}</h1><p id="view-description">{META[view].desc}</p></div>{BUTTONS[view]&&<div className="page-actions"><button className="btn-pt primary" type="button" onClick={openCreate}><Icon name={view==='refugio'?'edit':'plus'} size={17}/>{BUTTONS[view]}</button></div>}</div>{currentPage()}</main>
  <footer className="admin-footer"><span>Patitas · Trabajo integrador PSR 2026</span><span>Inspirado en Sneat Free y Pawprint · React + TypeScript</span></footer></div>
  {modal()}<Toast/>
 </div>;
}
