import { getErrorMessage } from '../../api/errors';
import { Button } from '../Button';
import { Card } from '../Card';
import type { ErrorStateProps, LoadingSkeletonProps, PageStateProps } from './PageState.types';

export const LoadingSkeleton = ({ rows = 5 }: LoadingSkeletonProps) => (
  <Card role="status" aria-live="polite" aria-label="Loading" className="space-y-3">
    {Array.from({ length: rows }, (_, i) => (
      <div key={i} className="h-5 animate-pulse rounded bg-slate-200" />
    ))}
    <span className="sr-only">Loading…</span>
  </Card>
);

export const ErrorState = ({ error, onRetry }: ErrorStateProps) => (
  <Card role="alert" className="flex flex-col items-center gap-3 border-red-200 bg-red-50 py-8 text-center">
    <span
      className="flex h-10 w-10 items-center justify-center rounded-full bg-red-100 text-lg font-bold text-red-700"
      aria-hidden="true"
    >
      !
    </span>
    <p className="font-medium text-red-800">Could not load this page</p>
    <p className="text-sm text-red-700">{getErrorMessage(error)}</p>
    {onRetry && (
      <Button variant="secondary" onClick={onRetry}>
        Retry
      </Button>
    )}
  </Card>
);

/** The four page states: loading skeleton, error with retry, empty with an action, or the content. */
export const PageState = (props: PageStateProps) => {
  if (props.isLoading) return <LoadingSkeleton />;
  if (props.isError) return <ErrorState error={props.error} onRetry={props.onRetry} />;
  if (props.isEmpty) {
    return (
      <Card className="flex flex-col items-center gap-2 border-dashed py-12 text-center">
        <span
          className="mb-1 flex h-10 w-10 items-center justify-center rounded-full bg-brand-50 text-brand-700"
          aria-hidden="true"
        >
          <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M4 7h16M4 12h10M4 17h6" strokeLinecap="round" />
          </svg>
        </span>
        <p className="font-medium text-slate-800">{props.emptyTitle ?? 'Nothing here yet'}</p>
        {props.emptyDescription && <p className="text-sm text-slate-500">{props.emptyDescription}</p>}
        {props.emptyAction}
      </Card>
    );
  }
  return <>{props.children}</>;
};
