import { useId, useState, type ChangeEvent } from 'react';

type Valores = Record<string, string | boolean>;

/** Estado de un formulario con ids estables por campo (para enlazar el resumen de errores). */
export function useFormulario<T extends Valores>(inicial: T) {
  const prefijo = useId();
  const [valores, setValores] = useState<T>(inicial);
  const [errores, setErrores] = useState<Partial<Record<keyof T, string>>>({});
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const ids = Object.fromEntries(Object.keys(inicial).map(k => [k, `${prefijo}-${k}`])) as Record<keyof T & string, string>;

  const campo = (nombre: keyof T & string) => ({
    id: ids[nombre],
    name: nombre,
    value: String(valores[nombre] ?? ''),
    error: errores[nombre],
    onChange: (e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
      const valor = e.target instanceof HTMLInputElement && e.target.type === 'checkbox' ? e.target.checked : e.target.value;
      setValores(v => ({ ...v, [nombre]: valor }));
      if (errores[nombre]) setErrores(er => ({ ...er, [nombre]: undefined }));
    }
  });

  return { valores, setValores, errores, setErrores, errorGeneral, setErrorGeneral, enviando, setEnviando, ids, campo };
}
