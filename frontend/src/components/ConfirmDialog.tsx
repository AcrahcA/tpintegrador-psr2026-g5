import { useState, type ReactNode } from 'react';
import { Dialog } from './Dialog';

type Props = {
  titulo: string;
  children: ReactNode;
  confirmar: string;
  peligro?: boolean;
  onConfirm: () => Promise<unknown> | void;
  onClose: () => void;
};

export function ConfirmDialog({ titulo, children, confirmar, peligro, onConfirm, onClose }: Props) {
  const [enviando, setEnviando] = useState(false);
  return (
    <Dialog title={titulo} onClose={onClose} size="sm">
      <div className="confirm__body">{children}</div>
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onClose} data-autofocus>No, volver</button>
        <button type="button" className={`button ${peligro ? 'button--danger' : 'button--primary'}`} disabled={enviando}
          onClick={async () => {
            setEnviando(true);
            try { await onConfirm(); onClose(); } catch { /* el error se informa desde onConfirm */ } finally { setEnviando(false); }
          }}>
          {enviando ? 'Procesando…' : confirmar}
        </button>
      </div>
    </Dialog>
  );
}
