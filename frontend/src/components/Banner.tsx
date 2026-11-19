import type { ReactNode } from 'react';

/**
 * Franja con degradé azul → violeta y una palabra gigante de fondo (decorativa, oculta
 * para lectores de pantalla). El título real es un encabezado normal.
 */
export function Banner({ titulo, fondo, nivel = 2, id, children, tabIndex }: {
  titulo: string; fondo: string; nivel?: 1 | 2; id?: string; children?: ReactNode; tabIndex?: number;
}) {
  const H = nivel === 1 ? 'h1' : 'h2';
  return (
    <div className="banner">
      <span className="banner__fondo" aria-hidden="true">{fondo}</span>
      <div className="banner__contenido">
        <H id={id} className="banner__titulo" tabIndex={tabIndex}>{titulo}</H>
        {children}
      </div>
    </div>
  );
}
