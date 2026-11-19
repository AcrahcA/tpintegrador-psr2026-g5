import { useMemo, useState } from 'react';
import { api } from '../../api/client';
import type { Adoptante } from '../../api/types';
import { ConfirmDialog } from '../../components/ConfirmDialog';
import { Dialog } from '../../components/Dialog';
import { AdoptanteForm } from '../../components/forms';
import { Icon } from '../../components/Icon';
import { useDatos } from '../data';
import { Encabezado, Filtros, Tabla } from '../ui';

export function Adoptantes() {
  const { datos, ejecutar } = useDatos();
  const [busqueda, setBusqueda] = useState('');
  const [editando, setEditando] = useState<Adoptante | 'nuevo' | null>(null);
  const [eliminar, setEliminar] = useState<Adoptante | null>(null);

  const lista = useMemo(() => {
    const q = busqueda.trim().toLowerCase();
    return datos.adoptantes.filter(a => !q || `${a.nombre} ${a.dni} ${a.email ?? ''}`.toLowerCase().includes(q));
  }, [datos.adoptantes, busqueda]);

  return (
    <>
      <Encabezado titulo="Adoptantes" descripcion="Personas registradas como posibles adoptantes."
        acciones={<button type="button" className="button button--primary" onClick={() => setEditando('nuevo')}><Icon name="plus" /> Registrar adoptante</button>} />
      <Filtros etiqueta="Buscar adoptantes">
        <div className="field">
          <label htmlFor="a-buscar" className="field__label">Buscar por nombre, DNI o correo</label>
          <input id="a-buscar" className="input" type="search" value={busqueda} onChange={e => setBusqueda(e.target.value)} />
        </div>
      </Filtros>
      <p className="results-count" role="status">{lista.length === 1 ? '1 adoptante' : `${lista.length} adoptantes`}</p>
      <Tabla caption="Adoptantes" vacio="No hay adoptantes que coincidan."
        columnas={[{ titulo: 'Nombre' }, { titulo: 'DNI' }, { titulo: 'Contacto' }, { titulo: 'Solicitudes' }, { titulo: 'Acciones', className: 'col-actions' }]}
        filas={lista.map(a => {
          const cantidad = datos.solicitudes.filter(s => s.adoptanteId === a.id).length;
          return {
            key: a.id,
            celdas: [
              <strong>{a.nombre}</strong>, a.dni,
              <>{a.telefono && <a href={`tel:${a.telefono.replace(/[^\d+]/g, '')}`}>{a.telefono}</a>}{a.telefono && a.email && <br />}
                {a.email && <a href={`mailto:${a.email}`}>{a.email}</a>}{!a.telefono && !a.email && '—'}</>,
              cantidad,
              <div className="button-row">
                <button type="button" className="button button--secondary button--small" onClick={() => setEditando(a)}>Editar<span className="sr-only"> a {a.nombre}</span></button>
                {cantidad === 0 && (
                  <button type="button" className="button button--ghost-danger button--small" onClick={() => setEliminar(a)}>Eliminar<span className="sr-only"> a {a.nombre}</span></button>
                )}
              </div>
            ]
          };
        })} />
      {editando && (
        <Dialog title={editando === 'nuevo' ? 'Registrar adoptante' : `Editar a ${editando.nombre}`} onClose={() => setEditando(null)}>
          <AdoptanteForm inicial={editando === 'nuevo' ? undefined : editando} onCancel={() => setEditando(null)}
            onSubmit={async d => {
              if (editando === 'nuevo') await api.crearAdoptante(d); else await api.actualizarAdoptante(editando.id, d);
              setEditando(null);
              await ejecutar(async () => null, editando === 'nuevo' ? 'Adoptante registrado.' : 'Adoptante actualizado.');
            }} />
        </Dialog>
      )}
      {eliminar && (
        <ConfirmDialog titulo={`Eliminar a ${eliminar.nombre}`} confirmar="Sí, eliminar" peligro onClose={() => setEliminar(null)}
          onConfirm={() => ejecutar(() => api.eliminarAdoptante(eliminar.id), `${eliminar.nombre} fue eliminado.`)}>
          <p>Se eliminarán sus datos. No se puede deshacer.</p>
        </ConfirmDialog>
      )}
    </>
  );
}
