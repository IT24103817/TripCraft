import { screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { useAuthStore } from '@/auth/authStore';
import { API, server } from '@/test/server';
import { renderApp, signInAs } from '@/test/render';

describe('ProtectedRoute and RoleGuard', () => {
  it('redirects to /login when nobody is signed in', async () => {
    const { location } = renderApp('/trips');

    expect(await screen.findByRole('heading', { name: 'TripCraft operations' })).toBeInTheDocument();
    expect(location()).toBe('/login');
  });

  it('shows the 403 page to a Tourist', async () => {
    signInAs('Tourist');
    renderApp('/dashboard');

    expect(
      await screen.findByRole('heading', { name: 'You do not have access to this page' }),
    ).toBeInTheDocument();
  });

  it('blocks an Admin from the approval inbox (separation of duties) and hides the link', async () => {
    signInAs('Admin');
    renderApp('/approvals');

    expect(await screen.findByText('403')).toBeInTheDocument();
    const nav = screen.getByRole('navigation', { name: 'Main' });
    expect(nav).toHaveTextContent('Users');
    expect(nav).not.toHaveTextContent('Approvals');
  });

  it('signs out and returns to /login when the API answers 401 (expired token)', async () => {
    signInAs('OperationsManager');
    server.use(
      http.get(`${API}/api/trip-requests`, () =>
        HttpResponse.json({ title: 'Unauthorized' }, { status: 401 }),
      ),
    );
    const { location } = renderApp('/trips');

    expect(await screen.findByRole('heading', { name: 'TripCraft operations' })).toBeInTheDocument();
    expect(useAuthStore.getState().token).toBeNull();
    expect(location()).toBe('/login');
  });
});
