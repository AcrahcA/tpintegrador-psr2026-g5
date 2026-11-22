import { fotoSrc } from '../api/client';
import { edad, etiqueta } from '../api/format';
import type { Mascota } from '../api/types';
import { Icon } from '../components/Icon';

/**
 * Tarjeta de una mascota. Toda la tarjeta se puede tocar: el botón "Conocer a…" extiende su área
 * a toda la tarjeta (sigue siendo un único control para teclado y lectores de pantalla).
 * Al pasar el mouse o enfocarla, la tarjeta "flota" (sin movimiento si se pidió reducir animaciones).
 */
export function PetCard({ mascota: m, onVer }: { mascota: Mascota; onVer: () => void }) {
  const principal = m.fotos[0];
  return (
    <article className="pet-card">
      <div className="pet-card__media">
        <img src={principal ? fotoSrc(principal.url) : '/assets/img/sin-foto.svg'} alt={principal ? principal.descripcion : ''}
          loading="lazy" width={480} height={360} />
        {m.fotos.length > 1 && (
          <span className="pet-card__photos"><Icon name="camera" size={16} /> {m.fotos.length} fotos</span>
        )}
      </div>
      <div className="pet-card__body">
        <h3 className="pet-card__name">{m.nombre}</h3>
        <p className="pet-card__breed">{etiqueta(m.especie)}{m.raza ? ` · ${m.raza}` : ''}</p>
        <ul className="pet-card__facts" aria-label={`Datos de ${m.nombre}`}>
          <li>{etiqueta(m.sexo)}</li>
          <li>{edad(m.edadAproximada)}</li>
          <li>{etiqueta(m.tamaño)}</li>
        </ul>
        <button type="button" className="button button--primary button--block pet-card__accion" onClick={onVer}>
          <Icon name="heart" /> Conocer a {m.nombre}<span className="sr-only">: ver fotos, ficha y solicitar adopción</span>
        </button>
      </div>
    </article>
  );
}
