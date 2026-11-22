import { useEffect, useId, useRef, useState } from 'react';
import { BotonBuzon } from './Buzon';
import { Icon } from './Icon';
import { useSesion } from './Sesion';

/**
 * Cuenta en el encabezado del sitio:
 * - sin sesión: botón "Iniciar sesión";
 * - con sesión: buzón (con mensajes nuevos) y menú con "Cerrar sesión";
 * - "Panel del refugio" aparece sólo para administradores.
 * El menú es un botón desplegable (aria-expanded), se cierra con Esc o al hacer clic afuera.
 */
export function MenuCuenta() {
  const { sesion, cargando, esAdmin, noLeidas, abrirLogin, abrirBuzon, cerrarSesion } = useSesion();
  const [abierto, setAbierto] = useState(false);
  const id = useId();
  const ref = useRef<HTMLDivElement>(null);
  const botonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!abierto) return;
    const fuera = (e: MouseEvent) => { if (!ref.current?.contains(e.target as Node)) setAbierto(false); };
    const tecla = (e: KeyboardEvent) => { if (e.key === 'Escape') { setAbierto(false); botonRef.current?.focus(); } };
    document.addEventListener('mousedown', fuera);
    document.addEventListener('keydown', tecla);
    return () => { document.removeEventListener('mousedown', fuera); document.removeEventListener('keydown', tecla); };
  }, [abierto]);

  if (cargando) return <div className="cuenta" aria-hidden="true" />;

  if (!sesion) {
    return (
      <div className="cuenta">
        <button type="button" className="button button--primary button--caps" onClick={() => abrirLogin()}>
          <Icon name="user" /> <span className="solo-ancho">Iniciar sesión</span><span className="solo-angosto">Ingresar</span>
        </button>
      </div>
    );
  }

  const nombre = sesion.usuario.nombre.split(' ')[0];
  return (
    <div className="cuenta" ref={ref}>
      <BotonBuzon noLeidas={noLeidas} onClick={abrirBuzon} />
      {esAdmin && <a href="/admin.html" className="button button--primary button--small cuenta__panel">Panel del refugio</a>}
      <div className="cuenta__menu">
        <button ref={botonRef} type="button" className="cuenta__boton" aria-expanded={abierto} aria-controls={`${id}-menu`}
          aria-label={`${nombre}: opciones de tu cuenta`} onClick={() => setAbierto(a => !a)}>
          <Icon name="user" /> <span className="cuenta__nombre">{nombre}</span>
          <Icon name="down" size={16} />
        </button>
        {abierto && (
          <ul id={`${id}-menu`} className="cuenta__lista">
            <li className="cuenta__quien">
              <strong>{sesion.usuario.nombre}</strong>
              <span>{sesion.usuario.email}</span>
              {esAdmin && <span>Administrador del refugio</span>}
            </li>
            <li><button type="button" onClick={() => { setAbierto(false); abrirBuzon(); }}><Icon name="mail" /> Mi buzón</button></li>
            {esAdmin && <li><a href="/admin.html"><Icon name="building" /> Panel del refugio</a></li>}
            <li><button type="button" onClick={() => { setAbierto(false); void cerrarSesion(); }}><Icon name="logout" /> Cerrar sesión</button></li>
          </ul>
        )}
      </div>
    </div>
  );
}
