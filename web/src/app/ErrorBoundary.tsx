import { QueryErrorResetBoundary } from '@tanstack/react-query';
import { Component, type ErrorInfo, type ReactNode } from 'react';

interface State {
  error: Error | null;
}

class Boundary extends Component<{ onReset: () => void; children: ReactNode }, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled UI error', error, info.componentStack);
  }

  render() {
    if (!this.state.error) return this.props.children;
    return (
      <div role="alert" className="card m-6 space-y-3 border-red-200 bg-red-50">
        <h1 className="font-semibold text-red-800">Something went wrong on this page</h1>
        <p className="text-sm text-red-700">The error was logged. Try again, or reload the page.</p>
        <button
          type="button"
          className="btn-secondary"
          onClick={() => {
            this.props.onReset();
            this.setState({ error: null });
          }}
        >
          Try again
        </button>
      </div>
    );
  }
}

/** Global error boundary that also resets failed TanStack queries when the user retries. */
export function ErrorBoundary({ children }: { children: ReactNode }) {
  return (
    <QueryErrorResetBoundary>
      {({ reset }) => <Boundary onReset={reset}>{children}</Boundary>}
    </QueryErrorResetBoundary>
  );
}
