import type { ReactNode } from 'react';
import { fotoSrc } from '../api/client';
import type { Mascota } from '../api/types';

/** Encabezado de cada vista. El h1 recibe el foco al navegar (lo anuncia el lector de pantalla). */
export function Encabezado({ titulo, descripcion, acciones, volver }: {
  titulo: string; descripcion?: ReactNode; acciones?: ReactNode; volver?: { href: string; texto: string };
}) {
  return (
    <header className="view-header">
      <span className="view-header__fondo" aria-hidden="true">{titulo}</span>
      {volver && <a className="back-link" href={volver.href}>← {volver.texto}</a>}
      <div className="view-header__row">
        <div>
          <h1 id="titulo-vista" tabIndex={-1}>{titulo}</h1>
          {descripcion && <p className="view-header__desc">{descripcion}</p>}
        </div>
        {acciones && <div className="view-header__actions">{acciones}</div>}
      </div>
    </header>
  );
}

export type Columna = { titulo: string; className?: string };
export type Fila = { key: string | number; celdas: ReactNode[] };

/**
 * Tabla de datos. En pantallas angostas cada fila se muestra como tarjeta;
 * los roles ARIA explícitos conservan la semántica de tabla para lectores de pantalla.
 */
export function Tabla({ caption, columnas, filas, vacio }: { caption: string; columnas: Columna[]; filas: Fila[]; vacio: string }) {
  if (filas.length === 0) return <p className="empty">{vacio}</p>;
  return (
    <div className="table-wrap">
      <table className="table" role="table">
        <caption className="sr-only">{caption}</caption>
        <thead role="rowgroup">
          <tr role="row">{columnas.map(c => <th key={c.titulo} role="columnheader" scope="col" className={c.className}>{c.titulo}</th>)}</tr>
        </thead>
        <tbody role="rowgroup">
          {filas.map(f => (
            <tr key={f.key} role="row">
              {f.celdas.map((c, i) => (
                <td key={i} role="cell" data-label={columnas[i].titulo} className={columnas[i].className}>{c}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function Miniatura({ mascota, size = 48 }: { mascota?: Mascota; size?: number }) {
  const foto = mascota?.fotos[0];
  return <img className="thumb" src={foto ? fotoSrc(foto.url) : '/assets/img/sin-foto.svg'} alt="" width={size} height={size} loading="lazy" />;
}

export function EnlaceMascota({ mascota, id }: { mascota?: Mascota; id: number }) {
  return (
    <a className="pet-link" href={`#/mascotas/${id}`}>
      <Miniatura mascota={mascota} size={40} />
      <span>{mascota?.nombre ?? `Mascota #${id}`}</span>
    </a>
  );
}

export function Filtros({ children, etiqueta }: { children: ReactNode; etiqueta: string }) {
  return <form className="filters" role="search" aria-label={etiqueta} onSubmit={e => e.preventDefault()}>{children}</form>;
}

export function Meter({ valor, maximo, etiqueta }: { valor: number; maximo: number; etiqueta: string }) {
  const pct = maximo > 0 ? Math.min(100, Math.round((valor / maximo) * 100)) : 0;
  const tono = pct >= 100 ? 'rojo' : pct >= 80 ? 'ambar' : 'verde';
  return (
    <div className="meter">
      <div className="meter__bar" role="meter" aria-valuemin={0} aria-valuemax={maximo} aria-valuenow={valor}
        aria-valuetext={`${valor} de ${maximo}`} aria-label={etiqueta}>
        <span className={`meter__fill meter__fill--${tono}`} style={{ width: `${pct}%` }} />
      </div>
      <span className="meter__text">{valor} de {maximo}</span>
    </div>
  );
}
