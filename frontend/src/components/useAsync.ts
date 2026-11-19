import { useCallback, useEffect, useRef, useState } from 'react';

/** Carga datos asincrónicos con estado de carga/error y una función para recargar. */
export function useAsync<T>(cargar: () => Promise<T>, deps: unknown[] = []) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const cargarRef = useRef(cargar);
  cargarRef.current = cargar;

  const reload = useCallback(async () => {
    setLoading(true);
    try {
      const d = await cargarRef.current();
      setData(d);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error desconocido.');
    } finally {
      setLoading(false);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  useEffect(() => { void reload(); }, [reload]);

  return { data, error, loading, reload, setData };
}
