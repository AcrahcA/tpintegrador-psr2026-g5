import { useState } from 'react';
import { fotoSrc } from '../api/client';
import { edad, etiqueta, fechaCorta } from '../api/format';
import type { Mascota } from '../api/types';
import { Badge } from '../components/Badge';
import { Icon } from '../components/Icon';
import { useDatos } from './data';
import { Miniatura, Tabla } from './ui';

/** Forma de ver las mascotas en el panel. Es la misma elección en todas las vistas y se recuerda en este navegador. */
export type Vista = 'tarjetas' | 'lista';
const CLAVE_VISTA = 'patitas-vista-mascotas';

function leerVista(): Vista {
  try { return localStorage.getItem(CLAVE_VISTA) === 'lista' ? 'lista' : 'tarjetas'; } catch { return 'tarjetas'; }
}

/**
 * Mascotas del panel en tarjetas (grilla) o en lista (tabla), con el selector para cambiar entre las dos.
 * Toda la tarjeta lleva a la ficha de la mascota.
 */
export function ListadoMascotas({ mascotas, caption, vacio, nivelTitulo = 2 }: {
  mascotas: Mascota[]; caption: string; vacio: string; nivelTitulo?: 2 | 3;
}) {
  const [vista, setVista] = useState<Vista>(leerVista);
  const cambiar = (v: Vista) => {
    setVista(v);
    try { localStorage.setItem(CLAVE_VISTA, v); } catch { /* sin almacenamiento: vale sólo en esta visita */ }
  };

  return (
    <>
      <div className="vista-barra">
        <p className="results-count" role="status">{mascotas.length === 1 ? '1 mascota' : `${mascotas.length} mascotas`}</p>
        <div className="segmented" role="group" aria-label={`Forma de ver: ${caption}`}>
          <button type="button" aria-pressed={vista === 'tarjetas'} onClick={() => cambiar('tarjetas')}><Icon name="paw" size={18} /> Tarjetas</button>
          <button type="button" aria-pressed={vista === 'lista'} onClick={() => cambiar('lista')}><Icon name="list" size={18} /> Lista</button>
        </div>
      </div>
      {vista === 'tarjetas'
        ? <TarjetasMascotas mascotas={mascotas} vacio={vacio} nivelTitulo={nivelTitulo} />
        : <TablaMascotas mascotas={mascotas} caption={caption} vacio={vacio} />}
    </>
  );
}

function TarjetasMascotas({ mascotas, vacio, nivelTitulo }: { mascotas: Mascota[]; vacio: string; nivelTitulo: 2 | 3 }) {
  const { cuidador } = useDatos();
  if (mascotas.length === 0) return <p className="empty">{vacio}</p>;
  const Titulo = nivelTitulo === 2 ? 'h2' : 'h3';
  return (
    <ul className="admin-pets">
      {mascotas.map(m => {
        const foto = m.fotos[0];
        return (
          <li key={m.id}>
            <article className="admin-pet" aria-labelledby={`tarjeta-mascota-${m.id}`}>
              <div className="admin-pet__media">
                <img src={foto ? fotoSrc(foto.url) : '/assets/img/sin-foto.svg'} alt="" loading="lazy" width={320} height={240} />
                <span className="admin-pet__estado"><Badge estado={m.estado} /></span>
              </div>
              <div className="admin-pet__body">
                <Titulo id={`tarjeta-mascota-${m.id}`} className="admin-pet__nombre">
                  {/* El enlace cubre toda la tarjeta: se puede tocar en cualquier parte */}
                  <a href={`#/mascotas/${m.id}`} className="admin-pet__enlace">{m.nombre}<span className="sr-only">: ver ficha</span></a>
                </Titulo>
                <p className="admin-pet__datos">{etiqueta(m.especie)} · {etiqueta(m.sexo)} · {edad(m.edadAproximada)} · {etiqueta(m.tamaño)}</p>
                <dl className="admin-pet__info">
                  <div><dt>Cuidador</dt><dd>{cuidador(m.cuidadorId)?.nombre ?? 'Sin asignar'}</dd></div>
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
  );
}

function TablaMascotas({ mascotas, caption, vacio }: { mascotas: Mascota[]; caption: string; vacio: string }) {
  const { cuidador } = useDatos();
  return (
    <Tabla caption={caption} vacio={vacio}
      columnas={[{ titulo: 'Mascota' }, { titulo: 'Datos' }, { titulo: 'Estado' }, { titulo: 'Cuidador' }, { titulo: 'Ingreso' }, { titulo: 'Acciones', className: 'col-actions' }]}
      filas={mascotas.map(m => ({
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
  );
}
