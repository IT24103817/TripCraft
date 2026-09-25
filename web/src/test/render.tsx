import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useEffect } from 'react';
import { MemoryRouter, useLocation, useRoutes } from 'react-router-dom';
import { routes } from '@/app/router';
import { useAuthStore } from '@/auth/authStore';
import { configureHttpAuth } from '@/shared/api/http';
import type { Role } from '@/shared/api/types';
import { ToastProvider } from '@/shared/components/Toast';

const NAMES: Record<Role, string> = {
  Tourist: 'Demo Tourist 1',
  Guide: 'Demo Guide 1',
  OperationsManager: 'Demo Operations Manager 1',
  Admin: 'Demo Admin 1',
};

export function signInAs(role: Role) {
  useAuthStore.setState({
    token: 'test-token',
    expiresAt: new Date(Date.now() + 60 * 60_000).toISOString(),
    user: {
      id: 'user-1',
      email: `${role.toLowerCase()}@tripcraft.test`,
      fullName: NAMES[role],
      role,
      isActive: true,
    },
  });
}

function AppRoutes({ onLocation }: { onLocation: (pathname: string) => void }) {
  const location = useLocation();
  useEffect(() => {
    onLocation(location.pathname + location.search);
  }, [location, onLocation]);
  return useRoutes(routes);
}

/**
 * Renders the real app routes (guards, layout, lazy pages) at `path`, with fresh providers.
 * Uses MemoryRouter + useRoutes: the data router builds fetch Requests whose jsdom AbortSignal
 * Node rejects, and these routes have no loaders anyway.
 */
export function renderApp(path: string) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  const location = { current: path };
  // A 401 signs out; ProtectedRoute then redirects to /login by itself.
  configureHttpAuth({
    getToken: () => useAuthStore.getState().token,
    onUnauthorized: () => useAuthStore.getState().logout(),
  });
  const user = userEvent.setup();
  const view = render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter
          initialEntries={[path]}
          future={{ v7_startTransition: true, v7_relativeSplatPath: true }}
        >
          <AppRoutes
            onLocation={(pathname) => {
              location.current = pathname;
            }}
          />
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  );
  return { ...view, user, location: () => location.current };
}
