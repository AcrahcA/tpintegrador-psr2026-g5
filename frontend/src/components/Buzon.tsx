import { useEffect } from 'react';
import { api } from '../api/client';
import { fecha } from '../api/format';
import type { Notificacion, TipoNotificacion } from '../api/types';
import { Badge } from './Badge';
import { Dialog } from './Dialog';
import { Icon } from './Icon';
import { ErrorBox, Loading } from './Status';
import { useAsync } from './useAsync';

const NEGATIVOS: TipoNotificacion[] = ['SolicitudRechazada', 'SolicitudCancelada', 'AvisoDescartado'];

/** Ícono del buzón (sobre de carta) con la cantidad de mensajes sin leer. */
export function BotonBuzon({ noLeidas, onClick, className }: { noLeidas: number; onClick: () => void; className?: string }) {
  // El nombre accesible empieza con el texto visible "Buzón" (WCAG 2.5.3) y agrega la cantidad de mensajes nuevos
  const etiqueta = noLeidas === 0 ? 'Buzón, sin mensajes nuevos' : noLeidas === 1 ? 'Buzón, 1 mensaje nuevo' : `Buzón, ${noLeidas} mensajes nuevos`;
  return (
    <button type="button" className={`boton-buzon ${className ?? ''}`} onClick={onClick} aria-haspopup="dialog" aria-label={etiqueta}>
      <Icon name="mail" size={24} />
      <span className="boton-buzon__texto">Buzón</span>
      {noLeidas > 0 && <span className="boton-buzon__cuenta" aria-hidden="true">{noLeidas > 99 ? '99+' : noLeidas}</span>}
    </button>
  );
}

/** Buzón de mensajes: resultados de solicitudes de adopción y avisos de rescate. */
export function BuzonDialog({ onClose, onCambio }: { onClose: () => void; onCambio: (noLeidas: number) => void }) {
  const datos = useAsync(api.buzon);
  const lista = datos.data ?? [];
  const sinLeer = lista.filter(n => !n.leida).length;

  // Al abrir el buzón los mensajes quedan vistos: se marcan como leídos (siguen resaltados en esta apertura)
  useEffect(() => {
    if (!datos.data || !datos.data.some(n => !n.leida)) return;
    api.marcarTodasLeidas().then(() => onCambio(0)).catch(() => { /* se reintenta al volver a abrir */ });
  }, [datos.data, onCambio]);

  return (
    <Dialog title="Mi buzón" size="lg" onClose={onClose}
      description={datos.data ? (sinLeer === 0 ? 'No tenés mensajes nuevos.' : sinLeer === 1 ? 'Tenés 1 mensaje nuevo.' : `Tenés ${sinLeer} mensajes nuevos.`) : undefined}>
      {datos.loading && !datos.data && <Loading texto="Cargando tus mensajes…" />}
      {datos.error && <ErrorBox mensaje={datos.error} onRetry={datos.reload} />}
      {datos.data && lista.length === 0 && (
        <p className="empty">Todavía no tenés mensajes. Acá vas a ver las respuestas a tus solicitudes de adopción y a tus avisos de rescate.</p>
      )}
      {lista.length > 0 && (
        <ul className="buzon">
          {lista.map(n => <Mensaje key={n.id} n={n} />)}
        </ul>
      )}
      <div className="form-actions">
        <button type="button" className="button button--primary" onClick={onClose} data-autofocus>Cerrar</button>
      </div>
    </Dialog>
  );
}

function Mensaje({ n }: { n: Notificacion }) {
  const negativo = NEGATIVOS.includes(n.tipo);
  return (
    <li className={`mensaje ${n.leida ? '' : 'mensaje--nuevo'} ${negativo ? 'mensaje--negativo' : ''}`}>
      <article aria-labelledby={`mensaje-${n.id}`}>
        <header className="mensaje__header">
          <h3 id={`mensaje-${n.id}`} className="mensaje__titulo">{n.titulo}</h3>
          <p className="item__badges">
            {!n.leida && <Badge tono="azul">Nuevo</Badge>}
            {negativo ? <Badge tono="rojo">{n.tipo === 'AvisoDescartado' ? 'Descartado' : n.tipo === 'SolicitudCancelada' ? 'Cancelada' : 'Rechazada'}</Badge>
              : <Badge tono="verde">{n.tipo === 'AdopcionConcretada' ? 'Adopción' : n.tipo === 'AvisoAtendido' ? 'Rescatado' : 'Aprobada'}</Badge>}
          </p>
        </header>
        <p className="mensaje__texto">{n.mensaje}</p>
        {n.motivo && (
          <div className="mensaje__motivo">
            <p className="mensaje__motivo-titulo">Motivo indicado por el refugio:</p>
            <p>{n.motivo}</p>
          </div>
        )}
        <p className="item__meta"><time dateTime={n.fecha}>{fecha(n.fecha)}</time></p>
      </article>
    </li>
  );
}
