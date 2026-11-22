import { useMemo, useState } from 'react';
import { api } from '../../api/client';
import { accionSolicitud, etiqueta, fechaCorta, transicionesSolicitud } from '../../api/format';
import { ESTADOS_SOLICITUD, type EstadoSolicitud, type Solicitud } from '../../api/types';
import { Badge } from '../../components/Badge';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { MotivoDialog } from '../../components/MotivoDialog';
import { Dialog } from '../../components/Dialog';
import { ErrorSummary, SelectField, TextAreaField, enfocarResumen } from '../../components/Fields';
import { Icon } from '../../components/Icon';
import { useFormulario } from '../../components/useFormulario';
import { useDatos } from '../data';
import { EnlaceMascota, Encabezado, Filtros, Tabla } from '../ui';

export function Solicitudes() {
  const { datos, ejecutar, mascota, adoptante } = useDatos();
  const [estado, setEstado] = useState('pendientes');
  const [nueva, setNueva] = useState(false);
  const [adoptar, setAdoptar] = useState<Solicitud | null>(null);
  const [confirmar, setConfirmar] = useState<{ s: Solicitud; estado: EstadoSolicitud } | null>(null);

  const conAdopcion = new Set(datos.adopciones.map(a => a.solicitudAdopcionId));
  const lista = useMemo(() => datos.solicitudes
    .filter(s => estado === 'todas' || (estado === 'pendientes' ? s.estado === 'Pendiente' || s.estado === 'EnEvaluacion' : s.estado === estado))
    .sort((a, b) => b.fechaSolicitud.localeCompare(a.fechaSolicitud)), [datos.solicitudes, estado]);

  const cambiar = (s: Solicitud, e: EstadoSolicitud) => {
    if (e === 'Rechazada' || e === 'Cancelada' || e === 'Aprobada') { setConfirmar({ s, estado: e }); return; }
    void ejecutar(() => api.cambiarEstadoSolicitud(s.id, e), `Solicitud #${s.id}: ${etiqueta(e).toLowerCase()}.`);
  };

  return (
    <>
      <Encabezado titulo="Solicitudes de adopción"
        descripcion="Al aprobar una solicitud la mascota queda reservada y las demás solicitudes pendientes se rechazan."
        acciones={<button type="button" className="button button--primary" onClick={() => setNueva(true)}><Icon name="plus" /> Nueva solicitud</button>} />
      <Filtros etiqueta="Filtrar solicitudes">
        <div className="field">
          <label htmlFor="s-estado" className="field__label">Estado</label>
          <select id="s-estado" className="input input--select" value={estado} onChange={e => setEstado(e.target.value)}>
            <option value="pendientes">Por revisar (pendientes o en evaluación)</option>
            {ESTADOS_SOLICITUD.map(s => <option key={s} value={s}>{etiqueta(s)}</option>)}
            <option value="todas">Todas</option>
          </select>
        </div>
      </Filtros>
      <p className="results-count" role="status">{lista.length === 1 ? '1 solicitud' : `${lista.length} solicitudes`}</p>
      <Tabla caption="Solicitudes de adopción" vacio="No hay solicitudes con ese estado."
        columnas={[{ titulo: 'Solicitud' }, { titulo: 'Mascota' }, { titulo: 'Adoptante' }, { titulo: 'Estado' }, { titulo: 'Acciones', className: 'col-actions' }]}
        filas={lista.map(s => {
          const a = adoptante(s.adoptanteId);
          const adoptada = conAdopcion.has(s.id);
          return {
            key: s.id,
            celdas: [
              <>#{s.id}<br /><span className="muted">{fechaCorta(s.fechaSolicitud)}</span></>,
              <EnlaceMascota mascota={mascota(s.mascotaId)} id={s.mascotaId} />,
              <>{a?.nombre ?? `#${s.adoptanteId}`}{a?.telefono && <><br /><span className="muted">{a.telefono}</span></>}{a?.tieneCuenta && <><br /><Badge tono="azul">Con cuenta</Badge></>}</>,
              adoptada ? <Badge tono="violeta">Adopción registrada</Badge> : <Badge estado={s.estado} />,
              adoptada ? <span className="muted">Finalizada</span> : (
                <div className="button-row">
                  {s.estado === 'Aprobada' && (
                    <button type="button" className="button button--primary button--small" onClick={() => setAdoptar(s)}>
                      <Icon name="heart" size={16} /> Registrar adopción<span className="sr-only"> de la solicitud #{s.id}</span>
                    </button>
                  )}
                  {transicionesSolicitud[s.estado].map(e => (
                    <button key={e} type="button" onClick={() => cambiar(s, e)}
                      className={`button button--small ${e === 'Aprobada' ? 'button--primary' : e === 'Rechazada' || e === 'Cancelada' ? 'button--ghost-danger' : 'button--secondary'}`}>
                      {accionSolicitud[e]}<span className="sr-only"> la solicitud #{s.id}</span>
                    </button>
                  ))}
                </div>
              )
            ]
          };
        })} />

      {nueva && <NuevaSolicitud onClose={() => setNueva(false)} />}
      {adoptar && <RegistrarAdopcion solicitud={adoptar} onClose={() => setAdoptar(null)} />}
      {confirmar && confirmar.estado === 'Aprobada' && (
        <ConfirmDialog titulo={`Aprobar la solicitud #${confirmar.s.id}`} confirmar="Sí, aprobar" onClose={() => setConfirmar(null)}
          onConfirm={() => ejecutar(() => api.cambiarEstadoSolicitud(confirmar.s.id, 'Aprobada'), `Solicitud #${confirmar.s.id}: aprobada.`)}>
          <p>{mascota(confirmar.s.mascotaId)?.nombre} quedará reservada para {adoptante(confirmar.s.adoptanteId)?.nombre} y las demás solicitudes pendientes sobre esta mascota se rechazarán (y se les avisará).</p>
        </ConfirmDialog>
      )}
      {confirmar && confirmar.estado !== 'Aprobada' && (
        <MotivoDialog titulo={`${accionSolicitud[confirmar.estado]} la solicitud #${confirmar.s.id}`}
          confirmar={`Sí, ${accionSolicitud[confirmar.estado]?.toLowerCase()}`}
          tieneCuenta={adoptante(confirmar.s.adoptanteId)?.tieneCuenta ?? false} onClose={() => setConfirmar(null)}
          onConfirm={motivo => ejecutar(
            () => confirmar.estado === 'Cancelada' ? api.cancelarSolicitud(confirmar.s.id, motivo) : api.rechazarSolicitud(confirmar.s.id, motivo),
            `Solicitud #${confirmar.s.id}: ${etiqueta(confirmar.estado).toLowerCase()}.`)}>
          {confirmar.estado === 'Cancelada'
            ? <p>La solicitud de {adoptante(confirmar.s.adoptanteId)?.nombre} se cancelará. Si estaba aprobada, la mascota vuelve a estar disponible.</p>
            : <p>La solicitud de {adoptante(confirmar.s.adoptanteId)?.nombre} para adoptar a {mascota(confirmar.s.mascotaId)?.nombre} quedará rechazada. Esta acción no se puede revertir.</p>}
        </MotivoDialog>
      )}
    </>
  );
}

function NuevaSolicitud({ onClose }: { onClose: () => void }) {
  const { datos, ejecutar } = useDatos();
  const f = useFormulario({ adoptanteId: '', mascotaId: '' });
  const disponibles = datos.mascotas.filter(m => m.estado === 'DisponibleAdopcion');
  return (
    <Dialog title="Nueva solicitud de adopción" onClose={onClose} description="Sólo se pueden solicitar mascotas disponibles para adopción.">
      <form noValidate onSubmit={async e => {
        e.preventDefault();
        const form = e.currentTarget;
        const er: typeof f.errores = {};
        if (!f.valores.adoptanteId) er.adoptanteId = 'Elegí el adoptante.';
        if (!f.valores.mascotaId) er.mascotaId = 'Elegí la mascota.';
        f.setErrores(er);
        if (Object.keys(er).length) { enfocarResumen(form); return; }
        const ok = await ejecutar(() => api.crearSolicitud(Number(f.valores.adoptanteId), Number(f.valores.mascotaId)), 'Solicitud creada.');
        if (ok) onClose();
      }}>
        <ErrorSummary errors={f.errores} ids={f.ids} />
        <SelectField label="Adoptante" required placeholder="Elegí un adoptante" {...f.campo('adoptanteId')} data-autofocus
          hint={<>¿No está en la lista? Registralo primero en <a href="#/adoptantes">Adoptantes</a>.</>}
          options={datos.adoptantes.map(a => ({ value: String(a.id), label: `${a.nombre} (DNI ${a.dni})` }))} />
        <SelectField label="Mascota" required placeholder="Elegí una mascota" {...f.campo('mascotaId')}
          options={disponibles.map(m => ({ value: String(m.id), label: `${m.nombre} (${etiqueta(m.especie)})` }))} />
        <div className="form-actions">
          <button type="button" className="button button--secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" className="button button--primary">Crear solicitud</button>
        </div>
      </form>
    </Dialog>
  );
}

function RegistrarAdopcion({ solicitud, onClose }: { solicitud: Solicitud; onClose: () => void }) {
  const { ejecutar, mascota, adoptante } = useDatos();
  const [obs, setObs] = useState('');
  const m = mascota(solicitud.mascotaId);
  return (
    <Dialog title={`Registrar la adopción de ${m?.nombre}`} onClose={onClose}
      description={`Adoptante: ${adoptante(solicitud.adoptanteId)?.nombre}. Se verifica que no tenga tratamientos activos y tenga sus vacunas y controles.`}>
      <form onSubmit={async e => {
        e.preventDefault();
        const ok = await ejecutar(() => api.registrarAdopcion(solicitud.id, obs.trim() || null), `¡${m?.nombre} fue adoptada!`);
        if (ok) onClose();
      }}>
        <TextAreaField label="Observaciones de la entrega" rows={3} value={obs} onChange={e => setObs(e.target.value)} data-autofocus />
        <div className="form-actions">
          <button type="button" className="button button--secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" className="button button--primary"><Icon name="heart" /> Confirmar adopción</button>
        </div>
      </form>
    </Dialog>
  );
}
