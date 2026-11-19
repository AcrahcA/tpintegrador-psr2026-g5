import { useEffect, useId, useRef, type ReactNode } from 'react';
import { Icon } from './Icon';

type Props = {
  title: string;
  description?: ReactNode;
  onClose: () => void;
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg';
};

/**
 * Diálogo modal accesible basado en <dialog>: el fondo queda inerte, Esc lo cierra,
 * el foco va al primer elemento marcado con data-autofocus (o al título) y vuelve
 * al control que lo abrió al cerrarse.
 * Se monta sólo cuando está abierto: {abierto && <Dialog …/>}.
 */
export function Dialog({ title, description, onClose, children, size = 'md' }: Props) {
  const ref = useRef<HTMLDialogElement>(null);
  const titleRef = useRef<HTMLHeadingElement>(null);
  const titleId = useId();
  const descId = useId();
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  useEffect(() => {
    const dialog = ref.current!;
    const anterior = document.activeElement as HTMLElement | null;
    if (!dialog.open) dialog.showModal();
    const destino = dialog.querySelector<HTMLElement>('[data-autofocus]') ?? titleRef.current;
    destino?.focus();

    const alCancelar = (e: Event) => { e.preventDefault(); onCloseRef.current(); };
    dialog.addEventListener('cancel', alCancelar);
    return () => {
      dialog.removeEventListener('cancel', alCancelar);
      if (dialog.open) dialog.close();
      if (anterior && document.contains(anterior)) anterior.focus();
    };
  }, []);

  return (
    <dialog ref={ref} className={`dialog dialog--${size}`} aria-labelledby={titleId}
      aria-describedby={description ? descId : undefined}>
      <div className="dialog__header">
        <div>
          <h2 id={titleId} ref={titleRef} tabIndex={-1} className="dialog__title">{title}</h2>
          {description && <p id={descId} className="dialog__description">{description}</p>}
        </div>
        <button type="button" className="icon-button" onClick={onClose}>
          <Icon name="close" />
          <span className="sr-only">Cerrar</span>
        </button>
      </div>
      <div className="dialog__body">{children}</div>
    </dialog>
  );
}
