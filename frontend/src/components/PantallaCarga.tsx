import { useEffect, useState } from 'react';
import { GatoAnillo, Logo } from './Ilustraciones';

/**
 * Pantalla de carga a pantalla completa: logo + gato enroscado que gira.
 * - El texto se anuncia a lectores de pantalla (role="status").
 * - Con "reducir movimiento" el gato no gira.
 * - Aparece recién a los 150 ms para no parpadear si la carga es instantánea.
 */
export function PantallaCarga({ texto = 'Cargando…' }: { texto?: string }) {
  const [visible, setVisible] = useState(false);
  useEffect(() => { const t = window.setTimeout(() => setVisible(true), 150); return () => window.clearTimeout(t); }, []);
  return (
    <div className={`pantalla-carga ${visible ? 'is-visible' : ''}`} role="status" aria-live="polite">
      <div className="pantalla-carga__marca">
        <Logo size={64} />
        <span className="pantalla-carga__nombre">Patitas<small>Refugio y adopciones</small></span>
      </div>
      <GatoAnillo size={150} className="pantalla-carga__gato" />
      <p className="pantalla-carga__texto">{texto}</p>
    </div>
  );
}
