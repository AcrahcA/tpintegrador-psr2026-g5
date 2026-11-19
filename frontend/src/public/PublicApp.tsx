import { useEffect, useMemo, useState } from 'react';
import { api } from '../api/client';
import { etiqueta, plural } from '../api/format';
import { SEXOS, TAMAÑOS, TIPOS_ANIMAL, type Mascota } from '../api/types';
import { Banner } from '../components/Banner';
import { Icon } from '../components/Icon';
import { Garabatos, GatoSentado, Logo, PerroEchado, PerroSalchicha, PerroSentado, Plato } from '../components/Ilustraciones';
import { PantallaCarga } from '../components/PantallaCarga';
import { ErrorBox } from '../components/Status';
import { useAsync } from '../components/useAsync';
import { PetCard } from './PetCard';
import { PetDialog } from './PetDialog';
import { ReportSection } from './ReportSection';

const NAV = [
  { href: '#inicio', texto: 'Inicio' },
  { href: '#adoptar', texto: 'Adoptar' },
  { href: '#reportar', texto: 'Reportar un animal' },
  { href: '#como-adoptar', texto: 'Cómo adoptar' },
  { href: '#accesibilidad', texto: 'Accesibilidad' }
];

export function PublicApp() {
  const [menuAbierto, setMenuAbierto] = useState(false);
  const [seccion, setSeccion] = useState(window.location.hash || '#inicio');
  const datos = useAsync(async () => {
    const [refugio, mascotas, adopciones] = await Promise.all([api.refugio(), api.mascotas('DisponibleAdopcion'), api.adopciones()]);
    return { refugio, mascotas, adopciones: adopciones.length };
  });

  const nombre = datos.data?.refugio.nombre ?? 'Refugio de Mascotas';
  useEffect(() => { document.title = `${nombre} | Rescates y adopciones`; }, [nombre]);
  useEffect(() => {
    const f = () => setSeccion(window.location.hash || '#inicio');
    window.addEventListener('hashchange', f);
    return () => window.removeEventListener('hashchange', f);
  }, []);

  if (datos.loading && !datos.data) return <PantallaCarga texto="Cargando las mascotas del refugio…" />;

  return (
    <>
      <a href="#contenido" className="skip-link">Saltar al contenido principal</a>
      <header className="site-header">
        <div className="container site-header__inner">
          <a href="#inicio" className="brand">
            <Logo size={46} />
            <span className="brand__name">Patitas<span className="brand__sub">{nombre}</span></span>
          </a>
          <button type="button" className="menu-toggle" aria-expanded={menuAbierto} aria-controls="menu-principal"
            onClick={() => setMenuAbierto(a => !a)}>
            <Icon name={menuAbierto ? 'close' : 'menu'} /> Menú
          </button>
          <nav id="menu-principal" aria-label="Principal" className={`site-nav ${menuAbierto ? 'is-open' : ''}`}>
            <ul>
              {NAV.map(n => (
                <li key={n.href}>
                  <a href={n.href} aria-current={seccion === n.href ? 'location' : undefined} onClick={() => setMenuAbierto(false)}>{n.texto}</a>
                </li>
              ))}
            </ul>
          </nav>
          <a href="/admin.html" className="button button--primary button--caps site-header__cta">Panel del refugio</a>
        </div>
      </header>

      <main id="contenido" tabIndex={-1}>
        <section id="inicio" className="portada" aria-labelledby="portada-titulo">
          <div className="container container--narrow portada__texto">
            <h1 id="portada-titulo">Cada animal rescatado merece una familia</h1>
            <p className="portada__bajada">
              En {nombre} rescatamos animales en riesgo, los curamos y los acompañamos hasta encontrar un hogar.
              Podés adoptar o avisarnos si ves un animal que necesita ayuda.
            </p>
            <div className="portada__acciones">
              <a href="#adoptar" className="button button--naranja button--large"><Icon name="heart" /> Quiero adoptar</a>
              <a href="#reportar" className="button button--secondary button--large"><Icon name="megaphone" /> Reportar un animal</a>
            </div>
            {datos.data && (
              <p className="portada__dato"><strong>{datos.data.adopciones}</strong> {datos.data.adopciones === 1 ? 'animal ya encontró' : 'animales ya encontraron'} su familia gracias a quienes adoptaron.</p>
            )}
          </div>
          <div className="portada__dibujos">
            <PerroSalchicha size={250} className="dibujo dibujo--salchicha" />
            <Plato size={84} className="dibujo dibujo--plato" />
            <GatoSentado size={150} className="dibujo dibujo--gato" />
            <PerroSentado size={190} className="dibujo dibujo--sentado" />
          </div>
        </section>

        <section id="adoptar" aria-labelledby="adoptar-titulo" tabIndex={-1} className="seccion seccion--gris">
          <Banner titulo="Mascotas que buscan hogar" fondo="ADOPTÁ" id="adoptar-titulo">
            <p>Todas están vacunadas, controladas por un veterinario y sin tratamientos pendientes.</p>
          </Banner>
          <div className="container">
            {datos.error && <ErrorBox mensaje={datos.error} onRetry={datos.reload} />}
            {datos.data && <Catalogo mascotas={datos.data.mascotas} onSolicitud={datos.reload} />}
          </div>
        </section>

        <ReportSection />

        <section id="como-adoptar" aria-labelledby="como-titulo" tabIndex={-1} className="seccion seccion--naranja">
          <div className="container dos-tarjetas">
            <div className="tarjeta-dibujo">
              <PerroSentado size={170} className="tarjeta-dibujo__ilustracion" />
              <h2 id="como-titulo" className="titulo-naranja">Cómo adoptar</h2>
              <ol className="pasos">
                <li><strong>Elegí.</strong> Mirá las fotos y la ficha de cada mascota disponible.</li>
                <li><strong>Enviá tu solicitud</strong> desde la ficha. No hace falta crear una cuenta.</li>
                <li><strong>Entrevista.</strong> El equipo del refugio te contacta para conocerte.</li>
                <li><strong>Adopción.</strong> Si se aprueba, coordinamos la entrega a su nuevo hogar.</li>
              </ol>
            </div>
            <div className="tarjeta-dibujo">
              <GatoSentado size={150} className="tarjeta-dibujo__ilustracion" />
              <h2 className="titulo-naranja">¿Qué pasa con tu aviso?</h2>
              <ul className="lista-iconos">
                <li><span className="lista-iconos__icono"><Icon name="bell" /></span><div><strong>Lo recibimos</strong><p>Queda registrado con un número y su nivel de urgencia.</p></div></li>
                <li><span className="lista-iconos__icono"><Icon name="search" /></span><div><strong>Lo evaluamos</strong><p>El equipo analiza la situación y si hay lugar en el refugio.</p></div></li>
                <li><span className="lista-iconos__icono"><Icon name="paw" /></span><div><strong>Rescatamos</strong><p>El animal ingresa, recibe atención veterinaria y, cuando está sano, busca familia.</p></div></li>
              </ul>
            </div>
          </div>
        </section>

        <section className="seccion seccion--crema" aria-labelledby="ayuda-titulo">
          <PerroEchado size={300} className="crema__dibujo crema__dibujo--izq" />
          <Garabatos className="crema__garabatos" />
          <div className="container container--narrow crema__texto">
            <h2 id="ayuda-titulo" className="titulo-naranja">¿Viste un animal que necesita ayuda?</h2>
            <p>Un aviso a tiempo puede cambiarle la vida. Contanos dónde está y cómo se encuentra: lo podés hacer desde el celular, en un minuto.</p>
            <a href="#reportar" className="button button--naranja button--large">Reportar un animal</a>
          </div>
          <div className="crema__dibujo crema__dibujo--der">
            <PerroSalchicha size={240} />
            <Plato size={80} />
          </div>
        </section>
      </main>

      <footer className="site-footer" id="accesibilidad" tabIndex={-1}>
        <div className="container site-footer__inner">
          <div>
            <h2 className="site-footer__title">{nombre}</h2>
            <p className="site-footer__marca"><Logo size={40} /> Patitas</p>
            <p>Rescate, recuperación y adopción responsable de animales.</p>
          </div>
          <div>
            <h2 className="site-footer__title">Accesibilidad</h2>
            <p>
              Este sitio busca cumplir las Pautas de Accesibilidad para el Contenido Web (WCAG) 2.1 nivel AA, según la
              Ley 26.653. Se puede usar con teclado, lectores de pantalla, zoom de hasta el 400 % y desde el celular.
            </p>
            <p>Si encontrás una barrera de accesibilidad, avisanos al equipo del refugio para corregirla.</p>
          </div>
          <div>
            <h2 className="site-footer__title">Equipo del refugio</h2>
            <p><a href="/admin.html">Ingresar al panel del refugio</a></p>
          </div>
        </div>
      </footer>
    </>
  );
}

function Catalogo({ mascotas, onSolicitud }: { mascotas: Mascota[]; onSolicitud: () => void }) {
  const [busqueda, setBusqueda] = useState('');
  const [especie, setEspecie] = useState('');
  const [tamaño, setTamaño] = useState('');
  const [sexo, setSexo] = useState('');
  const [seleccionada, setSeleccionada] = useState<Mascota | null>(null);

  const filtradas = useMemo(() => {
    const q = busqueda.trim().toLowerCase();
    return mascotas.filter(m =>
      (!especie || m.especie === especie) && (!tamaño || m.tamaño === tamaño) && (!sexo || m.sexo === sexo) &&
      (!q || `${m.nombre} ${m.raza ?? ''}`.toLowerCase().includes(q)));
  }, [mascotas, busqueda, especie, tamaño, sexo]);
  const especiesPresentes = TIPOS_ANIMAL.filter(t => mascotas.some(m => m.especie === t));

  return (
    <>
      <form className="tarjeta-filtros" role="search" aria-label="Filtrar mascotas" onSubmit={e => e.preventDefault()}>
        <div className="pildoras" role="group" aria-label="Especie">
          {['', ...especiesPresentes].map(t => (
            <button key={t || 'todas'} type="button" aria-pressed={especie === t} onClick={() => setEspecie(t)}>
              {t ? plural[t] : 'Todas'}
            </button>
          ))}
        </div>
        <div className="tarjeta-filtros__campos">
          <div className="field">
            <label htmlFor="f-buscar" className="field__label">Buscar por nombre o raza</label>
            <input id="f-buscar" className="input" type="search" value={busqueda} onChange={e => setBusqueda(e.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="f-tamano" className="field__label">Tamaño</label>
            <select id="f-tamano" className="input input--select" value={tamaño} onChange={e => setTamaño(e.target.value)}>
              <option value="">Todos</option>
              {TAMAÑOS.map(t => <option key={t} value={t}>{etiqueta(t)}</option>)}
            </select>
          </div>
          <div className="field">
            <label htmlFor="f-sexo" className="field__label">Sexo</label>
            <select id="f-sexo" className="input input--select" value={sexo} onChange={e => setSexo(e.target.value)}>
              <option value="">Todos</option>
              {SEXOS.map(t => <option key={t} value={t}>{etiqueta(t)}</option>)}
            </select>
          </div>
        </div>
      </form>

      <p className="results-count" role="status" aria-live="polite">
        {filtradas.length === 1 ? 'Se encontró 1 mascota.' : `Se encontraron ${filtradas.length} mascotas.`}
        {(busqueda || especie || tamaño || sexo) && (
          <button type="button" className="button button--ghost button--small"
            onClick={() => { setBusqueda(''); setEspecie(''); setTamaño(''); setSexo(''); }}>Limpiar filtros</button>
        )}
      </p>

      {filtradas.length > 0 ? (
        <ul className="pet-grid">
          {filtradas.map(m => <li key={m.id}><PetCard mascota={m} onVer={() => setSeleccionada(m)} /></li>)}
        </ul>
      ) : (
        <p className="empty">{mascotas.length === 0 ? 'En este momento no hay mascotas disponibles. ¡Volvé pronto!' : 'No hay mascotas que coincidan con esos filtros.'}</p>
      )}

      {seleccionada && <PetDialog mascota={seleccionada} onClose={() => setSeleccionada(null)} onSolicitud={onSolicitud} />}
    </>
  );
}
