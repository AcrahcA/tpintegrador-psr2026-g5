import { useEffect, useId, useRef, useState } from 'react';
import { api, fotoSrc } from '../api/client';
import type { Foto } from '../api/types';
import { ConfirmDialog } from './ConfirmDialog';
import { Icon } from './Icon';
import { useNotifier } from './Notifier';

const MAXIMO = 12;
const POR_CARGA = 6;
const TIPOS = ['image/jpeg', 'image/png', 'image/webp'];
const TAMAÑO_MAX = 5 * 1024 * 1024;

type Pendiente = { archivo: File; descripcion: string; preview: string; error?: string };

/** Administración de la galería de fotos de una mascota (panel del refugio). */
export function PhotoManager({ mascotaId, nombre, fotos, onChange }: {
  mascotaId: number; nombre: string; fotos: Foto[]; onChange: () => Promise<void> | void;
}) {
  const notifier = useNotifier();
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const [pendientes, setPendientes] = useState<Pendiente[]>([]);
  const [subiendo, setSubiendo] = useState(false);
  const [aEliminar, setAEliminar] = useState<Foto | null>(null);
  const [ocupado, setOcupado] = useState<number | null>(null);
  const restantes = MAXIMO - fotos.length;

  useEffect(() => () => pendientes.forEach(p => URL.revokeObjectURL(p.preview)), [pendientes]);

  const elegir = (lista: FileList | null) => {
    if (!lista) return;
    const nuevos = Array.from(lista).slice(0, Math.min(POR_CARGA, restantes)).map<Pendiente>(archivo => ({
      archivo, descripcion: '', preview: URL.createObjectURL(archivo),
      error: !TIPOS.includes(archivo.type) ? 'Formato no permitido (usá JPG, PNG o WebP).'
        : archivo.size > TAMAÑO_MAX ? 'Pesa más de 5 MB.' : undefined
    }));
    if (lista.length > nuevos.length)
      notifier.error(`Se tomaron ${nuevos.length} de ${lista.length} fotos: se pueden subir hasta ${POR_CARGA} por vez y ${MAXIMO} en total.`);
    setPendientes(nuevos);
  };

  const subir = async () => {
    const validas = pendientes.filter(p => !p.error);
    if (validas.length === 0) return;
    setSubiendo(true);
    try {
      await api.subirFotos(mascotaId, validas.map(p => ({ archivo: p.archivo, descripcion: p.descripcion.trim() })));
      notifier.exito(validas.length === 1 ? 'Se agregó 1 foto.' : `Se agregaron ${validas.length} fotos.`);
      setPendientes([]);
      if (inputRef.current) inputRef.current.value = '';
      await onChange();
    } catch (e) {
      notifier.error(e instanceof Error ? e.message : 'No se pudieron subir las fotos.');
    } finally {
      setSubiendo(false);
    }
  };

  const ejecutar = async (fotoId: number, accion: () => Promise<unknown>, exito: string) => {
    setOcupado(fotoId);
    try { await accion(); notifier.exito(exito); await onChange(); }
    catch (e) { notifier.error(e instanceof Error ? e.message : 'No se pudo completar la acción.'); }
    finally { setOcupado(null); }
  };

  return (
    <div className="photo-manager">
      <p className="photo-manager__count">{fotos.length} de {MAXIMO} fotos. La primera es la foto principal que se ve en el listado.</p>

      {fotos.length > 0 && (
        <ol className="photo-list">
          {fotos.map((f, i) => (
            <FotoItem key={f.id} foto={f} posicion={i + 1} total={fotos.length} nombre={nombre} ocupado={ocupado === f.id}
              onGuardarTexto={texto => ejecutar(f.id, () => api.actualizarFoto(mascotaId, f.id, { descripcion: texto }), 'Descripción de la foto actualizada.')}
              onPrincipal={() => ejecutar(f.id, () => api.actualizarFoto(mascotaId, f.id, { principal: true }), `La foto ${i + 1} ahora es la principal.`)}
              onMover={d => ejecutar(f.id, () => api.moverFoto(mascotaId, f.id, d), `Foto movida a la posición ${i + 1 + d}.`)}
              onEliminar={() => setAEliminar(f)} />
          ))}
        </ol>
      )}

      {restantes > 0 ? (
        <div className="upload">
          <label htmlFor={inputId} className="upload__label">
            <Icon name="camera" size={28} />
            <span><strong>Agregar fotos</strong><br />JPG, PNG o WebP de hasta 5 MB. Hasta {Math.min(POR_CARGA, restantes)} por vez.</span>
          </label>
          <input ref={inputRef} id={inputId} type="file" accept={TIPOS.join(',')} multiple className="upload__input"
            onChange={e => elegir(e.target.files)} />

          {pendientes.length > 0 && (
            <div className="upload__pending">
              <h4>Fotos seleccionadas</h4>
              <p className="field__hint">
                Describí qué se ve en cada foto: ese texto lo escuchan las personas que usan lectores de pantalla.
                Si lo dejás vacío se usará “Foto de {nombre}”.
              </p>
              <ul className="pending-list">
                {pendientes.map((p, i) => (
                  <li key={p.preview} className="pending">
                    <img src={p.preview} alt="" />
                    <div className="field">
                      <label className="field__label" htmlFor={`${inputId}-d${i}`}>Descripción de “{p.archivo.name}” <span className="field__optional">(opcional)</span></label>
                      <input id={`${inputId}-d${i}`} className="input" maxLength={250} value={p.descripcion} disabled={!!p.error}
                        onChange={e => setPendientes(l => l.map((x, j) => (j === i ? { ...x, descripcion: e.target.value } : x)))}
                        placeholder={`Ej.: ${nombre} acostada al sol`} />
                      {p.error && <p className="field__error"><Icon name="alert" size={16} /> <span>{p.error} No se va a subir.</span></p>}
                    </div>
                  </li>
                ))}
              </ul>
              <div className="form-actions">
                <button type="button" className="button button--secondary" onClick={() => { setPendientes([]); if (inputRef.current) inputRef.current.value = ''; }}>
                  Descartar selección
                </button>
                <button type="button" className="button button--primary" onClick={subir} disabled={subiendo || pendientes.every(p => p.error)}>
                  <Icon name="camera" /> {subiendo ? 'Subiendo…' : `Subir ${pendientes.filter(p => !p.error).length} foto(s)`}
                </button>
              </div>
            </div>
          )}
        </div>
      ) : (
        <p className="note">Se alcanzó el máximo de {MAXIMO} fotos. Eliminá alguna para agregar otra.</p>
      )}

      {aEliminar && (
        <ConfirmDialog titulo="Eliminar foto" confirmar="Sí, eliminar foto" peligro onClose={() => setAEliminar(null)}
          onConfirm={() => ejecutar(aEliminar.id, () => api.eliminarFoto(mascotaId, aEliminar.id), 'Foto eliminada.')}>
          <img src={fotoSrc(aEliminar.url)} alt={aEliminar.descripcion} className="confirm__image" />
          <p>¿Querés eliminar esta foto de {nombre}? No se puede deshacer.</p>
        </ConfirmDialog>
      )}
    </div>
  );
}

function FotoItem({ foto, posicion, total, nombre, ocupado, onGuardarTexto, onPrincipal, onMover, onEliminar }: {
  foto: Foto; posicion: number; total: number; nombre: string; ocupado: boolean;
  onGuardarTexto: (t: string) => void; onPrincipal: () => void; onMover: (d: -1 | 1) => void; onEliminar: () => void;
}) {
  const [texto, setTexto] = useState(foto.descripcion);
  const id = useId();
  useEffect(() => setTexto(foto.descripcion), [foto.descripcion]);
  const etiquetaFoto = `foto ${posicion} de ${nombre}`;

  return (
    <li className="photo-item" aria-busy={ocupado || undefined}>
      <div className="photo-item__media">
        <img src={fotoSrc(foto.url)} alt={foto.descripcion} loading="lazy" />
        {posicion === 1 && <span className="badge badge--verde photo-item__badge"><Icon name="star" size={14} /> Principal</span>}
      </div>
      <div className="photo-item__body">
        <form className="photo-item__text" onSubmit={e => { e.preventDefault(); onGuardarTexto(texto); }}>
          <label htmlFor={id} className="field__label">Descripción de la foto {posicion}</label>
          <div className="inline-input">
            <input id={id} className="input" value={texto} maxLength={250} onChange={e => setTexto(e.target.value)} />
            <button type="submit" className="button button--secondary button--small" disabled={ocupado || texto.trim() === foto.descripcion}>
              Guardar<span className="sr-only"> descripción de la {etiquetaFoto}</span>
            </button>
          </div>
        </form>
        <div className="photo-item__actions">
          {posicion > 1 && (
            <button type="button" className="button button--ghost button--small" onClick={onPrincipal} disabled={ocupado}>
              <Icon name="star" size={16} /> Hacer principal<span className="sr-only"> la {etiquetaFoto}</span>
            </button>
          )}
          <button type="button" className="button button--ghost button--small" onClick={() => onMover(-1)} disabled={ocupado || posicion === 1}>
            <Icon name="up" size={16} /> Antes<span className="sr-only">: mover la {etiquetaFoto} una posición antes</span>
          </button>
          <button type="button" className="button button--ghost button--small" onClick={() => onMover(1)} disabled={ocupado || posicion === total}>
            <Icon name="down" size={16} /> Después<span className="sr-only">: mover la {etiquetaFoto} una posición después</span>
          </button>
          <button type="button" className="button button--ghost-danger button--small" onClick={onEliminar} disabled={ocupado}>
            <Icon name="trash" size={16} /> Eliminar<span className="sr-only"> la {etiquetaFoto}</span>
          </button>
        </div>
      </div>
    </li>
  );
}
