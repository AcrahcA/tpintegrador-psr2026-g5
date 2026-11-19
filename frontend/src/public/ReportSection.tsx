import { useRef, useState } from 'react';
import { api } from '../api/client';
import type { Aviso } from '../api/types';
import { Banner } from '../components/Banner';
import { AvisoForm } from '../components/forms';
import { Icon } from '../components/Icon';

export function ReportSection() {
  const [enviado, setEnviado] = useState<Aviso | null>(null);
  const [clave, setClave] = useState(0);
  const exitoRef = useRef<HTMLDivElement>(null);

  return (
    <section id="reportar" aria-labelledby="reportar-titulo" tabIndex={-1} className="seccion seccion--gris">
      <Banner titulo="Reportá un animal que necesita ayuda" fondo="RESCATE" id="reportar-titulo">
        <p>¿Viste un animal abandonado, perdido, herido o en peligro? Avisanos desde acá mismo.</p>
      </Banner>
      <div className="container container--form">
        <div className="tarjeta-central">
          {enviado ? (
            <div ref={exitoRef} tabIndex={-1} className="success-panel">
              <Icon name="check" size={36} />
              <h3>¡Gracias por avisarnos!</h3>
              <p>Registramos tu aviso con el número <strong>#{enviado.id}</strong>. Si dejaste un teléfono, te vamos a contactar.</p>
              <button type="button" className="button button--secondary" onClick={() => { setEnviado(null); setClave(c => c + 1); }}>
                Hacer otro reporte
              </button>
            </div>
          ) : (
            <>
              <p className="tarjeta-central__titulo">Contanos dónde está y cómo se encuentra</p>
              <div className="callout">
                <Icon name="alert" />
                <p>Si el animal corre peligro inmediato (por ejemplo, está en una ruta), llamá también a los servicios de emergencia de tu municipio.</p>
              </div>
              <AvisoForm key={clave} onSubmit={async datos => {
                const aviso = await api.crearAviso(datos);
                setEnviado(aviso);
                requestAnimationFrame(() => exitoRef.current?.focus());
              }} />
            </>
          )}
        </div>
      </div>
    </section>
  );
}
