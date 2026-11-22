import { createContext, useCallback, useContext, useMemo, type ReactNode } from 'react';
import { ApiError, api } from '../api/client';
import type { Adopcion, Adoptante, Aviso, Capacidad, Cuidador, Mascota, Refugio, Solicitud, Tratamiento } from '../api/types';
import { useNotifier } from '../components/Notifier';
import { useSesion } from '../components/Sesion';
import { useAsync } from '../components/useAsync';

export type Datos = {
  refugio: Refugio; capacidad: Capacidad; mascotas: Mascota[]; avisos: Aviso[]; cuidadores: Cuidador[];
  tratamientos: Tratamiento[]; adoptantes: Adoptante[]; solicitudes: Solicitud[]; adopciones: Adopcion[];
};

type Ctx = {
  datos: Datos;
  recargar: () => Promise<void>;
  /** Ejecuta una operación, informa el resultado y recarga los datos. Devuelve true si salió bien. */
  ejecutar: (accion: () => Promise<unknown>, exito: string) => Promise<boolean>;
  mascota: (id: number) => Mascota | undefined;
  adoptante: (id: number) => Adoptante | undefined;
  cuidador: (id: number | null) => Cuidador | undefined;
};

const DataContext = createContext<Ctx | null>(null);

export async function cargarTodo(): Promise<Datos> {
  const [refugio, capacidad, mascotas, avisos, cuidadores, tratamientos, adoptantes, solicitudes, adopciones] = await Promise.all([
    api.refugio(), api.capacidad(), api.mascotas(), api.avisos(), api.cuidadores(), api.tratamientos(),
    api.adoptantes(), api.solicitudes(), api.adopciones()
  ]);
  return { refugio, capacidad, mascotas, avisos, cuidadores, tratamientos, adoptantes, solicitudes, adopciones };
}

export function DataProvider({ datos, recargar, children }: { datos: Datos; recargar: () => Promise<void>; children: ReactNode }) {
  const notifier = useNotifier();
  const { refrescar } = useSesion();

  const ejecutar = useCallback(async (accion: () => Promise<unknown>, exito: string) => {
    try {
      await accion();
      notifier.exito(exito);
      await recargar();
      return true;
    } catch (e) {
      notifier.error(e instanceof Error ? e.message : 'No se pudo completar la operación.');
      // Sesión vencida o sin permiso: se vuelve a verificar y aparece la pantalla de acceso
      if (e instanceof ApiError && (e.status === 401 || e.status === 403)) void refrescar();
      return false;
    }
  }, [notifier, recargar, refrescar]);

  const value = useMemo<Ctx>(() => {
    const mascotas = new Map(datos.mascotas.map(m => [m.id, m]));
    const adoptantes = new Map(datos.adoptantes.map(a => [a.id, a]));
    const cuidadores = new Map(datos.cuidadores.map(c => [c.id, c]));
    return {
      datos, recargar, ejecutar,
      mascota: id => mascotas.get(id),
      adoptante: id => adoptantes.get(id),
      cuidador: id => (id == null ? undefined : cuidadores.get(id))
    };
  }, [datos, recargar, ejecutar]);

  return <DataContext.Provider value={value}>{children}</DataContext.Provider>;
}

export function useDatos() {
  const ctx = useContext(DataContext);
  if (!ctx) throw new Error('Falta DataProvider');
  return ctx;
}

export function useCargaAdmin() {
  return useAsync(cargarTodo);
}
