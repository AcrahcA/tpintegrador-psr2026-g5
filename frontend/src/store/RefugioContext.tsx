import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import type { Command } from '../model/rules';
import type { DB } from '../model/types';
import { API_URL, GuardadoParcialError, executeRemote, loadDatabase } from '../services/api';

const EMPTY: DB = { version:2, refugio:{nombre:'Refugio', capacidadMaxima:1}, mascotas:[], avisos:[], cuidadores:[],
  tratamientos:[], historiales:[], adoptantes:[], solicitudes:[], adopciones:[] };
type Notice = {text:string; variant:'ok'|'error'} | null;
type ContextValue = {db:DB; run:(cmd:Command,success?:string)=>Promise<boolean>;
  reset:()=>Promise<void>; notice:Notice; clearNotice:()=>void; refresh:()=>Promise<void>; loading:boolean; busy:boolean};
const Ctx = createContext<ContextValue|null>(null);

export function RefugioProvider({children}:{children:ReactNode}) {
  const [db,setDb] = useState<DB>(EMPTY);
  const ref = useRef<DB>(EMPTY);
  const [loading,setLoading] = useState(true);
  const [busy,setBusy] = useState(false);
  const [error,setError] = useState('');
  const [notice,setNotice] = useState<Notice>(null);
  const inFlight = useRef(false);

  const refresh = useCallback(async()=>{
    try {
      const next = await loadDatabase();
      ref.current = next;
      setDb(next);
      setError('');
    } catch(e) {
      const msg = e instanceof Error ? e.message : 'No se pudo obtener información del servidor.';
      setError(msg);
      throw e;
    } finally { setLoading(false); }
  }, []);

  useEffect(() => { void refresh().catch(()=>{}); }, [refresh]);

  const run = useCallback(async (command:Command, success='Cambios guardados en el servidor.') => {
    if (inFlight.current) { setNotice({text:'Hay una operación en curso. Esperá a que finalice.', variant:'error'}); return false; }
    inFlight.current=true;
    setBusy(true);
    try {
      await executeRemote(command, ref.current);
      await refresh();
      setNotice({text:success, variant:'ok'});
      return true;
    } catch(e) {
      if (e instanceof GuardadoParcialError) {
        // La creación/edición ya ocurrió: recargar y cerrar el formulario evita
        // que el usuario repita Guardar y cree una mascota duplicada.
        await refresh().catch(()=>{});
        setNotice({text:e.message, variant:'error'});
        return true;
      }
      setNotice({text:e instanceof Error?e.message:'No se pudo completar la operación.', variant:'error'});
      return false;
    } finally {
      inFlight.current=false;
      setBusy(false);
    }
  },[refresh]);

  // Antes "reset" borraba la demo local. Ahora NO borra jamás la base de datos.
  const reset = useCallback(async()=>{await refresh();setNotice({text:'Datos actualizados desde PostgreSQL.', variant:'ok'});},[refresh]);
  useEffect(()=>{
    if(!notice) return;
    const timeout = window.setTimeout(()=>setNotice(null),6000);
    return ()=>window.clearTimeout(timeout);
  },[notice]);

  const context:ContextValue = {db,run,reset,notice,clearNotice:()=>setNotice(null),refresh,loading,busy};
  return <Ctx.Provider value={context}>
    {loading ? <main className="api-status" role="status"><h1>Conectando con el refugio</h1><p>Cargando información desde la API y PostgreSQL…</p></main>
    : error ? <main className="api-status" role="alert"><h1>No se pudo cargar el refugio</h1><p>{error}</p><p>API configurada: <code>{API_URL}</code></p><button type="button" onClick={()=>{setLoading(true);void refresh().catch(()=>{});}}>Reintentar conexión</button><p>No se muestran datos de ejemplo para evitar confundirlos con registros reales.</p></main>
    : <>{busy && <div className="api-loading" role="status">Guardando cambios en el servidor…</div>}{children}</>}
  </Ctx.Provider>;
}
export function useRefugio(){const value=useContext(Ctx);if(!value)throw new Error('Falta RefugioProvider');return value;}
