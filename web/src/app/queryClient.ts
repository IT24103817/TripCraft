import { QueryClient } from '@tanstack/react-query';
import { getErrorStatus } from '@/shared/api/errors';

/** 30 s staleTime; client errors (4xx) are never retried, server/network errors once. */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        retry: (failureCount, error) => {
          const status = getErrorStatus(error);
          if (status !== undefined && status < 500) return false;
          return failureCount < 1;
        },
      },
    },
  });
}
