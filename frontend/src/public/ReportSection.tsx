import { useRef, useState } from 'react';
import { api } from '../api/client';
import type { Aviso } from '../api/types';
import { Banner } from '../components/Banner';
import { AvisoForm } from '../components/forms';
import { Icon } from '../components/Icon';
import { useSesion } from '../components/Sesion';

export function ReportSection() {
  const { sesion, abrirLogin, abrirBuzon } = useSesion();
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
              <p>Registramos tu aviso con el número <strong>#{enviado.id}</strong>.{' '}
                {enviado.tieneCuenta ? 'Vas a recibir la respuesta del refugio en tu buzón.' : 'Si dejaste un teléfono, te vamos a contactar.'}</p>
              <div className="button-row button-row--centro">
                {enviado.tieneCuenta && <button type="button" className="button button--secondary" onClick={abrirBuzon}><Icon name="mail" /> Ver mi buzón</button>}
                <button type="button" className="button button--secondary" onClick={() => { setEnviado(null); setClave(c => c + 1); }}>
                  Hacer otro reporte
                </button>
              </div>
            </div>
          ) : (
            <>
              <p className="tarjeta-central__titulo">Contanos dónde está y cómo se encuentra</p>
              <div className="callout">
                <Icon name="alert" />
                <p>Si el animal corre peligro inmediato (por ejemplo, está en una ruta), llamá también a los servicios de emergencia de tu municipio.</p>
              </div>
              {sesion ? (
                <p className="note note--ok"><Icon name="mail" size={18} /> Reportás como {sesion.usuario.nombre}: la respuesta del refugio te va a llegar a tu buzón.</p>
              ) : (
                <div className="note">
                  <p>No hace falta una cuenta para reportar. Si <button type="button" className="link-boton" onClick={() => abrirLogin()}>iniciás sesión</button>,
                    vas a recibir en tu buzón la respuesta del refugio (por ejemplo, si el aviso se acepta o se descarta y por qué).</p>
                </div>
              )}
              <AvisoForm key={`${clave}-${sesion?.usuario.id ?? 0}`}
                contacto={sesion ? { nombre: sesion.usuario.nombre, telefono: sesion.usuario.telefono } : undefined}
                onSubmit={async datos => {
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
