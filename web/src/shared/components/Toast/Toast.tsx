import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import { cn } from '../../utils/cn';
import type { ToastApi, ToastItem, ToastKind, ToastProviderProps } from './Toast.types';

const ToastContext = createContext<ToastApi | null>(null);
let nextId = 1;

/** Toasts are announced to screen readers (role="status") and disappear after 5 seconds. */
export const ToastProvider = ({ children }: ToastProviderProps) => {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const dismiss = useCallback((id: number) => setToasts((all) => all.filter((t) => t.id !== id)), []);
  const push = useCallback(
    (kind: ToastKind, message: string) => {
      const id = nextId++;
      setToasts((all) => [...all, { id, kind, message }]);
      setTimeout(() => dismiss(id), 5000);
    },
    [dismiss],
  );

  const api = useMemo<ToastApi>(
    () => ({ success: (m) => push('success', m), error: (m) => push('error', m) }),
    [push],
  );

  return (
    <ToastContext.Provider value={api}>
      {children}
      <div
        role="status"
        aria-live="polite"
        className="fixed bottom-4 right-4 z-50 flex w-80 max-w-[calc(100vw-2rem)] flex-col gap-2"
      >
        {toasts.map((toast) => (
          <div
            key={toast.id}
            className={cn(
              'flex items-start gap-3 rounded-md p-3 text-sm text-white shadow-lg',
              toast.kind === 'success' ? 'bg-green-700' : 'bg-red-700',
            )}
          >
            <span
              className={cn(
                'flex h-5 w-5 shrink-0 items-center justify-center rounded-full text-xs font-bold',
                toast.kind === 'success' ? 'bg-green-800' : 'bg-red-800',
              )}
              aria-hidden="true"
            >
              {toast.kind === 'success' ? '✓' : '!'}
            </span>
            <span className="flex-1">{toast.message}</span>
            <button
              type="button"
              className={cn(
                '-m-1 rounded p-1 leading-none focus:outline-none focus-visible:ring-2 focus-visible:ring-white',
                toast.kind === 'success' ? 'hover:bg-green-800' : 'hover:bg-red-800',
              )}
              aria-label="Dismiss notification"
              onClick={() => dismiss(toast.id)}
            >
              ×
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useToast = (): ToastApi => {
  const api = useContext(ToastContext);
  if (!api) throw new Error('useToast must be used inside ToastProvider');
  return api;
};
