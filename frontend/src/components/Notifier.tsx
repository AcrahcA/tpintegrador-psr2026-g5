import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { Icon } from './Icon';

type Mensaje = { id: number; texto: string; tipo: 'exito' | 'error' };
type Ctx = { exito: (texto: string) => void; error: (texto: string) => void };

const NotifierContext = createContext<Ctx | null>(null);

/**
 * Mensajes de estado (WCAG 4.1.3): los éxitos se anuncian con role="status" (cortés)
 * y los errores con role="alert". Los errores no desaparecen solos.
 */
export function NotifierProvider({ children }: { children: ReactNode }) {
  const [mensajes, setMensajes] = useState<Mensaje[]>([]);
  const siguiente = useRef(1);

  const agregar = useCallback((texto: string, tipo: Mensaje['tipo']) => {
    const id = siguiente.current++;
    setMensajes(m => [...m.slice(-2), { id, texto, tipo }]);
  }, []);
  const quitar = (id: number) => setMensajes(m => m.filter(x => x.id !== id));

  const ctx: Ctx = { exito: useCallback(t => agregar(t, 'exito'), [agregar]), error: useCallback(t => agregar(t, 'error'), [agregar]) };

  return (
    <NotifierContext.Provider value={ctx}>
      {children}
      <div className="toasts">
        <div role="status" aria-live="polite" className="toasts__region">
          {mensajes.filter(m => m.tipo === 'exito').map(m => <Toast key={m.id} m={m} onClose={() => quitar(m.id)} />)}
        </div>
        <div role="alert" aria-live="assertive" className="toasts__region">
          {mensajes.filter(m => m.tipo === 'error').map(m => <Toast key={m.id} m={m} onClose={() => quitar(m.id)} />)}
        </div>
      </div>
    </NotifierContext.Provider>
  );
}

function Toast({ m, onClose }: { m: Mensaje; onClose: () => void }) {
  useEffect(() => {
    if (m.tipo !== 'exito') return;
    const t = window.setTimeout(onClose, 8000);
    return () => window.clearTimeout(t);
  }, [m, onClose]);
  return (
    <div className={`toast toast--${m.tipo}`}>
      <Icon name={m.tipo === 'exito' ? 'check' : 'alert'} />
      <p>{m.texto}</p>
      <button type="button" className="icon-button icon-button--small" onClick={onClose}>
        <Icon name="close" size={16} /><span className="sr-only">Cerrar mensaje</span>
      </button>
    </div>
  );
}

export function useNotifier() {
  const ctx = useContext(NotifierContext);
  if (!ctx) throw new Error('Falta NotifierProvider');
  return ctx;
}
