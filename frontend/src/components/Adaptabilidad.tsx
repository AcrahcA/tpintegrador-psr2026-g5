import { useEffect, useId, useState, type ReactNode } from 'react';
import { Badge } from './Badge';
import { Dialog } from './Dialog';
import { Icon } from './Icon';

/**
 * Adaptabilidad: preferencias de visualización de cada persona.
 * Se guardan en el navegador (localStorage) y se aplican como atributos de <html>,
 * que leen los estilos de styles/themes.css. Valen para el sitio público y el panel.
 */

export type Tema = 'predeterminado' | 'alto-contraste' | 'oscuro' | 'rojo-verde' | 'azul-amarillo' | 'grises';
export type TamañoTexto = '100' | '112' | '125' | '150';
export type Preferencias = { tema: Tema; texto: TamañoTexto; espaciado: boolean; movimiento: boolean };

const CLAVE = 'patitas-adaptabilidad';
const PREDETERMINADAS: Preferencias = { tema: 'predeterminado', texto: '100', espaciado: false, movimiento: false };

const TEMAS: { valor: Tema; nombre: string; desc: string; muestra: string[] }[] = [
  { valor: 'predeterminado', nombre: 'Predeterminado', desc: 'Colores originales del sitio.', muestra: ['#1d5fc9', '#ff9600', '#f0f1f5', '#262b30'] },
  { valor: 'alto-contraste', nombre: 'Alto contraste', desc: 'Negro sobre blanco y bordes marcados. Para baja visión.', muestra: ['#000000', '#ffffff', '#0037b3', '#000000'] },
  { valor: 'oscuro', nombre: 'Oscuro', desc: 'Fondo oscuro y texto claro. Para sensibilidad a la luz.', muestra: ['#15181c', '#21252b', '#8ab4ff', '#eef1f5'] },
  { valor: 'rojo-verde', nombre: 'Daltonismo rojo-verde', desc: 'Protanopía y deuteranopía: usa azul y naranja en lugar de verde y rojo.', muestra: ['#0072b2', '#e69f00', '#f0e442', '#56b4e9'] },
  { valor: 'azul-amarillo', nombre: 'Daltonismo azul-amarillo', desc: 'Tritanopía: usa rojo, verde azulado y rosa.', muestra: ['#b0002a', '#00695c', '#ffb3c8', '#4db6ac'] },
  { valor: 'grises', nombre: 'Escala de grises', desc: 'Sin colores. Para acromatopsia o para reducir distracciones.', muestra: ['#333333', '#777777', '#bbbbbb', '#eeeeee'] }
];

const TAMAÑOS: { valor: TamañoTexto; nombre: string }[] = [
  { valor: '100', nombre: 'Normal' }, { valor: '112', nombre: 'Mediano' }, { valor: '125', nombre: 'Grande' }, { valor: '150', nombre: 'Muy grande' }
];

export function leerPreferencias(): Preferencias {
  try {
    const guardadas = JSON.parse(localStorage.getItem(CLAVE) ?? 'null') as Partial<Preferencias> | null;
    return { ...PREDETERMINADAS, ...(guardadas ?? {}) };
  } catch {
    return PREDETERMINADAS;
  }
}

export function aplicarPreferencias(p: Preferencias) {
  const html = document.documentElement;
  const poner = (atributo: string, valor: string | null) => (valor ? html.setAttribute(atributo, valor) : html.removeAttribute(atributo));
  poner('data-tema', p.tema === 'predeterminado' ? null : p.tema);
  poner('data-texto', p.texto === '100' ? null : p.texto);
  poner('data-espaciado', p.espaciado ? 'amplio' : null);
  poner('data-movimiento', p.movimiento ? 'reducido' : null);
}

function guardar(p: Preferencias) {
  aplicarPreferencias(p);
  try { localStorage.setItem(CLAVE, JSON.stringify(p)); } catch { /* sin almacenamiento: se aplica sólo en esta visita */ }
}

/** Botón que abre el panel de adaptabilidad. */
export function BotonAdaptabilidad({ className, children }: { className?: string; children?: ReactNode }) {
  const [abierto, setAbierto] = useState(false);
  return (
    <>
      <button type="button" className={className} aria-haspopup="dialog" onClick={() => setAbierto(true)}>
        {children ?? <><Icon name="settings" /> Adaptabilidad</>}
      </button>
      {abierto && <AdaptabilidadDialog onClose={() => setAbierto(false)} />}
    </>
  );
}

export function AdaptabilidadDialog({ onClose }: { onClose: () => void }) {
  const [p, setP] = useState<Preferencias>(leerPreferencias);
  const [anuncio, setAnuncio] = useState('');
  const id = useId();

  useEffect(() => { guardar(p); }, [p]);

  const cambiar = (cambios: Partial<Preferencias>, mensaje: string) => { setP(x => ({ ...x, ...cambios })); setAnuncio(mensaje); };

  return (
    <Dialog title="Adaptabilidad" size="lg" onClose={onClose}
      description="Ajustá cómo se ve el sitio. Los cambios se aplican al instante y se guardan en este navegador.">
      <div className="adapt">
        <fieldset>
          <legend>Tema de colores</legend>
          <p className="adapt__ayuda">Pensados para baja visión, sensibilidad a la luz y distintos tipos de daltonismo.</p>
          <div className="opciones-tema">
            {TEMAS.map(t => (
              <label key={t.valor} className="opcion">
                <input type="radio" name={`${id}-tema`} value={t.valor} checked={p.tema === t.valor}
                  data-autofocus={p.tema === t.valor ? true : undefined}
                  onChange={() => cambiar({ tema: t.valor }, `Tema aplicado: ${t.nombre}.`)} />
                <span className="opcion__texto">
                  <span className="opcion__nombre">{t.nombre}</span>
                  <span className="opcion__desc">{t.desc}</span>
                  <span className="opcion__muestra" aria-hidden="true">{t.muestra.map((c, i) => <span key={i} style={{ background: c }} />)}</span>
                </span>
              </label>
            ))}
          </div>
        </fieldset>

        <fieldset>
          <legend>Tamaño del texto</legend>
          <p className="adapt__ayuda">Agranda todo el sitio sin que se corte el contenido.</p>
          <div className="opciones-texto">
            {TAMAÑOS.map(t => (
              <label key={t.valor} className="opcion">
                <input type="radio" name={`${id}-texto`} value={t.valor} checked={p.texto === t.valor}
                  onChange={() => cambiar({ texto: t.valor }, `Tamaño del texto: ${t.nombre.toLowerCase()}.`)} />
                <span className="opcion__nombre" style={{ fontSize: `${Number(t.valor) / 100}rem` }}>{t.nombre}</span>
              </label>
            ))}
          </div>
        </fieldset>

        <fieldset>
          <legend>Lectura y movimiento</legend>
          <div className="opciones-tema">
            <label className="opcion">
              <input type="checkbox" checked={p.espaciado}
                onChange={e => cambiar({ espaciado: e.target.checked }, e.target.checked ? 'Más espacio entre líneas activado.' : 'Espaciado normal.')} />
              <span className="opcion__texto">
                <span className="opcion__nombre">Más espacio entre líneas y letras</span>
                <span className="opcion__desc">Facilita la lectura, por ejemplo con dislexia.</span>
              </span>
            </label>
            <label className="opcion">
              <input type="checkbox" checked={p.movimiento}
                onChange={e => cambiar({ movimiento: e.target.checked }, e.target.checked ? 'Animaciones desactivadas.' : 'Animaciones activadas.')} />
              <span className="opcion__texto">
                <span className="opcion__nombre">Reducir animaciones</span>
                <span className="opcion__desc">Quita movimientos y transiciones (por ejemplo, el gato que gira al cargar).</span>
              </span>
            </label>
          </div>
        </fieldset>

        <section aria-labelledby={`${id}-estados`}>
          <h3 id={`${id}-estados`} className="adapt__subtitulo">Etiquetas de estado accesibles</h3>
          <p className="adapt__ayuda">
            En todo el sitio el estado nunca se indica sólo con color: cada etiqueta lleva siempre el texto del estado y un ícono,
            así se entiende igual con cualquier tema o tipo de daltonismo.
          </p>
          <div className="adapt__ejemplos">
            <Badge estado="DisponibleAdopcion" /><Badge estado="Reservada" /><Badge estado="EnTratamiento" />
            <Badge estado="EnRecuperacion" /><Badge estado="EnCurso" /><Badge estado="Cancelada" />
          </div>
        </section>

        <p className="sr-only" role="status" aria-live="polite">{anuncio}</p>

        <div className="adapt__acciones">
          <button type="button" className="button button--ghost" onClick={() => cambiar(PREDETERMINADAS, 'Se restablecieron los ajustes predeterminados.')}>
            <Icon name="refresh" /> Restablecer
          </button>
          <button type="button" className="button button--primary" onClick={onClose}>Listo</button>
        </div>
      </div>
    </Dialog>
  );
}
