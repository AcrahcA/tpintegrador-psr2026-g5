import { useMemo, useState } from 'react';
import { api, fotoSrc } from '../../api/client';
import { edad, etiqueta, fechaCorta, plural } from '../../api/format';
import { ESTADOS_MASCOTA, TIPOS_ANIMAL } from '../../api/types';
import { Badge } from '../../components/Badge';
import { Dialog } from '../../components/Dialog';
import { MascotaForm } from '../../components/forms';
import { Icon } from '../../components/Icon';
import { useDatos } from '../data';
import { Encabezado, Filtros, Miniatura, Tabla } from '../ui';

/** Vista elegida en el panel (tarjetas o lista); se recuerda en este navegador. */
const CLAVE_VISTA = 'patitas-vista-mascotas';

export function Mascotas() {
  const { datos, ejecutar, cuidador } = useDatos();
  const [estado, setEstado] = useState('alojadas');
  const [especie, setEspecie] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [nueva, setNueva] = useState(false);
  const [vista, setVista] = useState<'tarjetas' | 'lista'>(() => {
    try { return localStorage.getItem(CLAVE_VISTA) === 'lista' ? 'lista' : 'tarjetas'; } catch { return 'tarjetas'; }
  });
  const cambiarVista = (v: 'tarjetas' | 'lista') => {
    setVista(v);
    try { localStorage.setItem(CLAVE_VISTA, v); } catch { /* sin almacenamiento: sólo vale en esta visita */ }
  };

  const lista = useMemo(() => {
    const q = busqueda.trim().toLowerCase();
    return datos.mascotas.filter(m =>
      (estado === 'todas' || (estado === 'alojadas' ? m.estado !== 'Adoptada' : m.estado === estado)) &&
      (!especie || m.especie === especie) &&
      (!q || `${m.nombre} ${m.raza ?? ''}`.toLowerCase().includes(q)));
  }, [datos.mascotas, estado, especie, busqueda]);

  return (
    <>
      <Encabezado titulo="Mascotas"
        descripcion={`${datos.capacidad.mascotasActuales} alojadas de ${datos.capacidad.capacidadMaxima} plazas. Las adoptadas se conservan en el historial.`}
        acciones={
          <button type="button" className="button button--primary" onClick={() => setNueva(true)} disabled={!datos.capacidad.tieneLugarDisponible}>
            <Icon name="plus" /> Registrar mascota
          </button>
        } />
      {!datos.capacidad.tieneLugarDisponible && <p className="callout callout--danger"><Icon name="alert" /> El refugio está completo: no se pueden registrar ingresos.</p>}

      <Filtros etiqueta="Filtrar mascotas">
        <div className="field">
          <label htmlFor="m-estado" className="field__label">Estado</label>
          <select id="m-estado" className="input input--select" value={estado} onChange={e => setEstado(e.target.value)}>
            <option value="alojadas">Alojadas (todas menos adoptadas)</option>
            {ESTADOS_MASCOTA.map(s => <option key={s} value={s}>{etiqueta(s)}</option>)}
            <option value="todas">Todas, incluidas adoptadas</option>
          </select>
        </div>
        <div className="field">
          <label htmlFor="m-especie" className="field__label">Especie</label>
          <select id="m-especie" className="input input--select" value={especie} onChange={e => setEspecie(e.target.value)}>
            <option value="">Todas</option>
            {TIPOS_ANIMAL.map(t => <option key={t} value={t}>{plural[t]}</option>)}
          </select>
        </div>
        <div className="field">
          <label htmlFor="m-buscar" className="field__label">Buscar</label>
          <input id="m-buscar" className="input" type="search" value={busqueda} onChange={e => setBusqueda(e.target.value)} placeholder="Nombre o raza" />
        </div>
      </Filtros>
      <div className="vista-barra">
        <p className="results-count" role="status">{lista.length === 1 ? '1 mascota' : `${lista.length} mascotas`}</p>
        <div className="segmented" role="group" aria-label="Forma de ver las mascotas">
          <button type="button" aria-pressed={vista === 'tarjetas'} onClick={() => cambiarVista('tarjetas')}><Icon name="paw" size={18} /> Tarjetas</button>
          <button type="button" aria-pressed={vista === 'lista'} onClick={() => cambiarVista('lista')}><Icon name="list" size={18} /> Lista</button>
        </div>
      </div>

      {vista === 'tarjetas' ? (
        lista.length === 0 ? <p className="empty">No hay mascotas con esos filtros.</p> : (
          <ul className="admin-pets">
            {lista.map(m => {
              const foto = m.fotos[0];
              const c = cuidador(m.cuidadorId);
              return (
                <li key={m.id}>
                  <article className="admin-pet" aria-labelledby={`mascota-${m.id}`}>
                    <div className="admin-pet__media">
                      <img src={foto ? fotoSrc(foto.url) : '/assets/img/sin-foto.svg'} alt="" loading="lazy" width={320} height={240} />
                      <span className="admin-pet__estado"><Badge estado={m.estado} /></span>
                    </div>
                    <div className="admin-pet__body">
                      <h2 id={`mascota-${m.id}`} className="admin-pet__nombre">
                        {/* El enlace cubre toda la tarjeta: se puede tocar en cualquier parte */}
                        <a href={`#/mascotas/${m.id}`} className="admin-pet__enlace">{m.nombre}<span className="sr-only">: ver ficha</span></a>
                      </h2>
                      <p className="admin-pet__datos">{etiqueta(m.especie)} · {etiqueta(m.sexo)} · {edad(m.edadAproximada)} · {etiqueta(m.tamaño)}</p>
                      <dl className="admin-pet__info">
                        <div><dt>Cuidador</dt><dd>{c?.nombre ?? 'Sin asignar'}</dd></div>
                        <div><dt>Ingreso</dt><dd>{fechaCorta(m.fechaIngreso)}</dd></div>
                        <div><dt>Fotos</dt><dd>{m.fotos.length}</dd></div>
                      </dl>
                      <span className="admin-pet__ver" aria-hidden="true">Ver ficha <Icon name="arrow" size={16} /></span>
                    </div>
                  </article>
                </li>
              );
            })}
          </ul>
        )
      ) : (
      <Tabla caption="Mascotas del refugio" vacio="No hay mascotas con esos filtros."
        columnas={[{ titulo: 'Mascota' }, { titulo: 'Datos' }, { titulo: 'Estado' }, { titulo: 'Cuidador' }, { titulo: 'Ingreso' }, { titulo: 'Acciones', className: 'col-actions' }]}
        filas={lista.map(m => ({
          key: m.id,
          celdas: [
            <span className="cell-pet"><Miniatura mascota={m} /> <strong>{m.nombre}</strong></span>,
            `${etiqueta(m.especie)} · ${etiqueta(m.sexo)} · ${edad(m.edadAproximada)} · ${etiqueta(m.tamaño)}`,
            <Badge estado={m.estado} />,
            cuidador(m.cuidadorId)?.nombre ?? <span className="muted">Sin asignar</span>,
            fechaCorta(m.fechaIngreso),
            <a className="button button--secondary button--small" href={`#/mascotas/${m.id}`}>Ver ficha<span className="sr-only"> de {m.nombre}</span></a>
          ]
        }))} />
      )}

      {nueva && (
        <Dialog title="Registrar mascota" description="Ingreso directo al refugio. Queda en estado “Ingresada” con su historial sanitario." onClose={() => setNueva(false)}>
          <MascotaForm onCancel={() => setNueva(false)} textoEnviar="Registrar ingreso"
            onSubmit={async d => {
              const m = await api.crearMascota(d);
              setNueva(false);
              await ejecutar(async () => m, `${m.nombre} fue registrada. Podés cargar sus fotos desde la ficha.`);
              window.location.hash = `#/mascotas/${m.id}`;
            }} />
        </Dialog>
      )}
    </>
  );
}
