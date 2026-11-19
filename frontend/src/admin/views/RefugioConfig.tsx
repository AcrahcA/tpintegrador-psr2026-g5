import { api } from '../../api/client';
import { ErrorSummary, TextField, enfocarResumen } from '../../components/Fields';
import { useFormulario } from '../../components/useFormulario';
import { useDatos } from '../data';
import { Encabezado, Meter } from '../ui';

export function RefugioConfig() {
  const { datos, ejecutar } = useDatos();
  const f = useFormulario({ nombre: datos.refugio.nombre, capacidadMaxima: datos.refugio.capacidadMaxima.toString() });
  const { capacidad } = datos;
  return (
    <>
      <Encabezado titulo="Datos del refugio" descripcion="Nombre y capacidad máxima de animales alojados al mismo tiempo." />
      <div className="two-columns">
        <section className="card" aria-labelledby="r-ocupacion">
          <h2 id="r-ocupacion" className="card__title">Ocupación actual</h2>
          <Meter valor={capacidad.mascotasActuales} maximo={capacidad.capacidadMaxima} etiqueta="Ocupación del refugio" />
          <p>{capacidad.plazasDisponibles} plazas libres. Las mascotas adoptadas no ocupan plaza.</p>
        </section>
        <section className="card" aria-labelledby="r-config">
          <h2 id="r-config" className="card__title">Configuración</h2>
          <form noValidate onSubmit={async e => {
            e.preventDefault();
            const form = e.currentTarget;
            const er: typeof f.errores = {};
            if (!f.valores.nombre.trim()) er.nombre = 'Ingresá el nombre del refugio.';
            const cap = Number(f.valores.capacidadMaxima);
            if (!Number.isInteger(cap) || cap < 1) er.capacidadMaxima = 'La capacidad debe ser un número entero mayor a cero.';
            else if (cap < capacidad.mascotasActuales) er.capacidadMaxima = `No puede ser menor a las ${capacidad.mascotasActuales} mascotas alojadas.`;
            f.setErrores(er);
            if (Object.keys(er).length) { enfocarResumen(form); return; }
            await ejecutar(() => api.configurarRefugio(f.valores.nombre.trim(), cap), 'Datos del refugio guardados.');
          }}>
            <ErrorSummary errors={f.errores} ids={f.ids} />
            <TextField label="Nombre del refugio" required {...f.campo('nombre')} />
            <TextField label="Capacidad máxima" required type="number" inputMode="numeric" min={1} {...f.campo('capacidadMaxima')} />
            <div className="form-actions"><button type="submit" className="button button--primary">Guardar</button></div>
          </form>
        </section>
      </div>
    </>
  );
}
