import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { ApiError, api } from '../api/client';
import type { Sesion } from '../api/types';
import { BuzonDialog } from './Buzon';
import { LoginDialog } from './LoginDialog';
import { useNotifier } from './Notifier';

type OpcionesLogin = { motivo?: string; soloIngresar?: boolean; alIngresar?: (s: Sesion) => void };

type Ctx = {
  sesion: Sesion | null;
  cargando: boolean;
  esAdmin: boolean;
  noLeidas: number;
  /** Abre el diálogo de inicio de sesión / registro. */
  abrirLogin: (opciones?: OpcionesLogin) => void;
  abrirBuzon: () => void;
  cerrarSesion: () => Promise<void>;
  refrescar: () => Promise<Sesion | null>;
  actualizarNoLeidas: () => Promise<void>;
};

const SesionContext = createContext<Ctx | null>(null);

/**
 * Sesión de la persona (cookie HttpOnly manejada por la API).
 * También muestra los diálogos de inicio de sesión y del buzón.
 */
export function SesionProvider({ children }: { children: ReactNode }) {
  const notifier = useNotifier();
  const [sesion, setSesion] = useState<Sesion | null>(null);
  const [cargando, setCargando] = useState(true);
  const [noLeidas, setNoLeidas] = useState(0);
  const [login, setLogin] = useState<OpcionesLogin | null>(null);
  const [buzon, setBuzon] = useState(false);
  const sesionRef = useRef<Sesion | null>(null);

  const refrescar = useCallback(async () => {
    try {
      const s = await api.sesion();
      setSesion(s); sesionRef.current = s; setNoLeidas(s.notificacionesNoLeidas);
      return s;
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) { setSesion(null); sesionRef.current = null; setNoLeidas(0); }
      return null;
    } finally {
      setCargando(false);
    }
  }, []);

  const actualizarNoLeidas = useCallback(async () => {
    if (!sesionRef.current) return;
    try { setNoLeidas((await api.buzonNoLeidas()).cantidad); } catch { /* se reintenta en la próxima consulta */ }
  }, []);

  useEffect(() => { void refrescar(); }, [refrescar]);

  // Mensajes nuevos: se consultan al volver a la pestaña y cada 60 segundos
  useEffect(() => {
    if (!sesion) return;
    const alVolver = () => { if (document.visibilityState === 'visible') void actualizarNoLeidas(); };
    document.addEventListener('visibilitychange', alVolver);
    const t = window.setInterval(() => void actualizarNoLeidas(), 60_000);
    return () => { document.removeEventListener('visibilitychange', alVolver); window.clearInterval(t); };
  }, [sesion, actualizarNoLeidas]);

  const cerrarSesion = useCallback(async () => {
    try { await api.logout(); } finally {
      setSesion(null); sesionRef.current = null; setNoLeidas(0);
      notifier.exito('Cerraste la sesión.');
    }
  }, [notifier]);

  const value = useMemo<Ctx>(() => ({
    sesion, cargando, noLeidas,
    esAdmin: sesion?.usuario.rol === 'Administrador',
    abrirLogin: opciones => setLogin(opciones ?? {}),
    abrirBuzon: () => setBuzon(true),
    cerrarSesion, refrescar, actualizarNoLeidas
  }), [sesion, cargando, noLeidas, cerrarSesion, refrescar, actualizarNoLeidas]);

  return (
    <SesionContext.Provider value={value}>
      {children}
      {login && (
        <LoginDialog motivo={login.motivo} soloIngresar={login.soloIngresar} onClose={() => setLogin(null)}
          onSesion={s => {
            setSesion(s); sesionRef.current = s; setNoLeidas(s.notificacionesNoLeidas);
            const continuar = login.alIngresar;
            setLogin(null);
            notifier.exito(`¡Hola, ${s.usuario.nombre}! Iniciaste sesión.`);
            continuar?.(s);
          }} />
      )}
      {buzon && sesion && <BuzonDialog onClose={() => setBuzon(false)} onCambio={setNoLeidas} />}
    </SesionContext.Provider>
  );
}

export function useSesion() {
  const ctx = useContext(SesionContext);
  if (!ctx) throw new Error('Falta SesionProvider');
  return ctx;
}
