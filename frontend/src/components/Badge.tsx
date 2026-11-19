import { etiqueta, tonoEstado, type Tono } from '../api/format';

export function Badge({ estado, tono, children }: { estado?: string; tono?: Tono; children?: string }) {
  const t = tono ?? tonoEstado(estado ?? '');
  return <span className={`badge badge--${t}`}>{children ?? etiqueta(estado)}</span>;
}
