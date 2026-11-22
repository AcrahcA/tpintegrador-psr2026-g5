import { useEffect, useState } from 'react';
import { Icon } from './Icon';

/**
 * Lectura en voz alta (síntesis de voz del navegador, Web Speech API; no usa servicios externos).
 *
 * Botón fijo en la esquina inferior izquierda. Es el PRIMER elemento al que se llega con Tab, y al recibir el foco
 * dice cómo activarlo, así una persona ciega sin lector de pantalla puede encontrarlo.
 *
 * Con la lectura activada:
 *  - Tab / Mayús+Tab: lee el elemento donde está el foco (tipo, nombre, estado, valor y ayudas) y, al entrar a una
 *    tarjeta, fila de tabla o ventana, también lee lo que está escrito en ella.
 *  - Alt+L: lee toda la sección donde está el usuario.
 *  - Control: hace callar la voz.
 *  - Lee los mensajes que aparecen solos (confirmaciones, errores, resultados de búsqueda).
 *
 * Si la persona ya usa un lector de pantalla (NVDA, JAWS, TalkBack, VoiceOver) conviene dejarla desactivada.
 */

export type PreferenciasVoz = { activa: boolean; velocidad: number };
const CLAVE = 'patitas-voz';
const EVENTO = 'patitas-voz';
const PREDETERMINADAS: PreferenciasVoz = { activa: false, velocidad: 1 };

export const vozDisponible = () =>
  typeof window !== 'undefined' && 'speechSynthesis' in window && 'SpeechSynthesisUtterance' in window;

export function leerVoz(): PreferenciasVoz {
  try { return { ...PREDETERMINADAS, ...(JSON.parse(localStorage.getItem(CLAVE) ?? 'null') ?? {}) }; }
  catch { return PREDETERMINADAS; }
}

export function guardarVoz(p: PreferenciasVoz) {
  try { localStorage.setItem(CLAVE, JSON.stringify(p)); } catch { /* sin almacenamiento: vale sólo en esta visita */ }
  window.dispatchEvent(new CustomEvent<PreferenciasVoz>(EVENTO, { detail: p }));
}

/* ------------------------------------------------------------------ Voz */

let voz: SpeechSynthesisVoice | null = null;
function elegirVoz() {
  const voces = window.speechSynthesis.getVoices();
  const orden = ['es-AR', 'es-419', 'es-US', 'es-MX', 'es-ES'];
  voz = orden.map(l => voces.find(v => v.lang.replace('_', '-') === l)).find(Boolean)
    ?? voces.find(v => v.lang.toLowerCase().startsWith('es')) ?? null;
}

/** Divide el texto en frases cortas: algunos navegadores cortan las lecturas largas. */
function frases(texto: string) {
  const limpio = texto.replace(/\s+/g, ' ').replace(/([.!?:])(\s*\.)+/g, '$1').trim();
  const partes = limpio.split(/(?<=[.!?:;])\s+/);
  const salida: string[] = [];
  for (const p of partes) {
    if (p.length <= 220) { salida.push(p); continue; }
    for (let i = 0; i < p.length; i += 200) salida.push(p.slice(i, i + 200));
  }
  return salida.filter(Boolean);
}

export function decir(texto: string, { interrumpir = true }: { interrumpir?: boolean } = {}) {
  if (!vozDisponible() || !texto.trim()) return;
  const s = window.speechSynthesis;
  if (interrumpir) s.cancel();
  const velocidad = leerVoz().velocidad;
  for (const f of frases(texto)) {
    const u = new SpeechSynthesisUtterance(f);
    u.lang = voz?.lang ?? 'es-AR';
    if (voz) u.voice = voz;
    u.rate = velocidad;
    s.speak(u);
  }
}

export const callar = () => { if (vozDisponible()) window.speechSynthesis.cancel(); };

/* ------------------------------------------------------------------ Qué decir de cada elemento */

const limpiar = (t: string | null | undefined) => (t ?? '').replace(/\s+/g, ' ').replace(/\s+([:,.;])/g, '$1').trim();
const textoDe = (el: Element | null) => limpiar(el instanceof HTMLElement ? el.innerText : el?.textContent);
const porIds = (ids: string | null) =>
  limpiar((ids ?? '').split(/\s+/).map(id => textoDe(document.getElementById(id))).join('. '));

function nombre(el: HTMLElement): string {
  const etiqueta = el.getAttribute('aria-label');
  if (etiqueta) return limpiar(etiqueta);
  const ref = el.getAttribute('aria-labelledby');
  if (ref) return porIds(ref);
  if (el instanceof HTMLInputElement || el instanceof HTMLSelectElement || el instanceof HTMLTextAreaElement) {
    const lbl = el.labels?.[0];
    if (lbl) return textoDe(lbl);
    return limpiar(el.getAttribute('placeholder') ?? el.title);
  }
  if (el instanceof HTMLImageElement) return limpiar(el.alt);
  return textoDe(el) || limpiar(el.title);
}

const ROLES: Record<string, string> = {
  button: 'botón', link: 'enlace', dialog: 'ventana', checkbox: 'casilla', radio: 'opción', tab: 'pestaña',
  menuitem: 'opción de menú', switch: 'interruptor', meter: 'medidor', img: 'imagen', heading: 'encabezado', search: 'búsqueda'
};
const TIPOS_INPUT: Record<string, string> = {
  checkbox: 'casilla', radio: 'opción', search: 'campo de búsqueda', password: 'campo de contraseña', email: 'campo de correo',
  tel: 'campo de teléfono', number: 'campo numérico', file: 'botón para elegir archivos', date: 'campo de fecha',
  'datetime-local': 'campo de fecha y hora', url: 'campo de dirección web', submit: 'botón', button: 'botón'
};

function rol(el: HTMLElement): string {
  const r = el.getAttribute('role');
  if (r && ROLES[r]) return ROLES[r];
  const tag = el.tagName;
  if (tag === 'A') return el.hasAttribute('href') ? 'enlace' : '';
  if (tag === 'BUTTON') return 'botón';
  if (tag === 'SELECT') return 'lista desplegable';
  if (tag === 'TEXTAREA') return 'área de texto';
  if (tag === 'INPUT') return TIPOS_INPUT[(el as HTMLInputElement).type] ?? 'campo de texto';
  if (/^H[1-6]$/.test(tag)) return `encabezado nivel ${tag[1]}`;
  if (tag === 'IMG') return 'imagen';
  if (tag === 'DIALOG') return 'ventana';
  return '';
}

function estados(el: HTMLElement): string[] {
  const e: string[] = [];
  const pressed = el.getAttribute('aria-pressed');
  if (pressed) e.push(pressed === 'true' ? 'presionado' : 'no presionado');
  const expanded = el.getAttribute('aria-expanded');
  if (expanded) e.push(expanded === 'true' ? 'abierto' : 'cerrado');
  if (el instanceof HTMLInputElement && (el.type === 'checkbox' || el.type === 'radio'))
    e.push(el.checked ? (el.type === 'radio' ? 'seleccionada' : 'marcada') : (el.type === 'radio' ? 'no seleccionada' : 'no marcada'));
  if (el.getAttribute('aria-current')) e.push('actual');
  if (el.getAttribute('aria-invalid') === 'true') e.push('con error');
  if ((el as HTMLButtonElement).disabled || el.getAttribute('aria-disabled') === 'true') e.push('no disponible');
  if (el.getAttribute('aria-haspopup') === 'dialog') e.push('abre una ventana');
  if (el instanceof HTMLAnchorElement && el.target === '_blank') e.push('se abre en otra pestaña');
  return e;
}

function valor(el: HTMLElement): string {
  if (el instanceof HTMLSelectElement) return `elegido: ${limpiar(el.selectedOptions[0]?.text) || 'nada'}`;
  if (el instanceof HTMLTextAreaElement) return el.value ? `escrito: ${el.value}` : 'vacío';
  if (el instanceof HTMLInputElement) {
    if (['checkbox', 'radio', 'submit', 'button', 'file'].includes(el.type)) return '';
    if (el.type === 'password') return el.value ? `${el.value.length} caracteres escritos` : 'vacío';
    return el.value ? `escrito: ${el.value}` : 'vacío';
  }
  return limpiar(el.getAttribute('aria-valuetext'));
}

/** Recuerda en qué ventana y en qué tarjeta/fila está el usuario, para leer su contenido sólo al entrar. */
let ventanaActual: Element | null = null;
let bloqueActual: Element | null = null;

function contexto(el: HTMLElement): string {
  const partes: string[] = [];
  const ventana = el.closest('dialog, [role="dialog"]');
  if (ventana !== ventanaActual) {
    ventanaActual = ventana;
    if (ventana) {
      const titulo = porIds(ventana.getAttribute('aria-labelledby')) || limpiar(ventana.getAttribute('aria-label'));
      partes.push(`Ventana: ${titulo}. ${porIds(ventana.getAttribute('aria-describedby'))}`);
    }
  }
  const bloque = el.closest('article, tr, .item, .mensaje');
  if (bloque !== bloqueActual) {
    bloqueActual = bloque;
    // Lo escrito en la tarjeta o fila, sin repetir el texto del propio elemento (se dice después)
    if (bloque && bloque !== el) partes.push(textoDe(bloque).replace(textoDe(el), ' ').replace(/\s+/g, ' ').trim().slice(0, 700));
  }
  return partes.join('. ');
}

/** Tipo, nombre, estado, valor y ayudas del elemento (sin el contexto). */
function elemento(el: HTMLElement): string {
  const partes = [rol(el), nombre(el), ...estados(el), valor(el)].filter(Boolean);
  return [partes.join(', '), porIds(el.getAttribute('aria-describedby'))].filter(Boolean).join('. ');
}

export function describir(el: HTMLElement): string {
  return [contexto(el), elemento(el)].filter(Boolean).join('. ');
}

/** Lee la sección donde está el usuario (Alt+L). */
function leerSeccion() {
  const desde = document.activeElement instanceof HTMLElement && document.activeElement !== document.body
    ? document.activeElement : document.querySelector('main') ?? document.body;
  const seccion = desde.closest('dialog[open], article, section, form, main, header, footer, nav, aside') ?? document.body;
  const titulo = seccion.querySelector('h1, h2, h3');
  decir(`Sección${titulo ? ` ${textoDe(titulo)}` : ''}. ${textoDe(seccion)}`);
}

/* ------------------------------------------------------------------ Botón */

export function LectorVoz() {
  const [pref, setPref] = useState<PreferenciasVoz>(leerVoz);
  const disponible = vozDisponible();

  // Sincroniza con el panel de Adaptabilidad (y otras pestañas)
  useEffect(() => {
    const alCambiar = (e: Event) => setPref((e as CustomEvent<PreferenciasVoz>).detail);
    const alGuardarOtra = (e: StorageEvent) => { if (e.key === CLAVE) setPref(leerVoz()); };
    window.addEventListener(EVENTO, alCambiar);
    window.addEventListener('storage', alGuardarOtra);
    return () => { window.removeEventListener(EVENTO, alCambiar); window.removeEventListener('storage', alGuardarOtra); };
  }, []);

  useEffect(() => {
    if (!disponible) return;
    elegirVoz();
    window.speechSynthesis.addEventListener('voiceschanged', elegirVoz);
    return () => window.speechSynthesis.removeEventListener('voiceschanged', elegirVoz);
  }, [disponible]);

  // Con la lectura activada: leer el foco, los atajos y los mensajes que aparecen solos
  useEffect(() => {
    if (!disponible || !pref.activa) return;
    ventanaActual = null; bloqueActual = null;

    // Si el foco salta varias veces seguidas (por ejemplo, al abrir una ventana) se junta todo y se lee una vez:
    // primero el contexto nuevo (ventana, tarjeta) y después el elemento donde quedó el foco.
    let contextos: string[] = [];
    let ultimo: HTMLElement | null = null;
    let espera = 0;
    const alEnfocar = (e: FocusEvent) => {
      const el = e.target;
      if (!(el instanceof HTMLElement) || el === document.body) return;
      const ctx = contexto(el);
      if (ctx && !contextos.includes(ctx)) contextos.push(ctx);
      ultimo = el;
      window.clearTimeout(espera);
      espera = window.setTimeout(() => {
        if (ultimo) decir([...contextos, elemento(ultimo)].filter(Boolean).join('. '));
        contextos = []; ultimo = null;
      }, 60);
    };
    const alTeclear = (e: KeyboardEvent) => {
      if (e.key === 'Control' && !e.altKey && !e.shiftKey && !e.metaKey) { callar(); return; }
      if (e.altKey && !e.ctrlKey && !e.metaKey && e.key.toLowerCase() === 'l') { e.preventDefault(); leerSeccion(); }
    };
    // Casillas y opciones cambian sin mover el foco: avisar el nuevo estado
    const alCambiar = (e: Event) => {
      const el = e.target;
      if (el instanceof HTMLInputElement && (el.type === 'checkbox' || el.type === 'radio')) decir(estados(el).join(', '));
      else if (el instanceof HTMLSelectElement) decir(valor(el));
    };
    const alClic = (e: MouseEvent) => {
      const el = (e.target as Element | null)?.closest('button[aria-pressed], [aria-expanded]');
      if (el instanceof HTMLElement && e.detail > 0) return; // con el mouse no se repite
      if (el instanceof HTMLElement) setTimeout(() => decir(estados(el).join(', '), { interrumpir: false }), 50);
    };

    // Mensajes de estado (role="status" / "alert" / aria-live)
    const ultimos = new WeakMap<Element, string>();
    let pendientes = new Set<Element>();
    let temporizador = 0;
    const observador = new MutationObserver(lista => {
      for (const m of lista) {
        const nodo = m.target instanceof Element ? m.target : m.target.parentElement;
        const region = nodo?.closest('[aria-live], [role="status"], [role="alert"]');
        if (region && !region.closest('[aria-hidden="true"]')) pendientes.add(region);
      }
      window.clearTimeout(temporizador);
      temporizador = window.setTimeout(() => {
        for (const r of pendientes) {
          const t = textoDe(r);
          if (t && t !== ultimos.get(r)) {
            ultimos.set(r, t);
            decir(t, { interrumpir: r.getAttribute('role') === 'alert' || r.getAttribute('aria-live') === 'assertive' });
          } else if (!t) ultimos.delete(r);
        }
        pendientes = new Set();
      }, 250);
    });
    observador.observe(document.body, { subtree: true, childList: true, characterData: true });

    document.addEventListener('focusin', alEnfocar);
    document.addEventListener('keydown', alTeclear);
    document.addEventListener('change', alCambiar);
    document.addEventListener('click', alClic);
    return () => {
      observador.disconnect(); window.clearTimeout(temporizador); window.clearTimeout(espera);
      document.removeEventListener('focusin', alEnfocar);
      document.removeEventListener('keydown', alTeclear);
      document.removeEventListener('change', alCambiar);
      document.removeEventListener('click', alClic);
    };
  }, [disponible, pref.activa]);

  if (!disponible) return null;

  const alternar = () => {
    const nueva = { ...pref, activa: !pref.activa };
    setPref(nueva);
    guardarVoz(nueva);
    if (nueva.activa) {
      decir('Lectura en voz alta activada. Usá Tab para avanzar y Mayúscula más Tab para volver: voy a leer cada elemento. '
        + 'Con Alt más L leo toda la sección donde estás. Con la tecla Control me callo.');
    } else {
      decir('Lectura en voz alta desactivada.');
    }
  };

  return (
    <button type="button" className="lector-voz" aria-pressed={pref.activa} onClick={alternar}
      title={pref.activa ? 'Desactivar la lectura en voz alta' : 'Activar la lectura en voz alta'}
      onFocus={() => { if (!pref.activa) decir('Botón Leer en voz alta, desactivado. Presioná Enter para que la página te lea en voz alta.'); }}>
      <Icon name={pref.activa ? 'volume' : 'volume-off'} size={24} />
      <span className="lector-voz__texto">Leer en voz alta</span>
    </button>
  );
}
