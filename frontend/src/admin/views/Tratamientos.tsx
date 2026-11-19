import { useMemo, useState } from 'react';
import { api } from '../../api/client';
import { accionTratamiento, etiqueta, fechaCorta, transicionesTratamiento } from '../../api/format';
import { ESTADOS_TRATAMIENTO, TIPOS_TRATAMIENTO, type EstadoTratamiento, type TipoTratamiento } from '../../api/types';
import { Badge } from '../../components/Badge';
import { Dialog } from '../../components/Dialog';
import { ErrorSummary, SelectField, TextAreaField, TextField, enfocarResumen } from '../../components/Fields';
import { Icon } from '../../components/Icon';
import { useFormulario } from '../../components/useFormulario';
import { useDatos } from '../data';
import { EnlaceMascota, Encabezado, Filtros, Tabla } from '../ui';

export function Tratamientos() {
  const { datos, ejecutar, mascota } = useDatos();
  const [estado, setEstado] = useState('activos');
  const [nuevo, setNuevo] = useState(false);

  const lista = useMemo(() => datos.tratamientos
    .filter(t => estado === 'todos' || (estado === 'activos' ? t.estado === 'Pendiente' || t.estado === 'EnCurso' : t.estado === estado))
    .sort((a, b) => b.fechaInicio.localeCompare(a.fechaInicio)), [datos.tratamientos, estado]);

  return (
    <>
      <Encabezado titulo="Tratamientos" descripcion="Atenciones veterinarias de todas las mascotas. Cada una queda en su historial sanitario."
        acciones={<button type="button" className="button button--primary" onClick={() => setNuevo(true)}><Icon name="plus" /> Registrar tratamiento</button>} />
      <Filtros etiqueta="Filtrar tratamientos">
        <div className="field">
          <label htmlFor="t-estado" className="field__label">Estado</label>
          <select id="t-estado" className="input input--select" value={estado} onChange={e => setEstado(e.target.value)}>
            <option value="activos">Activos (pendientes o en curso)</option>
            {ESTADOS_TRATAMIENTO.map(s => <option key={s} value={s}>{etiqueta(s)}</option>)}
            <option value="todos">Todos</option>
          </select>
        </div>
      </Filtros>
      <p className="results-count" role="status">{lista.length === 1 ? '1 tratamiento' : `${lista.length} tratamientos`}</p>
      <Tabla caption="Tratamientos" vacio="No hay tratamientos con ese estado."
        columnas={[{ titulo: 'Mascota' }, { titulo: 'Tratamiento' }, { titulo: 'Estado' }, { titulo: 'Fechas' }, { titulo: 'Acciones', className: 'col-actions' }]}
        filas={lista.map(t => {
          const m = mascota(t.mascotaId);
          return {
            key: t.id,
            celdas: [
              <EnlaceMascota mascota={m} id={t.mascotaId} />,
              <><strong>{etiqueta(t.tipo)}</strong>{t.descripcion && <><br /><span className="muted">{t.descripcion}</span></>}</>,
              <Badge estado={t.estado} />,
              <>Inicio: {fechaCorta(t.fechaInicio)}{t.fechaFin && <><br />Fin: {fechaCorta(t.fechaFin)}</>}</>,
              m?.estado === 'Adoptada' || transicionesTratamiento[t.estado].length === 0 ? <span className="muted">Sin acciones</span> : (
                <div className="button-row">
                  {transicionesTratamiento[t.estado].map(e => (
                    <button key={e} type="button" className="button button--secondary button--small"
                      onClick={() => ejecutar(() => api.cambiarEstadoTratamiento(t.id, e), `${etiqueta(t.tipo)} de ${m?.nombre}: ${etiqueta(e).toLowerCase()}.`)}>
                      {accionTratamiento[e]}<span className="sr-only"> {etiqueta(t.tipo).toLowerCase()} de {m?.nombre}</span>
                    </button>
                  ))}
                </div>
              )
            ]
          };
        })} />
      {nuevo && <TratamientoDialog onClose={() => setNuevo(false)} />}
    </>
  );
}

export function TratamientoDialog({ mascotaId, onClose }: { mascotaId?: number; onClose: () => void }) {
  const { datos, ejecutar, mascota } = useDatos();
  const f = useFormulario({ mascotaId: mascotaId?.toString() ?? '', tipo: '', descripcion: '', fechaInicio: new Date().toISOString().slice(0, 10), estado: 'Pendiente' });
  const v = f.valores;
  const candidatas = datos.mascotas.filter(m => m.estado !== 'Adoptada');

  return (
    <Dialog title={mascotaId ? `Registrar tratamiento para ${mascota(mascotaId)?.nombre}` : 'Registrar tratamiento'} onClose={onClose}
      description="Con un tratamiento pendiente o en curso, la mascota pasa a “En tratamiento” y no puede adoptarse.">
      <form noValidate onSubmit={async e => {
        e.preventDefault();
        const form = e.currentTarget;
        const er: typeof f.errores = {};
        if (!v.mascotaId) er.mascotaId = 'Elegí la mascota.';
        if (!v.tipo) er.tipo = 'Elegí el tipo de tratamiento.';
        f.setErrores(er);
        if (Object.keys(er).length) { enfocarResumen(form); return; }
        f.setEnviando(true);
        const ok = await ejecutar(() => api.crearTratamiento(Number(v.mascotaId), {
          tipo: v.tipo as TipoTratamiento, descripcion: v.descripcion.trim() || null,
          fechaInicio: v.fechaInicio ? `${v.fechaInicio}T12:00:00Z` : null, estado: v.estado as EstadoTratamiento
        }), 'Tratamiento registrado en el historial sanitario.');
        f.setEnviando(false);
        if (ok) onClose();
      }}>
        <ErrorSummary errors={f.errores} ids={f.ids} />
        <div className="form-grid">
          {!mascotaId && (
            <SelectField label="Mascota" required placeholder="Elegí una mascota" {...f.campo('mascotaId')} data-autofocus
              options={candidatas.map(m => ({ value: String(m.id), label: `${m.nombre} (${etiqueta(m.especie)}, #${m.id})` }))} />
          )}
          <SelectField label="Tipo" required placeholder="Elegí una opción" {...f.campo('tipo')}
            options={TIPOS_TRATAMIENTO.map(t => ({ value: t, label: etiqueta(t) }))} />
          <SelectField label="Estado inicial" required {...f.campo('estado')}
            hint="Usá “Finalizado” para cargar una vacuna o control que ya se realizó."
            options={(['Pendiente', 'EnCurso', 'Finalizado'] as const).map(t => ({ value: t, label: etiqueta(t) }))} />
          <TextField label="Fecha de inicio" type="date" {...f.campo('fechaInicio')} />
          <TextAreaField label="Indicaciones o descripción" rows={3} className="form-grid__full" {...f.campo('descripcion')} />
        </div>
        <div className="form-actions">
          <button type="button" className="button button--secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" className="button button--primary" disabled={f.enviando}>{f.enviando ? 'Guardando…' : 'Registrar tratamiento'}</button>
        </div>
      </form>
    </Dialog>
  );
}
