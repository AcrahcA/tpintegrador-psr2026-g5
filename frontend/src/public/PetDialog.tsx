import { useRef, useState } from 'react';
import { api } from '../api/client';
import { edad, etiqueta, fecha } from '../api/format';
import type { Mascota, Solicitud } from '../api/types';
import { CheckboxField } from '../components/Fields';
import { Dialog } from '../components/Dialog';
import { AdoptanteForm } from '../components/forms';
import { Gallery } from '../components/Gallery';
import { Icon } from '../components/Icon';
import { useSesion } from '../components/Sesion';

export function PetDialog({ mascota: m, onClose, onSolicitud }: { mascota: Mascota; onClose: () => void; onSolicitud: () => void }) {
  const { sesion, abrirLogin, abrirBuzon } = useSesion();
  const [paso, setPaso] = useState<'ficha' | 'formulario' | 'enviado'>('ficha');
  const [solicitud, setSolicitud] = useState<Solicitud | null>(null);
  const [acepta, setAcepta] = useState(false);
  const [errorAcepta, setErrorAcepta] = useState<string>();
  const exitoRef = useRef<HTMLDivElement>(null);
  const formRef = useRef<HTMLDivElement>(null);

  const irAlFormulario = () => {
    setPaso('formulario');
    requestAnimationFrame(() => formRef.current?.querySelector<HTMLElement>('input')?.focus());
  };

  // Para pedir adoptar hace falta una cuenta: así la respuesta le llega a la persona en su buzón
  const quieroAdoptar = () => {
    if (sesion) { irAlFormulario(); return; }
    abrirLogin({ motivo: `Para enviar una solicitud de adopción iniciá sesión o creá una cuenta. Después seguís con la solicitud para ${m.nombre}.`, alIngresar: irAlFormulario });
  };

  const adoptante = sesion?.adoptante;
  const inicial = sesion ? {
    nombre: adoptante?.nombre ?? sesion.usuario.nombre, dni: adoptante?.dni ?? '',
    telefono: adoptante?.telefono ?? sesion.usuario.telefono, email: adoptante?.email ?? sesion.usuario.email,
    direccion: adoptante?.direccion ?? null
  } : undefined;

  return (
    <Dialog title={paso === 'ficha' ? `Conocé a ${m.nombre}` : `Solicitud para adoptar a ${m.nombre}`} onClose={onClose} size="lg"
      description={paso === 'ficha' ? `${etiqueta(m.especie)} disponible para adopción` : undefined}>
      {paso === 'ficha' && (
        <div className="pet-detail">
          <Gallery fotos={m.fotos} nombre={m.nombre} />
          <div className="pet-detail__info">
            <dl className="facts">
              <div><dt>Especie</dt><dd>{etiqueta(m.especie)}</dd></div>
              <div><dt>Raza</dt><dd>{m.raza ?? 'Sin determinar'}</dd></div>
              <div><dt>Sexo</dt><dd>{etiqueta(m.sexo)}</dd></div>
              <div><dt>Edad aproximada</dt><dd>{edad(m.edadAproximada)}</dd></div>
              <div><dt>Tamaño</dt><dd>{etiqueta(m.tamaño)}</dd></div>
              <div><dt>En el refugio desde</dt><dd>{fecha(m.fechaIngreso)}</dd></div>
            </dl>
            <p className="note"><Icon name="check" size={18} /> Vacunas y controles veterinarios al día.</p>
            <button type="button" className="button button--primary button--block button--large" onClick={quieroAdoptar}>
              <Icon name="heart" /> Quiero adoptar a {m.nombre}
            </button>
            {!sesion && <p className="field__hint">Vas a necesitar iniciar sesión o crear una cuenta.</p>}
          </div>
        </div>
      )}

      {paso === 'formulario' && sesion && (
        <div ref={formRef}>
          <p>Completá tus datos. El equipo del refugio va a evaluar la solicitud y te va a responder en tu <strong>buzón</strong>.</p>
          <AdoptanteForm key={sesion.usuario.id} inicial={inicial} textoEnviar="Enviar solicitud" onCancel={() => setPaso('ficha')}
            onSubmit={async datos => {
              if (!acepta) { setErrorAcepta('Para continuar tenés que aceptar que te contactemos.'); throw new Error('Falta aceptar que te contactemos.'); }
              const s = await api.solicitudPublica(m.id, datos);
              setSolicitud(s);
              setPaso('enviado');
              onSolicitud();
              requestAnimationFrame(() => exitoRef.current?.focus());
            }}>
            <CheckboxField label="Acepto que el refugio use estos datos para contactarme por esta adopción." checked={acepta}
              error={errorAcepta} onChange={e => { setAcepta(e.target.checked); setErrorAcepta(undefined); }} />
          </AdoptanteForm>
        </div>
      )}

      {paso === 'enviado' && solicitud && (
        <div ref={exitoRef} tabIndex={-1} className="success-panel">
          <Icon name="check" size={32} />
          <h3>¡Recibimos tu solicitud!</h3>
          <p>Tu número de solicitud es <strong>#{solicitud.id}</strong>. Cuando el refugio la evalúe, vas a recibir la respuesta en tu buzón.</p>
          <div className="button-row button-row--centro">
            <button type="button" className="button button--secondary" onClick={() => { onClose(); abrirBuzon(); }}><Icon name="mail" /> Ver mi buzón</button>
            <button type="button" className="button button--primary" onClick={onClose}>Volver al listado</button>
          </div>
        </div>
      )}
    </Dialog>
  );
}
