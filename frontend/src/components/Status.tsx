import type { ReactNode } from 'react';
import { Icon } from './Icon';

export function Loading({ texto = 'Cargando…' }: { texto?: string }) {
  return <div className="status-box" role="status"><span className="spinner" aria-hidden="true" />{texto}</div>;
}

export function ErrorBox({ mensaje, onRetry }: { mensaje: string; onRetry?: () => void }) {
  return (
    <div className="status-box status-box--error" role="alert">
      <Icon name="alert" />
      <div>
        <p><strong>No pudimos cargar la información.</strong> {mensaje}</p>
        {onRetry && <button type="button" className="button button--secondary" onClick={onRetry}><Icon name="refresh" /> Reintentar</button>}
      </div>
    </div>
  );
}

export function Empty({ children }: { children: ReactNode }) {
  return <p className="empty">{children}</p>;
}
