import { useId, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react';
import { Icon } from './Icon';

type Base = { label: string; hint?: ReactNode; error?: string; required?: boolean; className?: string };

function Etiqueta({ id, label, required }: { id: string; label: string; required?: boolean }) {
  return (
    <label htmlFor={id} className="field__label">
      {label}
      {required ? <span className="field__required"> (obligatorio)</span> : <span className="field__optional"> (opcional)</span>}
    </label>
  );
}

function Ayuda({ hintId, errorId, hint, error }: { hintId: string; errorId: string; hint?: ReactNode; error?: string }) {
  return (
    <>
      {hint && <p id={hintId} className="field__hint">{hint}</p>}
      {error && (
        <p id={errorId} className="field__error">
          <Icon name="alert" size={16} /> <span>{error}</span>
        </p>
      )}
    </>
  );
}

function describedBy(hint: unknown, error: unknown, hintId: string, errorId: string) {
  return [hint ? hintId : null, error ? errorId : null].filter(Boolean).join(' ') || undefined;
}

export function TextField({ label, hint, error, required, className, id: idProp, ...input }:
  Base & Omit<InputHTMLAttributes<HTMLInputElement>, 'required'>) {
  const auto = useId(); const id = idProp ?? auto; const hintId = `${id}-ayuda`; const errorId = `${id}-error`;
  return (
    <div className={`field ${className ?? ''}`}>
      <Etiqueta id={id} label={label} required={required} />
      <input id={id} className="input" aria-required={required || undefined} aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(hint, error, hintId, errorId)} {...input} />
      <Ayuda hintId={hintId} errorId={errorId} hint={hint} error={error} />
    </div>
  );
}

export function TextAreaField({ label, hint, error, required, className, id: idProp, ...input }:
  Base & Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, 'required'>) {
  const auto = useId(); const id = idProp ?? auto; const hintId = `${id}-ayuda`; const errorId = `${id}-error`;
  return (
    <div className={`field ${className ?? ''}`}>
      <Etiqueta id={id} label={label} required={required} />
      <textarea id={id} className="input input--textarea" aria-required={required || undefined} aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(hint, error, hintId, errorId)} {...input} />
      <Ayuda hintId={hintId} errorId={errorId} hint={hint} error={error} />
    </div>
  );
}

export type Opcion = { value: string; label: string };

export function SelectField({ label, hint, error, required, className, options, placeholder, id: idProp, ...select }:
  Base & Omit<SelectHTMLAttributes<HTMLSelectElement>, 'required'> & { options: Opcion[]; placeholder?: string }) {
  const auto = useId(); const id = idProp ?? auto; const hintId = `${id}-ayuda`; const errorId = `${id}-error`;
  return (
    <div className={`field ${className ?? ''}`}>
      <Etiqueta id={id} label={label} required={required} />
      <select id={id} className="input input--select" aria-required={required || undefined} aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(hint, error, hintId, errorId)} {...select}>
        {placeholder !== undefined && <option value="">{placeholder}</option>}
        {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
      <Ayuda hintId={hintId} errorId={errorId} hint={hint} error={error} />
    </div>
  );
}

export function CheckboxField({ label, hint, error, id: idProp, ...input }: Base & InputHTMLAttributes<HTMLInputElement>) {
  const auto = useId(); const id = idProp ?? auto; const hintId = `${id}-ayuda`; const errorId = `${id}-error`;
  return (
    <div className="field field--checkbox">
      <input id={id} type="checkbox" className="checkbox" aria-invalid={error ? true : undefined}
        aria-describedby={describedBy(hint, error, hintId, errorId)} {...input} />
      <label htmlFor={id} className="field__label">{label}</label>
      <Ayuda hintId={hintId} errorId={errorId} hint={hint} error={error} />
    </div>
  );
}

/**
 * Resumen de errores al enviar un formulario (WCAG 3.3.1): recibe el foco,
 * lista los problemas y cada uno lleva al campo correspondiente.
 */
export function ErrorSummary({ errors, ids }: { errors: Record<string, string | undefined>; ids: Record<string, string> }) {
  const lista = Object.entries(errors).filter(([, v]) => v);
  if (lista.length === 0) return null;
  return (
    <div className="error-summary" role="alert" tabIndex={-1} data-error-summary>
      <h3 className="error-summary__title">
        {lista.length === 1 ? 'Hay 1 dato para corregir' : `Hay ${lista.length} datos para corregir`}
      </h3>
      <ul>
        {lista.map(([campo, mensaje]) => (
          <li key={campo}><a href={`#${ids[campo]}`} onClick={e => { e.preventDefault(); document.getElementById(ids[campo])?.focus(); }}>{mensaje}</a></li>
        ))}
      </ul>
    </div>
  );
}

/** Lleva el foco al resumen de errores del formulario después de que se renderiza. */
export function enfocarResumen(form: HTMLFormElement | null) {
  requestAnimationFrame(() => form?.querySelector<HTMLElement>('[data-error-summary]')?.focus());
}
