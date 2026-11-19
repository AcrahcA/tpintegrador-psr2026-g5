import { useState } from 'react';
import { api } from '../../api/client';
import type { Cuidador } from '../../api/types';
import { Badge } from '../../components/Badge';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { Dialog } from '../../components/Dialog';
import { CheckboxField, ErrorSummary, TextField, enfocarResumen } from '../../components/Fields';
import { Icon } from '../../components/Icon';
import { useFormulario } from '../../components/useFormulario';
import { useDatos } from '../data';
import { EnlaceMascota, Encabezado, Meter, Tabla } from '../ui';

export function Cuidadores() {
  const { datos } = useDatos();
  const [editando, setEditando] = useState<Cuidador | 'nuevo' | null>(null);
  const [eliminar, setEliminar] = useState<Cuidador | null>(null);
  const { ejecutar } = useDatos();

  return (
    <>
      <Encabezado titulo="Cuidadores" descripcion="Cada cuidador atiende mascotas hasta su capacidad máxima y según su especialización."
        acciones={<button type="button" className="button button--primary" onClick={() => setEditando('nuevo')}><Icon name="plus" /> Registrar cuidador</button>} />
      <Tabla caption="Cuidadores" vacio="Todavía no hay cuidadores registrados."
        columnas={[{ titulo: 'Cuidador' }, { titulo: 'Especialización' }, { titulo: 'Carga' }, { titulo: 'Mascotas asignadas' }, { titulo: 'Acciones', className: 'col-actions' }]}
        filas={datos.cuidadores.map(c => {
          const asignadas = datos.mascotas.filter(m => m.cuidadorId === c.id && m.estado !== 'Adoptada');
          return {
            key: c.id,
            celdas: [
              <><strong>{c.nombre}</strong><br />{c.disponible ? <Badge tono="verde">Disponible</Badge> : <Badge tono="gris">No disponible</Badge>}</>,
              c.especializacion ?? 'General (todas las especies)',
              <Meter valor={c.mascotasAsignadas} maximo={c.capacidadMaxima} etiqueta={`Mascotas asignadas a ${c.nombre}`} />,
              asignadas.length === 0 ? <span className="muted">Ninguna</span> : (
                <ul className="chips-list">{asignadas.map(m => <li key={m.id}><EnlaceMascota mascota={m} id={m.id} /></li>)}</ul>
              ),
              <div className="button-row">
                <button type="button" className="button button--secondary button--small" onClick={() => setEditando(c)}>
                  Editar<span className="sr-only"> a {c.nombre}</span>
                </button>
                <button type="button" className="button button--ghost-danger button--small" onClick={() => setEliminar(c)}>
                  Eliminar<span className="sr-only"> a {c.nombre}</span>
                </button>
              </div>
            ]
          };
        })} />
      {editando && <CuidadorDialog cuidador={editando === 'nuevo' ? undefined : editando} onClose={() => setEditando(null)} />}
      {eliminar && (
        <ConfirmDialog titulo={`Eliminar a ${eliminar.nombre}`} confirmar="Sí, eliminar" peligro onClose={() => setEliminar(null)}
          onConfirm={() => ejecutar(() => api.eliminarCuidador(eliminar.id), `${eliminar.nombre} fue eliminado.`)}>
          <p>Sólo se puede eliminar un cuidador sin mascotas asignadas.</p>
        </ConfirmDialog>
      )}
    </>
  );
}

function CuidadorDialog({ cuidador, onClose }: { cuidador?: Cuidador; onClose: () => void }) {
  const { ejecutar } = useDatos();
  const f = useFormulario({
    nombre: cuidador?.nombre ?? '', especializacion: cuidador?.especializacion ?? '',
    capacidadMaxima: cuidador?.capacidadMaxima.toString() ?? '5', disponible: cuidador?.disponible ?? true
  });
  const v = f.valores;
  return (
    <Dialog title={cuidador ? `Editar a ${cuidador.nombre}` : 'Registrar cuidador'} onClose={onClose}>
      <form noValidate onSubmit={async e => {
        e.preventDefault();
        const form = e.currentTarget;
        const er: typeof f.errores = {};
        if (!v.nombre.trim()) er.nombre = 'Ingresá el nombre.';
        const cap = Number(v.capacidadMaxima);
        if (!Number.isInteger(cap) || cap < 1 || cap > 100) er.capacidadMaxima = 'La capacidad debe ser un número entre 1 y 100.';
        f.setErrores(er);
        if (Object.keys(er).length) { enfocarResumen(form); return; }
        const datos = { nombre: v.nombre.trim(), especializacion: v.especializacion.trim() || null, capacidadMaxima: cap, disponible: Boolean(v.disponible) };
        const ok = await ejecutar(() => cuidador ? api.actualizarCuidador(cuidador.id, datos) : api.crearCuidador(datos),
          cuidador ? 'Cuidador actualizado.' : 'Cuidador registrado.');
        if (ok) onClose();
      }}>
        <ErrorSummary errors={f.errores} ids={f.ids} />
        <div className="form-grid">
          <TextField label="Nombre y apellido" required autoComplete="off" {...f.campo('nombre')} data-autofocus />
          <TextField label="Especialización" hint="Tipos de animal que atiende: por ejemplo “Perros” o “Gatos y aves”. Vacío = todas las especies." {...f.campo('especializacion')} />
          <TextField label="Capacidad máxima de mascotas" required type="number" inputMode="numeric" min={1} max={100} {...f.campo('capacidadMaxima')} />
        </div>
        <CheckboxField label="Disponible para recibir mascotas" id={f.ids.disponible} checked={Boolean(v.disponible)}
          onChange={e => f.setValores(x => ({ ...x, disponible: e.target.checked }))} />
        <div className="form-actions">
          <button type="button" className="button button--secondary" onClick={onClose}>Cancelar</button>
          <button type="submit" className="button button--primary">Guardar</button>
        </div>
      </form>
    </Dialog>
  );
}
