import { useMemo, useState } from 'react';
import { api } from '../../api/client';
import { accionAviso, etiqueta, fecha, transicionesAviso } from '../../api/format';
import type { Aviso, EstadoAviso } from '../../api/types';
import { Badge } from '../../components/Badge';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { Dialog } from '../../components/Dialog';
import { AvisoForm, MascotaForm } from '../../components/forms';
import { Icon } from '../../components/Icon';
import { useDatos } from '../data';
import { Encabezado, EnlaceMascota } from '../ui';

const FILTROS = [
  { valor: 'pendientes', texto: 'Sin resolver' }, { valor: 'Aceptado', texto: 'Aceptados' },
  { valor: 'Atendido', texto: 'Atendidos' }, { valor: 'Descartado', texto: 'Descartados' }, { valor: 'todos', texto: 'Todos' }
];
const URGENCIA = { Alta: 0, Media: 1, Baja: 2 } as const;

export function Avisos() {
  const { datos, ejecutar, mascota } = useDatos();
  const [filtro, setFiltro] = useState('pendientes');
  const [nuevo, setNuevo] = useState(false);
  const [ingreso, setIngreso] = useState<Aviso | null>(null);
  const [descartar, setDescartar] = useState<Aviso | null>(null);

  const cuenta = (valor: string) => datos.avisos.filter(a => coincide(a, valor)).length;
  const lista = useMemo(() => datos.avisos.filter(a => coincide(a, filtro))
    .sort((a, b) => URGENCIA[a.nivelUrgencia] - URGENCIA[b.nivelUrgencia] || b.fechaAviso.localeCompare(a.fechaAviso)), [datos.avisos, filtro]);
  const lleno = !datos.capacidad.tieneLugarDisponible;

  const cambiar = (a: Aviso, estado: EstadoAviso) => {
    if (estado === 'Descartado') { setDescartar(a); return; }
    void ejecutar(() => api.cambiarEstadoAviso(a.id, estado), `Aviso #${a.id}: ${etiqueta(estado).toLowerCase()}.`);
  };

  return (
    <>
      <Encabezado titulo="Avisos de rescate" descripcion="Reportes de animales que necesitan ayuda, recibidos desde el sitio público o cargados por el equipo."
        acciones={<button type="button" className="button button--primary" onClick={() => setNuevo(true)}><Icon name="plus" /> Registrar aviso</button>} />
      {lleno && <p className="callout callout--danger"><Icon name="alert" /> El refugio está completo: los avisos pueden quedar pendientes o derivarse a otra organización.</p>}

      <div className="segmented" role="group" aria-label="Mostrar avisos">
        {FILTROS.map(f => (
          <button key={f.valor} type="button" aria-pressed={filtro === f.valor} onClick={() => setFiltro(f.valor)}>
            {f.texto} <span className="segmented__count">({cuenta(f.valor)})</span>
          </button>
        ))}
      </div>
      <p className="results-count" role="status">{lista.length === 1 ? '1 aviso' : `${lista.length} avisos`}</p>

      {lista.length === 0 ? <p className="empty">No hay avisos en esta categoría.</p> : (
        <ul className="notice-list">
          {lista.map(a => (
            <li key={a.id} className={`notice notice--${a.nivelUrgencia.toLowerCase()}`}>
              <article aria-labelledby={`aviso-${a.id}`}>
                <header className="notice__header">
                  <h2 id={`aviso-${a.id}`} className="notice__title">Aviso #{a.id}: {etiqueta(a.tipoAnimal)} {etiqueta(a.estadoAparente).toLowerCase()}</h2>
                  <p className="item__badges">
                    <Badge estado={a.nivelUrgencia}>{`Urgencia ${etiqueta(a.nivelUrgencia).toLowerCase()}`}</Badge> <Badge estado={a.estado} />
                  </p>
                </header>
                <p className="notice__desc">{a.descripcion}</p>
                <dl className="facts facts--compact">
                  <div><dt>Lugar</dt><dd>{a.direccion ?? 'Sin dirección'} ·{' '}
                    <a href={`https://www.openstreetmap.org/?mlat=${a.latitud}&mlon=${a.longitud}#map=17/${a.latitud}/${a.longitud}`} target="_blank" rel="noreferrer" className="link-with-icon">
                      Ver en el mapa<span className="sr-only"> la ubicación del aviso #{a.id} (se abre en una pestaña nueva)</span> <Icon name="external" size={14} />
                    </a></dd></div>
                  <div><dt>Reportó</dt><dd>{a.contactoNombre}{a.contactoTelefono && <> · <a href={`tel:${a.contactoTelefono.replace(/[^\d+]/g, '')}`}>{a.contactoTelefono}</a></>}</dd></div>
                  <div><dt>Fecha</dt><dd>{fecha(a.fechaAviso)}</dd></div>
                  {a.mascotaGeneradaId && <div><dt>Mascota ingresada</dt><dd><EnlaceMascota mascota={mascota(a.mascotaGeneradaId)} id={a.mascotaGeneradaId} /></dd></div>}
                </dl>
                {(transicionesAviso[a.estado].length > 0 || a.estado === 'Aceptado') && (
                  <div className="button-row">
                    {a.estado === 'Aceptado' && (
                      <button type="button" className="button button--primary button--small" onClick={() => setIngreso(a)} disabled={lleno}>
                        <Icon name="paw" size={16} /> Registrar ingreso<span className="sr-only"> del animal del aviso #{a.id}</span>
                      </button>
                    )}
                    {transicionesAviso[a.estado].map(e => (
                      <button key={e} type="button" disabled={e === 'Aceptado' && lleno}
                        className={`button button--small ${e === 'Descartado' ? 'button--ghost-danger' : e === 'Aceptado' ? 'button--primary' : 'button--secondary'}`}
                        onClick={() => cambiar(a, e)}>
                        {accionAviso[e]}<span className="sr-only"> aviso #{a.id}</span>
                      </button>
                    ))}
                  </div>
                )}
              </article>
            </li>
          ))}
        </ul>
      )}

      {nuevo && (
        <Dialog title="Registrar aviso de rescate" onClose={() => setNuevo(false)} size="lg">
          <AvisoForm textoEnviar="Registrar aviso" onCancel={() => setNuevo(false)}
            onSubmit={async d => { const a = await api.crearAviso(d); setNuevo(false); await ejecutar(async () => a, `Aviso #${a.id} registrado.`); }} />
        </Dialog>
      )}
      {ingreso && (
        <Dialog title={`Registrar ingreso del aviso #${ingreso.id}`} onClose={() => setIngreso(null)}
          description="El rescate se realizó: se crea la mascota en estado “Ingresada” y el aviso pasa a “Atendido”.">
          <MascotaForm inicial={{ especie: ingreso.tipoAnimal }} textoEnviar="Registrar ingreso" onCancel={() => setIngreso(null)}
            onSubmit={async d => {
              const m = await api.ingresarDesdeAviso(ingreso.id, d);
              setIngreso(null);
              await ejecutar(async () => m, `${m.nombre} ingresó al refugio. Podés cargar sus fotos desde la ficha.`);
              window.location.hash = `#/mascotas/${m.id}`;
            }} />
        </Dialog>
      )}
      {descartar && (
        <ConfirmDialog titulo={`Descartar el aviso #${descartar.id}`} confirmar="Sí, descartar" peligro onClose={() => setDescartar(null)}
          onConfirm={() => ejecutar(() => api.cambiarEstadoAviso(descartar.id, 'Descartado'), `Aviso #${descartar.id} descartado.`)}>
          <p>El aviso quedará cerrado (no requiere intervención o no pudo concretarse). Esta acción no se puede revertir.</p>
        </ConfirmDialog>
      )}
    </>
  );
}

function coincide(a: Aviso, filtro: string) {
  if (filtro === 'todos') return true;
  if (filtro === 'pendientes') return a.estado === 'Reportado' || a.estado === 'EnEvaluacion';
  return a.estado === filtro;
}
