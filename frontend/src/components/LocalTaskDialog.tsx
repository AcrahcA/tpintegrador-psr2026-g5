import {
  useEffect,
  useRef,
  useState,
  type FormEvent
} from 'react';

import type { LocalTask } from './RefugioDashboard';
import { Icon } from './Icon';

const KEY = 'patitas_local_dashboard_tasks_v1';

export const toISO = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

export function useLocalTasks() {
  const [tasks, setTasks] = useState<LocalTask[]>(() => {
    try {
      const data = JSON.parse(localStorage.getItem(KEY) || '[]');
      return Array.isArray(data) ? data : [];
    } catch {
      return [];
    }
  });

  const add = (task: Omit<LocalTask, 'id'>) => {
    const next = [
      ...tasks,
      { ...task, id: crypto.randomUUID() }
    ];
    setTasks(next);
    localStorage.setItem(KEY, JSON.stringify(next));
  };

  return { tasks, add };
}

export function LocalTaskDialog({
  onClose,
  onCreate
}: {
  onClose: () => void;
  onCreate: (task: Omit<LocalTask, 'id'>) => void;
}) {
  const [title, setTitle] = useState('');
  const [date, setDate] = useState(toISO(new Date()));
  const [time, setTime] = useState('09:00');
  const [category, setCategory] = useState('Salud');
  const [notes, setNotes] = useState('');
  const titleRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    titleRef.current?.focus();

    const close = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };

    document.addEventListener('keydown', close);

    return () => document.removeEventListener('keydown', close);
  }, [onClose]);

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!title.trim()) return;

    onCreate({
      title: title.trim(),
      date,
      time,
      category,
      notes: notes.trim()
    });

    onClose();
  };

  return (
    <div
      className="nido-modal-backdrop"
      onMouseDown={e => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <section
        role="dialog"
        aria-modal="true"
        aria-labelledby="nido-task-title"
        className="nido-modal-panel"
      >
        <header className="nido-modal-head">
          <div>
            <h2 id="nido-task-title">Nueva tarea</h2>
            <p>Agenda local del panel de demostración.</p>
          </div>
          <button
            className="nido-icon-link"
            aria-label="Cerrar"
            onClick={onClose}
            type="button"
          >
            <Icon name="x" size={21} />
          </button>
        </header>

        <form onSubmit={submit} className="nido-task-form">
          <label>
            Título de la tarea
            <input
              ref={titleRef}
              required
              value={title}
              onChange={e => setTitle(e.target.value)}
              maxLength={100}
              placeholder="Ej.: Control veterinario de Bruno"
            />
          </label>

          <div className="nido-form-cols">
            <label>
              Fecha
              <input
                required
                type="date"
                value={date}
                onChange={e => setDate(e.target.value)}
              />
            </label>

            <label>
              Hora
              <input
                required
                type="time"
                value={time}
                onChange={e => setTime(e.target.value)}
              />
            </label>
          </div>

          <label>
            Área
            <select
              value={category}
              onChange={e => setCategory(e.target.value)}
            >
              <option>Salud</option>
              <option>Rescates</option>
              <option>Adopciones</option>
              <option>Administración</option>
            </select>
          </label>

          <label>
            Observaciones (opcional)
            <textarea
              value={notes}
              onChange={e => setNotes(e.target.value)}
              rows={3}
              maxLength={180}
            />
          </label>

          <p className="nido-form-hint">
            Estas tareas se guardan solamente en este navegador.
            No se envían al backend.
          </p>

          <footer className="nido-modal-actions">
            <button
              className="nido-outline-btn"
              type="button"
              onClick={onClose}
            >
              Cancelar
            </button>
            <button className="nido-primary-btn" type="submit">
              Guardar tarea
            </button>
          </footer>
        </form>
      </section>
    </div>
  );
}