import { useEffect, useRef, useState } from 'react';
import { BotonAdaptabilidad } from '../components/Adaptabilidad';
import { Icon, type NombreIcono } from '../components/Icon';
import { Logo } from '../components/Ilustraciones';
import { PantallaCarga } from '../components/PantallaCarga';
import { ErrorBox } from '../components/Status';
import { DataProvider, useCargaAdmin } from './data';
import { Adopciones } from './views/Adopciones';
import { Adoptantes } from './views/Adoptantes';
import { Avisos } from './views/Avisos';
import { Cuidadores } from './views/Cuidadores';
import { FichaMascota } from './views/FichaMascota';
import { Mascotas } from './views/Mascotas';
import { RefugioConfig } from './views/RefugioConfig';
import { Resumen } from './views/Resumen';
import { Solicitudes } from './views/Solicitudes';
import { Tratamientos } from './views/Tratamientos';

type Seccion = { ruta: string; titulo: string; icono: NombreIcono };

const SECCIONES: Seccion[] = [
  { ruta: 'resumen', titulo: 'Resumen', icono: 'home' },
  { ruta: 'mascotas', titulo: 'Mascotas', icono: 'paw' },
  { ruta: 'avisos', titulo: 'Avisos de rescate', icono: 'bell' },
  { ruta: 'tratamientos', titulo: 'Tratamientos', icono: 'med' },
  { ruta: 'cuidadores', titulo: 'Cuidadores', icono: 'users' },
  { ruta: 'adoptantes', titulo: 'Adoptantes', icono: 'users' },
  { ruta: 'solicitudes', titulo: 'Solicitudes', icono: 'file' },
  { ruta: 'adopciones', titulo: 'Adopciones', icono: 'heart' },
  { ruta: 'refugio', titulo: 'Refugio', icono: 'building' }
];

function leerRuta() {
  const partes = window.location.hash.replace(/^#\/?/, '').split('/').filter(Boolean);
  const seccion = SECCIONES.some(s => s.ruta === partes[0]) ? partes[0] : 'resumen';
  const id = partes[1] ? Number(partes[1]) : undefined;
  return { seccion, id: Number.isFinite(id) ? id : undefined };
}

export function AdminApp() {
  const [ruta, setRuta] = useState(leerRuta);
  const [menuAbierto, setMenuAbierto] = useState(false);
  const carga = useCargaAdmin();
  const primeraVez = useRef(true);

  useEffect(() => {
    const alCambiar = () => { setRuta(leerRuta()); setMenuAbierto(false); };
    window.addEventListener('hashchange', alCambiar);
    return () => window.removeEventListener('hashchange', alCambiar);
  }, []);

  const seccion = SECCIONES.find(s => s.ruta === ruta.seccion)!;

  // Título de la página y foco en el encabezado de la vista al navegar (WCAG 2.4.2 y 2.4.3)
  useEffect(() => {
    if (!carga.data) return;
    const h1 = document.getElementById('titulo-vista');
    document.title = `${h1?.textContent ?? seccion.titulo} | Panel del refugio`;
    if (primeraVez.current) { primeraVez.current = false; return; }
    h1?.focus();
    window.scrollTo({ top: 0 });
  }, [ruta, carga.data !== null]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (!menuAbierto) return;
    const alTeclado = (e: KeyboardEvent) => { if (e.key === 'Escape') { setMenuAbierto(false); document.getElementById('boton-menu')?.focus(); } };
    document.addEventListener('keydown', alTeclado);
    return () => document.removeEventListener('keydown', alTeclado);
  }, [menuAbierto]);

  const pendientes = carga.data ? {
    avisos: carga.data.avisos.filter(a => a.estado === 'Reportado' || a.estado === 'EnEvaluacion').length,
    solicitudes: carga.data.solicitudes.filter(s => s.estado === 'Pendiente' || s.estado === 'EnEvaluacion').length
  } : { avisos: 0, solicitudes: 0 };

  return (
    <div className="admin">
      <a href="#contenido" className="skip-link" onClick={e => { e.preventDefault(); document.getElementById('contenido')?.focus(); }}>
        Saltar al contenido principal
      </a>
      <header className="admin-topbar">
        <a href="#/resumen" className="brand brand--admin">
          <Logo size={40} />
          <span className="brand__name">Patitas<span className="brand__sub">Panel del refugio</span></span>
        </a>
        <button id="boton-menu" type="button" className="menu-toggle" aria-expanded={menuAbierto} aria-controls="menu-admin"
          onClick={() => setMenuAbierto(a => !a)}>
          <Icon name={menuAbierto ? 'close' : 'menu'} /> Menú
        </button>
        <a href="/" className="button button--secondary button--small admin-topbar__public">Ver sitio público</a>
      </header>

      <nav id="menu-admin" aria-label="Secciones del panel" className={`admin-nav ${menuAbierto ? 'is-open' : ''}`}>
        <ul>
          {SECCIONES.map(s => {
            const cuenta = s.ruta === 'avisos' ? pendientes.avisos : s.ruta === 'solicitudes' ? pendientes.solicitudes : 0;
            return (
              <li key={s.ruta}>
                <a href={`#/${s.ruta}`} aria-current={s.ruta === ruta.seccion ? 'page' : undefined}>
                  <Icon name={s.icono} /> <span>{s.titulo}</span>
                  {cuenta > 0 && <span className="nav-count">{cuenta}<span className="sr-only"> pendientes</span></span>}
                </a>
              </li>
            );
          })}
          <li className="admin-nav__separador">
            <BotonAdaptabilidad className="admin-nav__boton">
              <Icon name="settings" /> <span>Adaptabilidad</span>
            </BotonAdaptabilidad>
          </li>
        </ul>
      </nav>

      <main id="contenido" className="admin-main" tabIndex={-1}>
        {carga.loading && !carga.data && <PantallaCarga texto="Cargando la información del refugio…" />}
        {carga.error && !carga.data && <ErrorBox mensaje={carga.error} onRetry={carga.reload} />}
        {carga.data && (
          <DataProvider datos={carga.data} recargar={carga.reload}>
            <Vista seccion={ruta.seccion} id={ruta.id} />
          </DataProvider>
        )}
      </main>
    </div>
  );
}

function Vista({ seccion, id }: { seccion: string; id?: number }) {
  switch (seccion) {
    case 'mascotas': return id ? <FichaMascota id={id} /> : <Mascotas />;
    case 'avisos': return <Avisos />;
    case 'tratamientos': return <Tratamientos />;
    case 'cuidadores': return <Cuidadores />;
    case 'adoptantes': return <Adoptantes />;
    case 'solicitudes': return <Solicitudes />;
    case 'adopciones': return <Adopciones />;
    case 'refugio': return <RefugioConfig />;
    default: return <Resumen />;
  }
}
