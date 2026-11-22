import { useId, useState, type ReactNode } from 'react';
import { Dialog } from './Dialog';
import { Icon } from './Icon';

/**
 * Confirmación de un rechazo / cancelación / descarte con un MOTIVO OPCIONAL.
 * Si la persona tiene cuenta, recibe el mensaje (con el motivo, si se escribió) en su buzón.
 */
export function MotivoDialog({ titulo, children, confirmar, tieneCuenta, onConfirm, onClose }: {
  titulo: string; children: ReactNode; confirmar: string; tieneCuenta: boolean;
  onConfirm: (motivo: string | null) => Promise<boolean>; onClose: () => void;
}) {
  const [motivo, setMotivo] = useState('');
  const [enviando, setEnviando] = useState(false);
  const id = useId();
  const MAX = 500;

  return (
    <Dialog title={titulo} onClose={onClose}>
      <div className="confirm__body">{children}</div>
      <div className="field">
        <label htmlFor={id} className="field__label">Motivo <span className="field__optional">(opcional)</span></label>
        <textarea id={id} className="input input--textarea" rows={3} maxLength={MAX} value={motivo}
          aria-describedby={`${id}-ayuda`} onChange={e => setMotivo(e.target.value)} data-autofocus />
        <p id={`${id}-ayuda`} className="field__hint">
          Podés explicar brevemente la decisión. {MAX - motivo.length} caracteres disponibles.
        </p>
      </div>
      <p className="motivo-cuenta">
        <Icon name={tieneCuenta ? 'mail' : 'info'} size={18} />
        {tieneCuenta
          ? 'La persona tiene cuenta: va a recibir este aviso en su buzón, junto con el motivo si lo escribís.'
          : 'La persona no tiene cuenta en el sitio, así que no va a recibir un aviso en el buzón.'}
      </p>
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onClose}>No, volver</button>
        <button type="button" className="button button--danger" disabled={enviando}
          onClick={async () => {
            setEnviando(true);
            const ok = await onConfirm(motivo.trim() || null);
            setEnviando(false);
            if (ok) onClose();
          }}>
          {enviando ? 'Procesando…' : confirmar}
        </button>
      </div>
    </Dialog>
  );
}
