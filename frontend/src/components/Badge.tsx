import { etiqueta, tonoEstado, type Tono } from '../api/format';
import { Icon, type NombreIcono } from './Icon';

/**
 * Etiqueta de estado accesible: color + ícono + texto visible.
 * Nunca se transmite el estado sólo con color (WCAG 1.4.1).
 */
const ICONO: Record<Tono, NombreIcono> = {
  verde: 'check', ambar: 'clock', rojo: 'alert', azul: 'info', violeta: 'heart', gris: 'minus'
};

export function Badge({ estado, tono, children }: { estado?: string; tono?: Tono; children?: string }) {
  const t = tono ?? tonoEstado(estado ?? '');
  return (
    <span className={`badge badge--${t}`}>
      <Icon name={ICONO[t]} size={14} />
      {children ?? etiqueta(estado)}
    </span>
  );
}
