import { useEffect, useId, useRef } from 'react';
import { cn } from '../../utils/cn';
import type { DialogProps } from './Dialog.types';

/** Accessible modal: labelled, Escape closes it, focus moves inside and returns afterwards. */
export const Dialog = ({
  open,
  title,
  onClose,
  children,
  size = 'default',
  placement = 'center',
  className,
}: DialogProps) => {
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
        'fixed inset-0 z-40 flex bg-ink/50 backdrop-blur-[2px]',
        placement === 'side' ? 'justify-end' : 'items-center justify-center p-4',
      )}
    >
      <div
        ref={panelRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className={cn(
          'w-full overflow-y-auto border-slate-200 bg-surface p-6 shadow-xl',
          placement === 'side' ? 'h-full max-w-md border-l' : 'max-h-[90vh] rounded-lg border',
          placement === 'center' && (size === 'wide' ? 'max-w-3xl' : 'max-w-lg'),
          className,
        )}
      >
        <h2 id={titleId} className="mb-4 text-lg font-semibold text-slate-900">
          {title}
        </h2>
        {children}
      </div>
    </div>
  );
};
