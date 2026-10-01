import { useEffect, useId, useRef, type ReactNode } from 'react';
import { cn } from '../utils/cn';

interface DialogProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  /** "wide" for forms with a table (e.g. a hotel and its room types). */
  size?: 'default' | 'wide';
  /** "side" slides in from the right as a panel (e.g. one availability cell). */
  placement?: 'center' | 'side';
}

/** Accessible modal: labelled, Escape closes it, focus moves inside and returns afterwards. */
export function Dialog({
  open,
  title,
  onClose,
  children,
  size = 'default',
  placement = 'center',
}: DialogProps) {
  const titleId = useId();
  const panelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const previous = document.activeElement as HTMLElement | null;
    const firstField = panelRef.current?.querySelector<HTMLElement>('input, textarea, select, button');
    firstField?.focus();
    const onKey = (event: KeyboardEvent) => event.key === 'Escape' && onClose();
    document.addEventListener('keydown', onKey);
    return () => {
      document.removeEventListener('keydown', onKey);
      previous?.focus();
    };
  }, [open, onClose]);

  if (!open) return null;
  return (
    <div
      className={cn(
        'fixed inset-0 z-40 flex bg-ink/40',
        placement === 'side' ? 'justify-end' : 'items-center justify-center p-4',
      )}
    >
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={cn(
          'w-full overflow-y-auto bg-surface p-5 shadow-xl',
          placement === 'side' ? 'h-full max-w-md' : 'max-h-[90vh] rounded-lg',
          placement === 'center' && (size === 'wide' ? 'max-w-3xl' : 'max-w-lg'),
        )}
      >
        <h2 id={titleId} className="mb-3 text-lg font-semibold text-slate-900">
          {title}
        </h2>
        {children}
      </div>
    </div>
  );
}
