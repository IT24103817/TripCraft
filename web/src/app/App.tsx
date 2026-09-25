import { QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { RouterProvider } from 'react-router-dom';
import { useAuthStore } from '@/auth/authStore';
import { configureHttpAuth } from '@/shared/api/http';
import { ToastProvider } from '@/shared/components/Toast';
import { ErrorBoundary } from './ErrorBoundary';
import { createQueryClient } from './queryClient';
import { router } from './router';

// JWT on every request; a 401 (expired or revoked token) signs out and goes to /login.
configureHttpAuth({
  getToken: () => useAuthStore.getState().token,
  onUnauthorized: () => {
    useAuthStore.getState().logout();
    void router.navigate('/login', { replace: true });
  },
});

export function App() {
  const [queryClient] = useState(createQueryClient);
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <RouterProvider router={router} future={{ v7_startTransition: true }} />
        </ToastProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}
