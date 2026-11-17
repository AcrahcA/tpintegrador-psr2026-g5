import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { createInitialData } from '../model/seed';
import { execute, type Command } from '../model/rules';
import type { DB } from '../model/types';
const KEY = 'patitas_react_psr2026_v2';
function load():DB {try{const saved=localStorage.getItem(KEY);if(saved){const val=JSON.parse(saved) as DB;if(val.version===2&&Array.isArray(val.mascotas)&&Array.isArray(val.adopciones))return val;}}catch{ /* datos de ejemplo */ }return createInitialData();}
type Notice = { text:string; variant:'ok'|'error' }|null;
type ContextValue = { db:DB; run:(cmd:Command,success?:string)=>boolean; reset:()=>void; notice:Notice; clearNotice:()=>void };
const Ctx = createContext<ContextValue|null>(null);
export function RefugioProvider({children}:{children:ReactNode}) {
 const [db,setDb]=useState<DB>(load);const data=useRef(db);const [notice,setNotice]=useState<Notice>(null);
 const run=useCallback((command:Command,success='Cambios guardados correctamente.')=>{
   try{const next=execute(data.current,command);localStorage.setItem(KEY,JSON.stringify(next));data.current=next;setDb(next);setNotice({text:success,variant:'ok'});return true;}
   catch(error){setNotice({text:error instanceof Error?error.message:'No se pudo realizar la operación.',variant:'error'});return false;}
 },[]);
 const reset=useCallback(()=>{const next=createInitialData();data.current=next;setDb(next);localStorage.setItem(KEY,JSON.stringify(next));setNotice({text:'Se restauraron los datos de demostración.',variant:'ok'});},[]);
 useEffect(()=>{const handler=(e:StorageEvent)=>{if(e.key===KEY){const next=load();data.current=next;setDb(next);}};window.addEventListener('storage',handler);return()=>window.removeEventListener('storage',handler);},[]);
 useEffect(()=>{if(!notice)return;const id=window.setTimeout(()=>setNotice(null),5100);return()=>window.clearTimeout(id);},[notice]);
 return <Ctx.Provider value={{db,run,reset,notice,clearNotice:()=>setNotice(null)}}>{children}</Ctx.Provider>;
}
export function useRefugio(){const ctx=useContext(Ctx);if(!ctx)throw new Error('Falta RefugioProvider');return ctx;}
