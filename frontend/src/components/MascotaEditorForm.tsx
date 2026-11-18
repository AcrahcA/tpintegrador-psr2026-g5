import { useEffect, useState } from 'react';
import { EntryForm, type FieldSpec } from './UI';
import { photo, type Mascota } from '../model/types';

interface Props {
  fields: FieldSpec[];
  values?: Record<string, unknown>;
  current?: Mascota | null;
  onSave: (values: Record<string, unknown>) => Promise<boolean>;
  onClose: () => void;
  submitLabel?: string;
  hint?: string;
}

const MAX_BYTES = 5 * 1024 * 1024;
const validTypes = ['image/jpeg', 'image/png', 'image/webp'];

export function MascotaEditorForm({ fields, values, current, onSave, onClose, submitLabel, hint }: Props) {
  const [selected, setSelected] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [removeExisting, setRemoveExisting] = useState(false);
  const [photoError, setPhotoError] = useState('');

  useEffect(() => {
    if (!selected) { setPreview(null); return; }
    const localUrl = URL.createObjectURL(selected);
    setPreview(localUrl);
    return () => URL.revokeObjectURL(localUrl);
  }, [selected]);

  const existingUrl = current?.foto || null;
  const displayUrl = preview || (!removeExisting && existingUrl ? photo(current) : null);
  const extra = <section className="pet-photo-field" aria-label="Fotografía de la mascota">
    <div>
      <label className="pet-photo-label" htmlFor="pet-photo-input">Fotografía (opcional)</label>
      <p className="pet-photo-help">JPG, PNG o WebP · máximo 5 MB. Se guarda en el servidor.</p>
    </div>
    <div className="pet-photo-picker">
      <div className="pet-photo-preview">
        <img src={displayUrl || '/assets/img/sin-foto.svg'} alt={displayUrl ? 'Vista previa de la fotografía' : 'Mascota sin fotografía'} />
      </div>
      <div className="pet-photo-controls">
        <input
          id="pet-photo-input"
          type="file"
          accept="image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp"
          className="pt-control"
          onChange={event => {
            const file = event.target.files?.[0] ?? null;
            setPhotoError('');
            if (file && (!validTypes.includes(file.type) || !/\.(jpe?g|png|webp)$/i.test(file.name) || !file.size || file.size > MAX_BYTES)) {
              setPhotoError('Elegí un archivo JPG, PNG o WebP de hasta 5 MB.');
              setSelected(null);
              event.target.value = '';
              return;
            }
            setSelected(file);
            if (file) setRemoveExisting(false);
          }}
        />
        {selected && <button className="pet-photo-remove" type="button" onClick={() => {
          setSelected(null);
          const field = document.getElementById('pet-photo-input') as HTMLInputElement | null;
          if (field) field.value = '';
        }}>Descartar foto seleccionada</button>}
        {existingUrl && !selected && <label className="pet-photo-checkbox">
          <input type="checkbox" checked={removeExisting} onChange={e => setRemoveExisting(e.target.checked)} />
          Quitar fotografía actual
        </label>}
        {photoError && <p className="pet-photo-error" role="alert">{photoError}</p>}
      </div>
    </div>
  </section>;

  return <EntryForm
    fields={fields}
    values={values}
    extra={extra}
    onClose={onClose}
    submitLabel={submitLabel}
    hint={hint}
    onSubmit={async form => {
      if (photoError) throw new Error(photoError);
      return onSave({ ...form, ...(selected ? { fotoArchivo: selected } : {}), ...(removeExisting && !selected ? { quitarFoto: true } : {}) });
    }}
  />;
}
