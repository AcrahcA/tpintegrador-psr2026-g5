import { fecha } from '../../api/format';
import { useDatos } from '../data';
import { EnlaceMascota, Encabezado, Tabla } from '../ui';

export function Adopciones() {
  const { datos, mascota, adoptante } = useDatos();
  const lista = [...datos.adopciones].sort((a, b) => b.fechaAdopcion.localeCompare(a.fechaAdopcion));
  return (
    <>
      <Encabezado titulo="Adopciones realizadas" descripcion="Adopciones definitivas. Las mascotas adoptadas conservan su ficha e historial sanitario." />
      <Tabla caption="Adopciones realizadas" vacio="Todavía no se registraron adopciones."
        columnas={[{ titulo: 'Fecha' }, { titulo: 'Mascota' }, { titulo: 'Adoptante' }, { titulo: 'Solicitud' }, { titulo: 'Observaciones' }]}
        filas={lista.map(a => ({
          key: a.id,
          celdas: [fecha(a.fechaAdopcion), <EnlaceMascota mascota={mascota(a.mascotaId)} id={a.mascotaId} />,
            adoptante(a.adoptanteId)?.nombre ?? `#${a.adoptanteId}`, `#${a.solicitudAdopcionId}`, a.observaciones ?? '—']
        }))} />
    </>
  );
}
