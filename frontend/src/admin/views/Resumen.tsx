import { api } from '../../api/client';
import { etiqueta, fechaCorta } from '../../api/format';
import { Badge } from '../../components/Badge';
import { Icon } from '../../components/Icon';
import { useDatos } from '../data';
import { EnlaceMascota, Encabezado, Meter } from '../ui';

const ORDEN_URGENCIA = { Alta: 0, Media: 1, Baja: 2 } as const;

export function Resumen() {
  const { datos, ejecutar, mascota, adoptante } = useDatos();
  const { capacidad, mascotas, avisos, solicitudes, tratamientos } = datos;

  const avisosPendientes = avisos
    .filter(a => a.estado === 'Reportado' || a.estado === 'EnEvaluacion')
    .sort((a, b) => ORDEN_URGENCIA[a.nivelUrgencia] - ORDEN_URGENCIA[b.nivelUrgencia]);
  const solicitudesPendientes = solicitudes.filter(s => s.estado === 'Pendiente' || s.estado === 'EnEvaluacion');
  const enTratamiento = mascotas.filter(m => m.estado === 'EnTratamiento');
  const disponibles = mascotas.filter(m => m.estado === 'DisponibleAdopcion').length;
  const activos = tratamientos.filter(t => t.estado === 'Pendiente' || t.estado === 'EnCurso').length;

  return (
    <>
      <Encabezado titulo="Resumen" descripcion={`Estado actual de ${datos.refugio.nombre}.`} />

      <section aria-labelledby="indicadores" className="block">
        <h2 id="indicadores" className="sr-only">Indicadores</h2>
        <ul className="mosaicos mosaicos--kpi">
          <li><a href="#/refugio" className="mosaico mosaico--azul">
            <span className="mosaico__valor">{capacidad.mascotasActuales}<small>/{capacidad.capacidadMaxima}</small></span>
            <span className="mosaico__texto">Ocupación del refugio</span>
            <span className="mosaico__dato">{capacidad.plazasDisponibles} plazas libres</span></a></li>
          <li><a href="#/mascotas" className="mosaico mosaico--verde">
            <span className="mosaico__valor">{disponibles}</span><span className="mosaico__texto">Disponibles para adopción</span></a></li>
          <li><a href="#/avisos" className="mosaico mosaico--amarillo">
            <span className="mosaico__valor">{avisosPendientes.length}</span><span className="mosaico__texto">Avisos sin resolver</span></a></li>
          <li><a href="#/solicitudes" className="mosaico mosaico--coral">
            <span className="mosaico__valor">{solicitudesPendientes.length}</span><span className="mosaico__texto">Solicitudes por revisar</span></a></li>
          <li><a href="#/tratamientos" className="mosaico mosaico--teal">
            <span className="mosaico__valor">{activos}</span><span className="mosaico__texto">Tratamientos activos</span></a></li>
        </ul>
        <div className="card block"><h3 className="card__title">Ocupación</h3>
          <Meter valor={capacidad.mascotasActuales} maximo={capacidad.capacidadMaxima} etiqueta="Ocupación del refugio" /></div>
        {!capacidad.tieneLugarDisponible && (
          <p className="callout callout--danger" role="note"><Icon name="alert" /> El refugio está completo: no se pueden aceptar avisos ni ingresar mascotas.</p>
        )}
      </section>

      <div className="two-columns">
        <section aria-labelledby="t-avisos" className="card">
          <h2 id="t-avisos" className="card__title">Avisos de rescate sin resolver</h2>
          {avisosPendientes.length === 0 ? <p className="empty">No hay avisos pendientes.</p> : (
            <ul className="item-list">
              {avisosPendientes.slice(0, 5).map(a => (
                <li key={a.id} className="item">
                  <div className="item__main">
                    <p className="item__title">Aviso #{a.id}: {etiqueta(a.tipoAnimal)} {etiqueta(a.estadoAparente).toLowerCase()}</p>
                    <p className="item__meta">{a.direccion ?? 'Sin dirección'} · {fechaCorta(a.fechaAviso)}</p>
                    <p className="item__badges"><Badge estado={a.nivelUrgencia}>{`Urgencia ${etiqueta(a.nivelUrgencia).toLowerCase()}`}</Badge> <Badge estado={a.estado} /></p>
                  </div>
                  <div className="item__actions">
                    <button type="button" className="button button--primary button--small"
                      onClick={() => ejecutar(() => api.cambiarEstadoAviso(a.id, 'Aceptado'), `Aviso #${a.id} aceptado.`)}>
                      Aceptar<span className="sr-only"> aviso #{a.id}</span>
                    </button>
                  </div>
                </li>
              ))}
            </ul>
          )}
          <a href="#/avisos" className="card__more">Gestionar avisos</a>
        </section>

        <section aria-labelledby="t-solicitudes" className="card">
          <h2 id="t-solicitudes" className="card__title">Solicitudes por revisar</h2>
          {solicitudesPendientes.length === 0 ? <p className="empty">No hay solicitudes pendientes.</p> : (
            <ul className="item-list">
              {solicitudesPendientes.slice(0, 5).map(s => (
                <li key={s.id} className="item">
                  <div className="item__main">
                    <p className="item__title">{adoptante(s.adoptanteId)?.nombre ?? `Adoptante #${s.adoptanteId}`} quiere adoptar a {mascota(s.mascotaId)?.nombre}</p>
                    <p className="item__meta">Solicitud #{s.id} · {fechaCorta(s.fechaSolicitud)}</p>
                    <p className="item__badges"><Badge estado={s.estado} /></p>
                  </div>
                  <div className="item__actions">
                    <a href="#/solicitudes" className="button button--secondary button--small">Revisar<span className="sr-only"> solicitud #{s.id}</span></a>
                  </div>
                </li>
              ))}
            </ul>
          )}
          <a href="#/solicitudes" className="card__more">Gestionar solicitudes</a>
        </section>
      </div>

      <section aria-labelledby="t-tratamiento" className="card block">
        <h2 id="t-tratamiento" className="card__title">Mascotas en tratamiento</h2>
        {enTratamiento.length === 0 ? <p className="empty">Ninguna mascota está en tratamiento.</p> : (
          <ul className="chips-list">
            {enTratamiento.map(m => <li key={m.id}><EnlaceMascota mascota={m} id={m.id} /></li>)}
          </ul>
        )}
      </section>
    </>
  );
}
