import { useEffect, useMemo, useRef, useState } from 'react';
import { api } from '../api/client';
import { etiqueta, plural } from '../api/format';
import { SEXOS, TAMAÑOS, TIPOS_ANIMAL, type Mascota } from '../api/types';
import { Icon } from '../components/Icon';
import { ErrorBox, Loading } from '../components/Status';
import { useAsync } from '../components/useAsync';
import { PetCard } from './PetCard';
import { PetDialog } from './PetDialog';
import { ReportSection } from './ReportSection';

const NAV = [
  { href: '#adoptar', texto: 'Adoptar' },
  { href: '#reportar', texto: 'Reportar un animal' },
  { href: '#como-adoptar', texto: 'Cómo adoptar' },
  { href: '#accesibilidad', texto: 'Accesibilidad' }
];

export function PublicApp() {
  const [menuAbierto, setMenuAbierto] = useState(false);
  const datos = useAsync(async () => {
    const [refugio, mascotas, adopciones] = await Promise.all([
      api.refugio(), api.mascotas('DisponibleAdopcion'), api.adopciones()
    ]);
    return { refugio, mascotas, adopciones: adopciones.length };
  });

  const nombre = datos.data?.refugio.nombre ?? 'Refugio de Mascotas';
  useEffect(() => { document.title = `${nombre} | Rescates y adopciones`; }, [nombre]);

  return (
    <>
      <a href="#contenido" className="skip-link">Saltar al contenido principal</a>
      <header className="site-header">
        <div className="container site-header__inner">
          <a href="#inicio" className="brand">
            <span className="brand__logo" aria-hidden="true"><Icon name="paw" size={26} /></span>
            <span className="brand__name">Patitas<span className="brand__sub">{nombre}</span></span>
          </a>
          <button type="button" className="menu-toggle" aria-expanded={menuAbierto} aria-controls="menu-principal"
            onClick={() => setMenuAbierto(a => !a)}>
            <Icon name={menuAbierto ? 'close' : 'menu'} /> Menú
          </button>
          <nav id="menu-principal" aria-label="Principal" className={`site-nav ${menuAbierto ? 'is-open' : ''}`}>
            <ul>
              {NAV.map(n => <li key={n.href}><a href={n.href} onClick={() => setMenuAbierto(false)}>{n.texto}</a></li>)}
              <li><a href="/admin.html" className="site-nav__admin">Panel del refugio</a></li>
            </ul>
          </nav>
        </div>
      </header>

      <main id="contenido" tabIndex={-1}>
        <section id="inicio" className="hero" aria-labelledby="hero-titulo">
          <div className="container hero__inner">
            <div className="hero__text">
              <p className="eyebrow">Rescate, cuidado y adopción responsable</p>
              <h1 id="hero-titulo">Cada animal rescatado merece una familia</h1>
              <p className="hero__lead">
                En {nombre} rescatamos animales en riesgo, los curamos y los acompañamos hasta encontrar un hogar.
                Podés adoptar o avisarnos si ves un animal que necesita ayuda.
              </p>
              <div className="hero__actions">
                <a href="#adoptar" className="button button--primary button--large"><Icon name="heart" /> Quiero adoptar</a>
                <a href="#reportar" className="button button--secondary button--large"><Icon name="alert" /> Reportar un animal</a>
              </div>
              {datos.data && (
                <ul className="hero__stats" aria-label="El refugio en números">
                  <li><strong>{datos.data.mascotas.length}</strong> buscan hogar</li>
                  <li><strong>{datos.data.adopciones}</strong> ya fueron adoptados</li>
                </ul>
              )}
            </div>
            <img className="hero__image" src="/assets/img/hero.jpg" width={640} height={480}
              alt="Perro de pelo largo color marrón sentado en un parque, mirando hacia adelante." />
          </div>
        </section>

        <section id="adoptar" className="section" aria-labelledby="adoptar-titulo" tabIndex={-1}>
          <div className="container">
            <h2 id="adoptar-titulo">Mascotas que buscan hogar</h2>
            <p className="section__lead">Todas están vacunadas, controladas por un veterinario y sin tratamientos pendientes.</p>
            {datos.loading && !datos.data && <Loading texto="Cargando mascotas disponibles…" />}
            {datos.error && <ErrorBox mensaje={datos.error} onRetry={datos.reload} />}
            {datos.data && <Catalogo mascotas={datos.data.mascotas} onSolicitud={datos.reload} />}
          </div>
        </section>

        <ReportSection />

        <section id="como-adoptar" className="section section--alt" aria-labelledby="como-titulo" tabIndex={-1}>
          <div className="container">
            <h2 id="como-titulo">Cómo adoptar</h2>
            <ol className="steps">
              <li><h3>Elegí</h3><p>Mirá las fotos y la ficha de cada mascota disponible.</p></li>
              <li><h3>Enviá tu solicitud</h3><p>Completá tus datos desde la ficha de la mascota. No hace falta crear una cuenta.</p></li>
              <li><h3>Entrevista</h3><p>El equipo del refugio te contacta para conocerte y evaluar la solicitud.</p></li>
              <li><h3>Adopción</h3><p>Si se aprueba, coordinamos la entrega y la mascota se va a su nuevo hogar.</p></li>
            </ol>
          </div>
        </section>
      </main>

      <footer className="site-footer" id="accesibilidad" tabIndex={-1}>
        <div className="container site-footer__inner">
          <div>
            <h2 className="site-footer__title">Accesibilidad</h2>
            <p>
              Este sitio busca cumplir las Pautas de Accesibilidad para el Contenido Web (WCAG) 2.1 nivel AA,
              según la Ley 26.653 de Accesibilidad de la Información en las Páginas Web. Se puede usar con teclado,
              lectores de pantalla, zoom de hasta el 400 % y desde el celular.
            </p>
            <p>Si encontrás una barrera de accesibilidad, avisanos al equipo del refugio para que podamos corregirla.</p>
          </div>
          <div>
            <h2 className="site-footer__title">{nombre}</h2>
            <p>Rescate, recuperación y adopción responsable de animales.</p>
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
  const resultadosRef = useRef<HTMLParagraphElement>(null);

  const filtradas = useMemo(() => {
    const q = busqueda.trim().toLowerCase();
    return mascotas.filter(m =>
      (!especie || m.especie === especie) && (!tamaño || m.tamaño === tamaño) && (!sexo || m.sexo === sexo) &&
      (!q || `${m.nombre} ${m.raza ?? ''}`.toLowerCase().includes(q)));
  }, [mascotas, busqueda, especie, tamaño, sexo]);

  const hayFiltros = busqueda || especie || tamaño || sexo;

  return (
    <>
      <form className="filters" role="search" aria-label="Filtrar mascotas" onSubmit={e => e.preventDefault()}>
        <div className="field">
          <label htmlFor="f-buscar" className="field__label">Buscar por nombre o raza</label>
          <input id="f-buscar" className="input" type="search" value={busqueda} onChange={e => setBusqueda(e.target.value)} />
        </div>
        <div className="field">
          <label htmlFor="f-especie" className="field__label">Especie</label>
          <select id="f-especie" className="input input--select" value={especie} onChange={e => setEspecie(e.target.value)}>
            <option value="">Todas</option>
            {TIPOS_ANIMAL.map(t => <option key={t} value={t}>{plural[t]}</option>)}
          </select>
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
        {hayFiltros && (
          <button type="button" className="button button--ghost filters__clear"
            onClick={() => { setBusqueda(''); setEspecie(''); setTamaño(''); setSexo(''); }}>
            Limpiar filtros
          </button>
        )}
      </form>

      <p ref={resultadosRef} className="results-count" role="status" aria-live="polite">
        {filtradas.length === 1 ? 'Se encontró 1 mascota.' : `Se encontraron ${filtradas.length} mascotas.`}
      </p>

      {filtradas.length > 0 ? (
        <ul className="pet-grid">
          {filtradas.map(m => <li key={m.id}><PetCard mascota={m} onVer={() => setSeleccionada(m)} /></li>)}
        </ul>
      ) : (
        <p className="empty">
          {mascotas.length === 0 ? 'En este momento no hay mascotas disponibles. ¡Volvé pronto!' : 'No hay mascotas que coincidan con esos filtros.'}
        </p>
      )}

      {seleccionada && <PetDialog mascota={seleccionada} onClose={() => setSeleccionada(null)} onSolicitud={onSolicitud} />}
    </>
  );
}
