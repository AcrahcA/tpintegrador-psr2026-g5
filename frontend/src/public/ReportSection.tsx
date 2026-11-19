import { useRef, useState } from 'react';
import { api } from '../api/client';
import type { Aviso } from '../api/types';
import { AvisoForm } from '../components/forms';
import { Icon } from '../components/Icon';

export function ReportSection() {
  const [enviado, setEnviado] = useState<Aviso | null>(null);
  const [clave, setClave] = useState(0);
  const exitoRef = useRef<HTMLDivElement>(null);

  return (
    <section id="reportar" className="section" aria-labelledby="reportar-titulo" tabIndex={-1}>
      <div className="container container--narrow">
        <h2 id="reportar-titulo">Reportar un animal que necesita ayuda</h2>
        <p className="section__lead">
          ¿Viste un animal abandonado, perdido, herido o en peligro? Contanos dónde está y cómo se encuentra.
          El equipo del refugio evalúa cada aviso según su urgencia.
        </p>
        <div className="callout">
          <Icon name="alert" />
          <p>Si el animal corre peligro inmediato (por ejemplo, está en una ruta), llamá también a los servicios de emergencia de tu municipio.</p>
        </div>

        {enviado ? (
          <div ref={exitoRef} tabIndex={-1} className="success-panel">
            <Icon name="check" size={32} />
            <h3>¡Gracias por avisarnos!</h3>
            <p>Registramos tu aviso con el número <strong>#{enviado.id}</strong>. Si dejaste un teléfono, te vamos a contactar.</p>
            <button type="button" className="button button--secondary" onClick={() => { setEnviado(null); setClave(c => c + 1); }}>
              Hacer otro reporte
            </button>
          </div>
        ) : (
          <div className="card">
            <AvisoForm key={clave} onSubmit={async datos => {
              const aviso = await api.crearAviso(datos);
              setEnviado(aviso);
              requestAnimationFrame(() => exitoRef.current?.focus());
            }} />
          </div>
        )}
      </div>
    </section>
  );
}
