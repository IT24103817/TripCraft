import { screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { API, server } from '@/test/server';
import { renderApp } from '@/test/render';

describe('LoginPage', () => {
  it('shows field errors and does not call the API when the form is invalid', async () => {
    let called = false;
    server.use(http.post(`${API}/api/auth/login`, () => ((called = true), HttpResponse.json({}))));
    const { user } = renderApp('/login');

    await user.type(await screen.findByLabelText('Email'), 'not-an-email');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByText('Enter a valid email address.')).toBeInTheDocument();
    expect(screen.getByText('Password is required.')).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toHaveAttribute('aria-invalid', 'true');
    expect(called).toBe(false);
  });

  it('shows an error when the API rejects the credentials', async () => {
    server.use(
      http.post(`${API}/api/auth/login`, () =>
        HttpResponse.json(
          { title: 'Unauthorized', detail: 'Invalid email or password.', status: 401 },
          { status: 401 },
        ),
      ),
    );
    const { user } = renderApp('/login');

    await user.type(await screen.findByLabelText('Email'), 'manager1@tripcraft.test');
    await user.type(screen.getByLabelText('Password'), 'wrong');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password.');
    expect(screen.getByRole('button', { name: 'Sign in' })).toBeEnabled();
  });

  it('sends a manager to the dashboard and a tourist to the mobile-app page', async () => {
    const respondAs = (role: string) =>
      server.use(
        http.post(`${API}/api/auth/login`, () =>
          HttpResponse.json({
            accessToken: 'jwt',
            expiresAt: new Date(Date.now() + 3_600_000).toISOString(),
            user: { id: 'u1', email: 'x@tripcraft.test', fullName: `Demo ${role}`, role, isActive: true },
          }),
        ),
      );

    respondAs('OperationsManager');
    const manager = renderApp('/login');
    await manager.user.type(await screen.findByLabelText('Email'), 'manager1@tripcraft.test');
    await manager.user.type(screen.getByLabelText('Password'), 'Passw0rd!');
    await manager.user.click(screen.getByRole('button', { name: 'Sign in' }));
    expect(await screen.findByRole('heading', { name: 'Dashboard' })).toBeInTheDocument();
    manager.unmount();

    respondAs('Tourist');
    const { useAuthStore } = await import('@/auth/authStore');
    useAuthStore.getState().logout();
    const tourist = renderApp('/login');
    await tourist.user.type(await screen.findByLabelText('Email'), 'tourist1@tripcraft.test');
    await tourist.user.type(screen.getByLabelText('Password'), 'Passw0rd!');
    await tourist.user.click(screen.getByRole('button', { name: 'Sign in' }));
    expect(
      await screen.findByRole('heading', { name: 'Please use the TripCraft mobile app' }),
    ).toBeInTheDocument();
  });
});
