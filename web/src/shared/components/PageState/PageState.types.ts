import type { ReactNode } from 'react';

export interface PageStateProps {
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

export interface LoadingSkeletonProps {
  rows?: number;
}

export interface ErrorStateProps {
  error?: unknown;
  onRetry?: () => void;
}
