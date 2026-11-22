import { useRef, useState, type FormEvent, type ReactNode } from 'react';
import { etiqueta } from '../api/format';
import {
  ESTADOS_APARENTES, NIVELES_URGENCIA, SEXOS, TAMAÑOS, TIPOS_ANIMAL,
  type AdoptanteDatos, type AvisoDatos, type EstadoAparente, type MascotaDatos, type NivelUrgencia, type Sexo,
  type Tamaño, type TipoAnimal
} from '../api/types';
import { ErrorSummary, SelectField, TextAreaField, TextField, enfocarResumen } from './Fields';
import { Icon } from './Icon';
import { useFormulario } from './useFormulario';

const opciones = (lista: readonly string[]) => lista.map(v => ({ value: v, label: etiqueta(v) }));

function Acciones({ enviando, textoEnviar, onCancel }: { enviando: boolean; textoEnviar: string; onCancel?: () => void }) {
  return (
    <div className="form-actions">
      {onCancel && <button type="button" className="button button--secondary" onClick={onCancel}>Cancelar</button>}
      <button type="submit" className="button button--primary" disabled={enviando} aria-disabled={enviando}>
        {enviando ? 'Enviando…' : textoEnviar}
      </button>
    </div>
  );
}

function ErrorGeneral({ mensaje }: { mensaje: string | null }) {
  if (!mensaje) return null;
  return <div className="form-alert" role="alert"><Icon name="alert" /><p>{mensaje}</p></div>;
}

async function enviar(
  e: FormEvent<HTMLFormElement>,
  f: { setErrorGeneral: (m: string | null) => void; setEnviando: (b: boolean) => void },
  validar: () => boolean,
  accion: () => Promise<void>
) {
  e.preventDefault();
  const form = e.currentTarget;
  f.setErrorGeneral(null);
  if (!validar()) { enfocarResumen(form); return; }
  f.setEnviando(true);
  try {
    await accion();
  } catch (err) {
    f.setErrorGeneral(err instanceof Error ? err.message : 'No se pudo guardar.');
    requestAnimationFrame(() => form.querySelector<HTMLElement>('.form-alert')?.scrollIntoView({ block: 'center' }));
  } finally {
    f.setEnviando(false);
  }
}

// ------------------------------------------------------------------ Mascota

export function MascotaForm({ inicial, especieFija, onSubmit, onCancel, textoEnviar = 'Guardar mascota' }: {
  inicial?: Partial<MascotaDatos>; especieFija?: boolean; onSubmit: (d: MascotaDatos) => Promise<void>;
  onCancel?: () => void; textoEnviar?: string;
}) {
  const f = useFormulario({
    nombre: inicial?.nombre ?? '', especie: inicial?.especie ?? '', raza: inicial?.raza ?? '',
    edadAproximada: inicial?.edadAproximada?.toString() ?? '', sexo: inicial?.sexo ?? '', tamaño: inicial?.tamaño ?? ''
  });
  const v = f.valores;

  const validar = () => {
    const e: typeof f.errores = {};
    if (!v.nombre.trim()) e.nombre = 'Ingresá el nombre de la mascota.';
    if (!v.especie) e.especie = 'Elegí la especie.';
    const edadNum = Number(v.edadAproximada);
    if (v.edadAproximada === '' || !Number.isInteger(edadNum) || edadNum < 0 || edadNum > 40) e.edadAproximada = 'La edad debe ser un número entero entre 0 y 40.';
    if (!v.sexo) e.sexo = 'Elegí el sexo.';
    if (!v.tamaño) e.tamaño = 'Elegí el tamaño.';
    f.setErrores(e);
    return Object.keys(e).length === 0;
  };

  return (
    <form noValidate onSubmit={e => enviar(e, f, validar, () => onSubmit({
      nombre: v.nombre.trim(), especie: v.especie as TipoAnimal, raza: v.raza.trim() || null,
      edadAproximada: Number(v.edadAproximada), sexo: v.sexo as Sexo, tamaño: v.tamaño as Tamaño
    }))}>
      <ErrorSummary errors={f.errores} ids={f.ids} />
      <ErrorGeneral mensaje={f.errorGeneral} />
      <div className="form-grid">
        <TextField label="Nombre" required autoComplete="off" {...f.campo('nombre')} data-autofocus />
        <SelectField label="Especie" required placeholder="Elegí una opción" options={opciones(TIPOS_ANIMAL)}
          disabled={especieFija} hint={especieFija ? 'Se toma del aviso de rescate.' : undefined} {...f.campo('especie')} />
        <TextField label="Raza" hint="Si no se puede determinar, dejalo vacío." {...f.campo('raza')} />
        <TextField label="Edad aproximada (en años)" required type="number" inputMode="numeric" min={0} max={40}
          hint="Usá 0 si tiene menos de un año." {...f.campo('edadAproximada')} />
        <SelectField label="Sexo" required placeholder="Elegí una opción" options={opciones(SEXOS)} {...f.campo('sexo')} />
        <SelectField label="Tamaño" required placeholder="Elegí una opción" options={opciones(TAMAÑOS)} {...f.campo('tamaño')} />
      </div>
      <Acciones enviando={f.enviando} textoEnviar={textoEnviar} onCancel={onCancel} />
    </form>
  );
}

// ------------------------------------------------------------------ Aviso de rescate (reporte)

type Ubicacion = { lat: number; lng: number; origen: string } | null;

async function geocodificar(direccion: string): Promise<{ lat: number; lng: number } | null> {
  const url = `https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=ar&accept-language=es&q=${encodeURIComponent(direccion)}`;
  try {
    const r = await fetch(url, { headers: { Accept: 'application/json' } });
    if (!r.ok) return null;
    const datos = (await r.json()) as { lat: string; lon: string }[];
    return datos.length ? { lat: Number(datos[0].lat), lng: Number(datos[0].lon) } : null;
  } catch {
    return null;
  }
}

function Radios({ legend, name, valores, valor, onChange, error, id, required }: {
  legend: string; name: string; valores: readonly string[]; valor: string; onChange: (v: string) => void;
  error?: string; id: string; required?: boolean;
}) {
  return (
    <fieldset className="field fieldset" aria-describedby={error ? `${id}-error` : undefined}>
      <legend className="field__label">{legend}{required && <span className="field__required"> (obligatorio)</span>}</legend>
      <div className="radio-row">
        {valores.map((v, i) => (
          <label key={v} className="radio">
            <input type="radio" name={name} value={v} checked={valor === v} onChange={() => onChange(v)}
              id={i === 0 ? id : undefined} aria-invalid={error ? true : undefined} />
            <span>{etiqueta(v)}</span>
          </label>
        ))}
      </div>
      {error && <p id={`${id}-error`} className="field__error"><Icon name="alert" size={16} /> <span>{error}</span></p>}
    </fieldset>
  );
}

export function AvisoForm({ onSubmit, onCancel, textoEnviar = 'Enviar reporte', intro, contacto }: {
  onSubmit: (d: AvisoDatos) => Promise<void>; onCancel?: () => void; textoEnviar?: string; intro?: ReactNode;
  /** Datos de contacto precargados (por ejemplo, de la cuenta con la que se inició sesión). */
  contacto?: { nombre: string; telefono: string | null };
}) {
  const f = useFormulario({
    tipoAnimal: '', estadoAparente: '', nivelUrgencia: '', descripcion: '', direccion: '',
    latitud: '', longitud: '', contactoNombre: contacto?.nombre ?? '', contactoTelefono: contacto?.telefono ?? ''
  });
  const v = f.valores;
  const [ubicacion, setUbicacion] = useState<Ubicacion>(null);
  const [estadoGps, setEstadoGps] = useState('');
  const [buscandoGps, setBuscandoGps] = useState(false);
  const detallesRef = useRef<HTMLDetailsElement>(null);

  const usarMiUbicacion = () => {
    if (!('geolocation' in navigator)) { setEstadoGps('Tu navegador no permite obtener la ubicación. Escribí la dirección.'); return; }
    setBuscandoGps(true);
    setEstadoGps('Buscando tu ubicación…');
    navigator.geolocation.getCurrentPosition(
      pos => {
        const u = { lat: pos.coords.latitude, lng: pos.coords.longitude, origen: 'tu ubicación actual' };
        setUbicacion(u);
        f.setValores(x => ({ ...x, latitud: u.lat.toFixed(6), longitud: u.lng.toFixed(6) }));
        setEstadoGps(`Listo: se usará tu ubicación actual (${u.lat.toFixed(4)}, ${u.lng.toFixed(4)}).`);
        setBuscandoGps(false);
      },
      () => {
        setEstadoGps('No pudimos obtener tu ubicación (permiso denegado o sin señal). Escribí la dirección del lugar.');
        setBuscandoGps(false);
      },
      { enableHighAccuracy: true, timeout: 15000 }
    );
  };

  const validar = () => {
    const e: typeof f.errores = {};
    if (!v.tipoAnimal) e.tipoAnimal = 'Elegí qué animal viste.';
    if (!v.estadoAparente) e.estadoAparente = 'Elegí cómo se ve el animal.';
    if (!v.nivelUrgencia) e.nivelUrgencia = 'Indicá qué tan urgente es.';
    if (v.descripcion.trim().length < 10) e.descripcion = 'Contanos qué viste (al menos 10 caracteres).';
    if (!v.direccion.trim() && !ubicacion && !(v.latitud && v.longitud)) e.direccion = 'Escribí la dirección o usá tu ubicación actual.';
    if (v.latitud || v.longitud) {
      const lat = Number(v.latitud), lng = Number(v.longitud);
      if (!Number.isFinite(lat) || lat < -90 || lat > 90) e.latitud = 'La latitud debe estar entre -90 y 90.';
      if (!Number.isFinite(lng) || lng < -180 || lng > 180) e.longitud = 'La longitud debe estar entre -180 y 180.';
      if (e.latitud || e.longitud) detallesRef.current?.setAttribute('open', '');
    }
    if (!v.contactoNombre.trim()) e.contactoNombre = 'Ingresá tu nombre para que podamos contactarte.';
    if (v.contactoTelefono && !/^[\d\s()+-]{6,20}$/.test(v.contactoTelefono)) e.contactoTelefono = 'El teléfono sólo puede tener números, espacios, paréntesis, + o -.';
    f.setErrores(e);
    return Object.keys(e).length === 0;
  };

  const accion = async () => {
    let lat = v.latitud ? Number(v.latitud) : NaN;
    let lng = v.longitud ? Number(v.longitud) : NaN;
    if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
      const g = await geocodificar(v.direccion.trim());
      if (!g) {
        f.setErrores(er => ({ ...er, direccion: 'No encontramos esa dirección en el mapa. Agregá la localidad, usá tu ubicación actual o ingresá las coordenadas.' }));
        throw new Error('No pudimos ubicar la dirección en el mapa. Revisá el campo "Dirección o referencia del lugar".');
      }
      lat = g.lat; lng = g.lng;
    }
    await onSubmit({
      descripcion: v.descripcion.trim(), tipoAnimal: v.tipoAnimal as TipoAnimal, latitud: lat, longitud: lng,
      direccion: v.direccion.trim() || null, contactoNombre: v.contactoNombre.trim(),
      contactoTelefono: v.contactoTelefono.trim() || null, estadoAparente: v.estadoAparente as EstadoAparente,
      nivelUrgencia: v.nivelUrgencia as NivelUrgencia
    });
  };

  return (
    <form noValidate onSubmit={e => enviar(e, f, validar, accion)}>
      {intro}
      <ErrorSummary errors={f.errores} ids={f.ids} />
      <ErrorGeneral mensaje={f.errorGeneral} />

      <fieldset className="form-section">
        <legend>El animal</legend>
        <div className="form-grid">
          <SelectField label="¿Qué animal viste?" required placeholder="Elegí una opción" options={opciones(TIPOS_ANIMAL)} {...f.campo('tipoAnimal')} />
          <SelectField label="¿Cómo se ve?" required placeholder="Elegí una opción" options={opciones(ESTADOS_APARENTES)} {...f.campo('estadoAparente')} />
        </div>
        <Radios legend="¿Qué tan urgente es?" name="nivelUrgencia" id={f.ids.nivelUrgencia} required valores={NIVELES_URGENCIA}
          valor={v.nivelUrgencia} error={f.errores.nivelUrgencia}
          onChange={x => { f.setValores(s => ({ ...s, nivelUrgencia: x })); f.setErrores(er => ({ ...er, nivelUrgencia: undefined })); }} />
        <TextAreaField label="Describí la situación" required rows={4} maxLength={1000}
          hint="Por ejemplo: color, tamaño, si está lastimado, si se deja agarrar, desde cuándo está ahí." {...f.campo('descripcion')} />
      </fieldset>

      <fieldset className="form-section">
        <legend>¿Dónde está?</legend>
        <TextField label="Dirección o referencia del lugar" required={!ubicacion} autoComplete="street-address"
          hint="Calle y altura, esquina o una referencia, y la localidad." {...f.campo('direccion')} />
        <div className="location-row">
          <button type="button" className="button button--secondary" onClick={usarMiUbicacion} disabled={buscandoGps}>
            <Icon name="locate" /> {buscandoGps ? 'Buscando…' : 'Usar mi ubicación actual'}
          </button>
          <p className="field__hint" role="status" aria-live="polite">{estadoGps}</p>
        </div>
        <details ref={detallesRef} className="details">
          <summary>Ingresar coordenadas manualmente</summary>
          <div className="form-grid">
            <TextField label="Latitud" inputMode="decimal" hint="Ejemplo: -34.6037" {...f.campo('latitud')} />
            <TextField label="Longitud" inputMode="decimal" hint="Ejemplo: -58.3816" {...f.campo('longitud')} />
          </div>
        </details>
      </fieldset>

      <fieldset className="form-section">
        <legend>Tus datos de contacto</legend>
        <p className="field__hint">Los usamos sólo para coordinar el rescate.</p>
        <div className="form-grid">
          <TextField label="Tu nombre" required autoComplete="name" {...f.campo('contactoNombre')} />
          <TextField label="Teléfono" type="tel" autoComplete="tel" inputMode="tel" {...f.campo('contactoTelefono')} />
        </div>
      </fieldset>

      <Acciones enviando={f.enviando} textoEnviar={textoEnviar} onCancel={onCancel} />
    </form>
  );
}

// ------------------------------------------------------------------ Adoptante

export function AdoptanteForm({ inicial, onSubmit, onCancel, textoEnviar = 'Guardar', children }: {
  inicial?: Partial<AdoptanteDatos>; onSubmit: (d: AdoptanteDatos) => Promise<void>; onCancel?: () => void;
  textoEnviar?: string; children?: ReactNode;
}) {
  const f = useFormulario({
    nombre: inicial?.nombre ?? '', dni: inicial?.dni ?? '', telefono: inicial?.telefono ?? '',
    email: inicial?.email ?? '', direccion: inicial?.direccion ?? ''
  });
  const v = f.valores;

  const validar = () => {
    const e: typeof f.errores = {};
    if (!v.nombre.trim()) e.nombre = 'Ingresá el nombre y apellido.';
    const dni = v.dni.replace(/\D/g, '');
    if (dni.length < 6 || dni.length > 9) e.dni = 'El DNI debe tener entre 6 y 9 números (sin puntos).';
    if (!v.telefono.trim() && !v.email.trim()) e.telefono = 'Dejanos al menos un teléfono o un correo electrónico.';
    if (v.telefono && !/^[\d\s()+-]{6,20}$/.test(v.telefono)) e.telefono = 'El teléfono sólo puede tener números, espacios, paréntesis, + o -.';
    if (v.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.email)) e.email = 'El correo electrónico no tiene un formato válido (ejemplo: nombre@correo.com).';
    f.setErrores(e);
    return Object.keys(e).length === 0;
  };

  return (
    <form noValidate onSubmit={e => enviar(e, f, validar, () => onSubmit({
      nombre: v.nombre.trim(), dni: v.dni.replace(/\D/g, ''), telefono: v.telefono.trim() || null,
      email: v.email.trim() || null, direccion: v.direccion.trim() || null
    }))}>
      <ErrorSummary errors={f.errores} ids={f.ids} />
      <ErrorGeneral mensaje={f.errorGeneral} />
      <div className="form-grid">
        <TextField label="Nombre y apellido" required autoComplete="name" {...f.campo('nombre')} data-autofocus />
        <TextField label="DNI" required inputMode="numeric" autoComplete="off" hint="Sólo números, sin puntos." {...f.campo('dni')} />
        <TextField label="Teléfono" type="tel" inputMode="tel" autoComplete="tel" hint="Teléfono o correo: al menos uno." {...f.campo('telefono')} />
        <TextField label="Correo electrónico" type="email" autoComplete="email" {...f.campo('email')} />
        <TextField label="Dirección" autoComplete="street-address" className="form-grid__full" {...f.campo('direccion')} />
      </div>
      {children}
      <Acciones enviando={f.enviando} textoEnviar={textoEnviar} onCancel={onCancel} />
    </form>
  );
}
