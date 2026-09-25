import type { ReactNode } from 'react';
import { getErrorMessage } from '../api/errors';

interface PageStateProps {
  isLoading: boolean;
  isError: boolean;
  error?: unknown;
  onRetry?: () => void;
  isEmpty?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: ReactNode;
  children: ReactNode;
}

/** The four page states: loading skeleton, error with retry, empty with an action, or the content. */
export function PageState(props: PageStateProps) {
  if (props.isLoading) return <LoadingSkeleton />;
  if (props.isError) return <ErrorState error={props.error} onRetry={props.onRetry} />;
  if (props.isEmpty) {
    return (
      <div className="card flex flex-col items-center gap-2 py-10 text-center">
        <p className="font-medium text-slate-800">{props.emptyTitle ?? 'Nothing here yet'}</p>
        {props.emptyDescription && <p className="text-sm text-slate-500">{props.emptyDescription}</p>}
        {props.emptyAction}
      </div>
    );
  }
  return <>{props.children}</>;
}

export function LoadingSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <div role="status" aria-live="polite" aria-label="Loading" className="card space-y-3">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="h-5 animate-pulse rounded bg-slate-200" />
      ))}
      <span className="sr-only">Loading…</span>
    </div>
  );
}

export function ErrorState({ error, onRetry }: { error?: unknown; onRetry?: () => void }) {
  return (
    <div
      role="alert"
      className="card flex flex-col items-center gap-3 border-red-200 bg-red-50 py-8 text-center"
    >
      <p className="font-medium text-red-800">Could not load this page</p>
      <p className="text-sm text-red-700">{getErrorMessage(error)}</p>
      {onRetry && (
        <button type="button" className="btn-secondary" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  );
}
