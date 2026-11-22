import { useState } from 'react';
import { api } from '../api/client';
import type { Sesion } from '../api/types';
import { Dialog } from './Dialog';
import { ErrorSummary, TextField, enfocarResumen } from './Fields';
import { Icon } from './Icon';
import { useFormulario } from './useFormulario';

/**
 * Iniciar sesión / Crear cuenta. Desde el sitio sólo se crean cuentas comunes;
 * el rol de administrador se asigna desde la base de datos.
 */
export function LoginDialog({ onClose, onSesion, motivo, soloIngresar }: {
  onClose: () => void; onSesion: (s: Sesion) => void; motivo?: string; soloIngresar?: boolean;
}) {
  const [modo, setModo] = useState<'ingresar' | 'registro'>('ingresar');
  return (
    <Dialog title={modo === 'ingresar' ? 'Iniciar sesión' : 'Crear una cuenta'} onClose={onClose}
      description={motivo ?? (modo === 'ingresar' ? 'Ingresá con tu correo y contraseña.' : 'Con tu cuenta podés pedir adoptar y recibir respuestas en tu buzón.')}>
      {!soloIngresar && (
        <div className="login__modos">
          <div className="pildoras" role="group" aria-label="Elegí una opción">
            <button type="button" aria-pressed={modo === 'ingresar'} onClick={() => setModo('ingresar')}>Ya tengo cuenta</button>
            <button type="button" aria-pressed={modo === 'registro'} onClick={() => setModo('registro')}>Crear cuenta</button>
          </div>
        </div>
      )}
      {modo === 'ingresar'
        ? <FormIngresar key="ingresar" onSesion={onSesion} onCancel={onClose} />
        : <FormRegistro key="registro" onSesion={onSesion} onCancel={onClose} />}
    </Dialog>
  );
}

function AlertaGeneral({ mensaje }: { mensaje: string | null }) {
  return mensaje ? <div className="form-alert" role="alert"><Icon name="alert" /><p>{mensaje}</p></div> : null;
}

function FormIngresar({ onSesion, onCancel }: { onSesion: (s: Sesion) => void; onCancel: () => void }) {
  const f = useFormulario({ email: '', contraseña: '' });
  return (
    <form noValidate onSubmit={async e => {
      e.preventDefault();
      const form = e.currentTarget;
      const er: typeof f.errores = {};
      if (!f.valores.email.trim()) er.email = 'Ingresá tu correo electrónico.';
      if (!f.valores.contraseña) er.contraseña = 'Ingresá tu contraseña.';
      f.setErrores(er); f.setErrorGeneral(null);
      if (Object.keys(er).length) { enfocarResumen(form); return; }
      f.setEnviando(true);
      try { onSesion(await api.login(f.valores.email.trim(), f.valores.contraseña)); }
      catch (err) { f.setErrorGeneral(err instanceof Error ? err.message : 'No se pudo iniciar sesión.'); }
      finally { f.setEnviando(false); }
    }}>
      <ErrorSummary errors={f.errores} ids={f.ids} />
      <AlertaGeneral mensaje={f.errorGeneral} />
      <TextField label="Correo electrónico" required type="email" autoComplete="email" {...f.campo('email')} data-autofocus />
      <TextField label="Contraseña" required type="password" autoComplete="current-password" {...f.campo('contraseña')} />
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onCancel}>Cancelar</button>
        <button type="submit" className="button button--primary" disabled={f.enviando}>{f.enviando ? 'Ingresando…' : 'Iniciar sesión'}</button>
      </div>
    </form>
  );
}

function FormRegistro({ onSesion, onCancel }: { onSesion: (s: Sesion) => void; onCancel: () => void }) {
  const f = useFormulario({ nombre: '', email: '', telefono: '', contraseña: '', repetir: '' });
  const v = f.valores;
  return (
    <form noValidate onSubmit={async e => {
      e.preventDefault();
      const form = e.currentTarget;
      const er: typeof f.errores = {};
      if (v.nombre.trim().length < 2) er.nombre = 'Ingresá tu nombre y apellido.';
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v.email.trim())) er.email = 'Ingresá un correo válido (ejemplo: nombre@correo.com).';
      if (v.telefono && !/^[\d\s()+-]{6,20}$/.test(v.telefono)) er.telefono = 'El teléfono sólo puede tener números, espacios, paréntesis, + o -.';
      if (v.contraseña.length < 8 || !/[a-zA-ZáéíóúñÁÉÍÓÚÑ]/.test(v.contraseña) || !/\d/.test(v.contraseña))
        er.contraseña = 'La contraseña debe tener al menos 8 caracteres, con letras y números.';
      if (v.repetir !== v.contraseña) er.repetir = 'Las contraseñas no coinciden.';
      f.setErrores(er); f.setErrorGeneral(null);
      if (Object.keys(er).length) { enfocarResumen(form); return; }
      f.setEnviando(true);
      try {
        onSesion(await api.registro({ nombre: v.nombre.trim(), email: v.email.trim(), contraseña: v.contraseña, telefono: v.telefono.trim() || null }));
      } catch (err) { f.setErrorGeneral(err instanceof Error ? err.message : 'No se pudo crear la cuenta.'); }
      finally { f.setEnviando(false); }
    }}>
      <ErrorSummary errors={f.errores} ids={f.ids} />
      <AlertaGeneral mensaje={f.errorGeneral} />
      <div className="form-grid">
        <TextField label="Nombre y apellido" required autoComplete="name" {...f.campo('nombre')} data-autofocus />
        <TextField label="Correo electrónico" required type="email" autoComplete="email" {...f.campo('email')} />
        <TextField label="Teléfono" type="tel" inputMode="tel" autoComplete="tel" {...f.campo('telefono')} />
        <TextField label="Contraseña" required type="password" autoComplete="new-password"
          hint="Al menos 8 caracteres, con letras y números." {...f.campo('contraseña')} />
        <TextField label="Repetí la contraseña" required type="password" autoComplete="new-password" {...f.campo('repetir')} />
      </div>
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onCancel}>Cancelar</button>
        <button type="submit" className="button button--primary" disabled={f.enviando}>{f.enviando ? 'Creando…' : 'Crear cuenta'}</button>
      </div>
    </form>
  );
}
