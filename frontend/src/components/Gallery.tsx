import { useEffect, useId, useState, type KeyboardEvent } from 'react';
import { fotoSrc } from '../api/client';
import type { Foto } from '../api/types';
import { Icon } from './Icon';

/**
 * Galería de fotos de una mascota.
 * - Cada imagen tiene texto alternativo (la descripción cargada por el refugio).
 * - Se navega con botones, miniaturas o las flechas del teclado.
 * - La posición ("Foto 2 de 5") se anuncia a lectores de pantalla.
 */
export function Gallery({ fotos, nombre }: { fotos: Foto[]; nombre: string }) {
  const [indice, setIndice] = useState(0);
  const tituloId = useId();
  const total = fotos.length;

  useEffect(() => { if (indice >= total) setIndice(Math.max(0, total - 1)); }, [indice, total]);

  if (total === 0) {
    return (
      <div className="gallery gallery--empty">
        <img src="/assets/img/sin-foto.svg" alt="" width={320} height={240} />
        <p>{nombre} todavía no tiene fotos publicadas.</p>
      </div>
    );
  }

  const actual = fotos[Math.min(indice, total - 1)];
  const ir = (i: number) => setIndice((i + total) % total);

  const alTeclado = (e: KeyboardEvent<HTMLElement>) => {
    if (total < 2) return;
    if (e.key === 'ArrowLeft') { e.preventDefault(); ir(indice - 1); }
    if (e.key === 'ArrowRight') { e.preventDefault(); ir(indice + 1); }
    if (e.key === 'Home') { e.preventDefault(); ir(0); }
    if (e.key === 'End') { e.preventDefault(); ir(total - 1); }
  };

  return (
    <section className="gallery" aria-labelledby={tituloId} onKeyDown={alTeclado}>
      <h3 id={tituloId} className="sr-only">Fotos de {nombre}</h3>
      <figure className="gallery__figure">
        <div className="gallery__stage">
          <img key={actual.id} src={fotoSrc(actual.url)} alt={actual.descripcion} className="gallery__image" />
          {total > 1 && (
            <>
              <button type="button" className="gallery__nav gallery__nav--prev" onClick={() => ir(indice - 1)}>
                <Icon name="left" size={24} /><span className="sr-only">Foto anterior</span>
              </button>
              <button type="button" className="gallery__nav gallery__nav--next" onClick={() => ir(indice + 1)}>
                <Icon name="right" size={24} /><span className="sr-only">Foto siguiente</span>
              </button>
            </>
          )}
        </div>
        <figcaption className="gallery__caption">
          <span aria-live="polite" aria-atomic="true" className="gallery__counter">Foto {indice + 1} de {total}</span>
          <span className="gallery__description">{actual.descripcion}</span>
          <a href={fotoSrc(actual.url)} target="_blank" rel="noreferrer" className="link-with-icon">
            Ver en tamaño completo <Icon name="external" size={16} />
            <span className="sr-only"> (se abre en una pestaña nueva)</span>
          </a>
        </figcaption>
      </figure>
      {total > 1 && (
        <>
          <p className="gallery__help">También podés usar las flechas izquierda y derecha del teclado.</p>
          <ul className="gallery__thumbs" aria-label={`Miniaturas de las ${total} fotos`}>
            {fotos.map((f, i) => (
              <li key={f.id}>
                <button type="button" className="gallery__thumb" aria-current={i === indice ? 'true' : undefined} onClick={() => ir(i)}>
                  <img src={fotoSrc(f.url)} alt="" loading="lazy" />
                  <span className="sr-only">Ver foto {i + 1}: {f.descripcion}</span>
                </button>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
