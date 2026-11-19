import { useState } from 'react';
import { api } from '../../api/client';
import {
  accionTratamiento, edad, estadosManualesMascota, etiqueta, fecha, fechaCorta, transicionesTratamiento
} from '../../api/format';
import type { EstadoMascota, Mascota } from '../../api/types';
import { Badge } from '../../components/Badge';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { Dialog } from '../../components/Dialog';
import { MascotaForm } from '../../components/forms';
import { Gallery } from '../../components/Gallery';
import { Icon } from '../../components/Icon';
import { PhotoManager } from '../../components/PhotoManager';
import { ErrorBox, Loading } from '../../components/Status';
import { useAsync } from '../../components/useAsync';
import { useDatos } from '../data';
import { Encabezado } from '../ui';
import { TratamientoDialog } from './Tratamientos';

export function FichaMascota({ id }: { id: number }) {
  const { mascota: buscar } = useDatos();
  const m = buscar(id);

  if (!m) {
    return (
      <>
        <Encabezado titulo="Mascota no encontrada" volver={{ href: '#/mascotas', texto: 'Volver a mascotas' }} />
        <p className="empty">No existe la mascota #{id}.</p>
      </>
    );
  }
  return <Ficha key={m.id} m={m} />;
}

function Ficha({ m }: { m: Mascota }) {
  const { datos, ejecutar, recargar, cuidador, adoptante } = useDatos();
  const [editar, setEditar] = useState(false);
  const [verGaleria, setVerGaleria] = useState(false);
  const [eliminar, setEliminar] = useState(false);
  const [nuevoTratamiento, setNuevoTratamiento] = useState(false);

  const historial = useAsync(() => Promise.all([api.historial(m.id), api.evaluacionAdopcion(m.id)]), [m.id, m.estado, datos.tratamientos]);
  const solicitudes = datos.solicitudes.filter(s => s.mascotaId === m.id);
  const adopcion = datos.adopciones.find(a => a.mascotaId === m.id);
  const aviso = m.avisoRescateOrigenId ? datos.avisos.find(a => a.id === m.avisoRescateOrigenId) : undefined;
  const adoptada = m.estado === 'Adoptada';

  return (
    <>
      <Encabezado titulo={m.nombre} volver={{ href: '#/mascotas', texto: 'Volver a mascotas' }}
        descripcion={<><Badge estado={m.estado} /> {etiqueta(m.especie)} · Mascota #{m.id}</>}
        acciones={!adoptada && <button type="button" className="button button--secondary" onClick={() => setEditar(true)}><Icon name="edit" /> Editar datos</button>} />

      <div className="detail-layout">
        <section className="card" aria-labelledby="s-datos">
          <h2 id="s-datos" className="card__title">Datos</h2>
          <dl className="facts">
            <div><dt>Especie</dt><dd>{etiqueta(m.especie)}</dd></div>
            <div><dt>Raza</dt><dd>{m.raza ?? 'Sin determinar'}</dd></div>
            <div><dt>Sexo</dt><dd>{etiqueta(m.sexo)}</dd></div>
            <div><dt>Edad aproximada</dt><dd>{edad(m.edadAproximada)}</dd></div>
            <div><dt>Tamaño</dt><dd>{etiqueta(m.tamaño)}</dd></div>
            <div><dt>Ingreso</dt><dd>{fecha(m.fechaIngreso)}</dd></div>
            <div><dt>Origen</dt><dd>{aviso ? <a href="#/avisos">Aviso de rescate #{aviso.id}</a> : 'Ingreso directo'}</dd></div>
          </dl>
          {m.estado === 'Ingresada' && !aviso && (
            <button type="button" className="button button--ghost-danger button--small" onClick={() => setEliminar(true)}>
              <Icon name="trash" size={16} /> Eliminar ingreso cargado por error
            </button>
          )}
        </section>

        <section className="card" aria-labelledby="s-estado">
          <h2 id="s-estado" className="card__title">Estado y cuidado</h2>
          {adoptada ? (
            <p>Fue adoptada {adopcion ? `el ${fecha(adopcion.fechaAdopcion)} por ${adoptante(adopcion.adoptanteId)?.nombre ?? 'su adoptante'}` : ''}. Su información se conserva en el historial.</p>
          ) : (
            <>
              <CambiarEstado m={m} motivos={historial.data?.[1].motivos} />
              <AsignarCuidador m={m} />
            </>
          )}
          {!adoptada && historial.data && (
            historial.data[1].disponibleParaAdopcion
              ? <p className="note note--ok"><Icon name="check" size={18} /> Está en condiciones de ser adoptada y se muestra en el sitio público.</p>
              : m.estado !== 'Reservada' && (
                <div className="note">
                  <p><strong>Para habilitarla para adopción falta:</strong></p>
                  <ul>{historial.data[1].motivos.map(x => <li key={x}>{x}</li>)}</ul>
                </div>
              )
          )}
          {!adoptada && cuidador(m.cuidadorId) && <p className="muted">Cuidador actual: {cuidador(m.cuidadorId)!.nombre}</p>}
        </section>
      </div>

      <section className="card block" aria-labelledby="s-fotos">
        <div className="card__header">
          <h2 id="s-fotos" className="card__title">Fotos</h2>
          {m.fotos.length > 0 && (
            <button type="button" className="button button--secondary button--small" onClick={() => setVerGaleria(true)}>
              Ver galería como el público
            </button>
          )}
        </div>
        <PhotoManager mascotaId={m.id} nombre={m.nombre} fotos={m.fotos} onChange={recargar} />
      </section>

      <section className="card block" aria-labelledby="s-historial">
        <div className="card__header">
          <h2 id="s-historial" className="card__title">Historial sanitario</h2>
          {!adoptada && (
            <button type="button" className="button button--primary button--small" onClick={() => setNuevoTratamiento(true)}>
              <Icon name="plus" /> Registrar tratamiento
            </button>
          )}
        </div>
        {historial.loading && !historial.data && <Loading texto="Cargando historial…" />}
        {historial.error && <ErrorBox mensaje={historial.error} onRetry={historial.reload} />}
        {historial.data && (
          <>
            <Observaciones historialId={historial.data[0].id} mascotaId={m.id} valor={historial.data[0].observaciones ?? ''} />
            {historial.data[0].tratamientos.length === 0 ? <p className="empty">Todavía no tiene tratamientos registrados.</p> : (
              <ol className="timeline">
                {[...historial.data[0].tratamientos].reverse().map(t => (
                  <li key={t.id} className="timeline__item">
                    <div>
                      <p className="item__title">{etiqueta(t.tipo)} <Badge estado={t.estado} /></p>
                      {t.descripcion && <p>{t.descripcion}</p>}
                      <p className="item__meta">Inicio: {fechaCorta(t.fechaInicio)}{t.fechaFin ? ` · Fin: ${fechaCorta(t.fechaFin)}` : ''}</p>
                    </div>
                    {!adoptada && transicionesTratamiento[t.estado].length > 0 && (
                      <div className="item__actions">
                        {transicionesTratamiento[t.estado].map(e => (
                          <button key={e} type="button" className="button button--secondary button--small"
                            onClick={() => ejecutar(() => api.cambiarEstadoTratamiento(t.id, e), `${etiqueta(t.tipo)}: ${etiqueta(e).toLowerCase()}.`)}>
                            {accionTratamiento[e]}<span className="sr-only"> {etiqueta(t.tipo).toLowerCase()} del {fechaCorta(t.fechaInicio)}</span>
                          </button>
                        ))}
                      </div>
                    )}
                  </li>
                ))}
              </ol>
            )}
          </>
        )}
      </section>

      <section className="card block" aria-labelledby="s-solicitudes">
        <h2 id="s-solicitudes" className="card__title">Solicitudes de adopción</h2>
        {solicitudes.length === 0 ? <p className="empty">No recibió solicitudes.</p> : (
          <ul className="item-list">
            {solicitudes.map(s => (
              <li key={s.id} className="item">
                <div className="item__main">
                  <p className="item__title">#{s.id} · {adoptante(s.adoptanteId)?.nombre ?? `Adoptante #${s.adoptanteId}`}</p>
                  <p className="item__meta">{fechaCorta(s.fechaSolicitud)}</p>
                </div>
                <Badge estado={s.estado} />
              </li>
            ))}
          </ul>
        )}
        <a href="#/solicitudes" className="card__more">Gestionar solicitudes</a>
      </section>

      {editar && (
        <Dialog title={`Editar datos de ${m.nombre}`} onClose={() => setEditar(false)}>
          <MascotaForm inicial={m} onCancel={() => setEditar(false)}
            onSubmit={async d => { await api.actualizarMascota(m.id, d); setEditar(false); await ejecutar(async () => null, 'Datos actualizados.'); }} />
        </Dialog>
      )}
      {verGaleria && (
        <Dialog title={`Fotos de ${m.nombre}`} onClose={() => setVerGaleria(false)} size="lg">
          <Gallery fotos={m.fotos} nombre={m.nombre} />
        </Dialog>
      )}
      {nuevoTratamiento && <TratamientoDialog mascotaId={m.id} onClose={() => setNuevoTratamiento(false)} />}
      {eliminar && (
        <ConfirmDialog titulo={`Eliminar a ${m.nombre}`} confirmar="Sí, eliminar" peligro onClose={() => setEliminar(false)}
          onConfirm={async () => {
            const ok = await ejecutar(() => api.eliminarMascota(m.id), `${m.nombre} fue eliminada.`);
            if (ok) window.location.hash = '#/mascotas';
          }}>
          <p>Usalo sólo si el ingreso se cargó por error. Se eliminan también sus fotos. No se puede deshacer.</p>
        </ConfirmDialog>
      )}
    </>
  );
}

function CambiarEstado({ m, motivos }: { m: Mascota; motivos?: string[] }) {
  const { ejecutar } = useDatos();
  const [valor, setValor] = useState<EstadoMascota | ''>('');
  if (m.estado === 'Reservada') {
    return <p>Está <strong>reservada</strong> por una solicitud aprobada. Para liberarla, cancelá la solicitud; para entregarla, registrá la adopción desde Solicitudes.</p>;
  }
  const opciones = estadosManualesMascota.filter(e => e !== m.estado);
  return (
    <form className="inline-form" onSubmit={async e => {
      e.preventDefault();
      if (!valor) return;
      const ok = await ejecutar(() => api.cambiarEstadoMascota(m.id, valor), `${m.nombre} ahora está: ${etiqueta(valor).toLowerCase()}.`);
      if (ok) setValor('');
    }}>
      <div className="field">
        <label htmlFor="nuevo-estado" className="field__label">Cambiar estado</label>
        <select id="nuevo-estado" className="input input--select" value={valor} onChange={e => setValor(e.target.value as EstadoMascota)}
          aria-describedby={motivos?.length ? 'estado-ayuda' : undefined}>
          <option value="">Elegí el nuevo estado</option>
          {opciones.map(o => <option key={o} value={o}>{etiqueta(o)}</option>)}
        </select>
        <p id="estado-ayuda" className="field__hint">“Reservada” y “Adoptada” se asignan solas durante el proceso de adopción.</p>
      </div>
      <button type="submit" className="button button--secondary" disabled={!valor}>Cambiar</button>
    </form>
  );
}

function AsignarCuidador({ m }: { m: Mascota }) {
  const { datos, ejecutar } = useDatos();
  const [valor, setValor] = useState(m.cuidadorId?.toString() ?? '');
  return (
    <form className="inline-form" onSubmit={async e => {
      e.preventDefault();
      if (valor === '') await ejecutar(() => api.quitarCuidador(m.id), `${m.nombre} quedó sin cuidador asignado.`);
      else await ejecutar(() => api.asignarCuidador(m.id, Number(valor)), `Cuidador asignado a ${m.nombre}.`);
    }}>
      <div className="field">
        <label htmlFor="cuidador" className="field__label">Cuidador responsable</label>
        <select id="cuidador" className="input input--select" value={valor} onChange={e => setValor(e.target.value)}>
          <option value="">Sin asignar</option>
          {datos.cuidadores.map(c => (
            <option key={c.id} value={c.id}>
              {c.nombre} ({c.especializacion ?? 'General'}) · {c.mascotasAsignadas}/{c.capacidadMaxima}{c.disponible ? '' : ' · no disponible'}
            </option>
          ))}
        </select>
      </div>
      <button type="submit" className="button button--secondary" disabled={valor === (m.cuidadorId?.toString() ?? '')}>Guardar</button>
    </form>
  );
}

function Observaciones({ historialId, mascotaId, valor }: { historialId: number; mascotaId: number; valor: string }) {
  const { ejecutar } = useDatos();
  const [texto, setTexto] = useState(valor);
  return (
    <form className="inline-form inline-form--wide" onSubmit={e => {
      e.preventDefault();
      void ejecutar(() => api.actualizarHistorial(historialId, mascotaId, texto), 'Observaciones guardadas.');
    }}>
      <div className="field">
        <label htmlFor="observaciones" className="field__label">Observaciones generales de salud</label>
        <textarea id="observaciones" className="input input--textarea" rows={2} value={texto} onChange={e => setTexto(e.target.value)} />
      </div>
      <button type="submit" className="button button--secondary" disabled={texto === valor}>Guardar observaciones</button>
    </form>
  );
}
