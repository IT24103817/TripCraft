import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { HomeLink } from '@/auth/HomeLink';
import { renderApp, signInAs } from '@/test/render';

describe('HomeLink on the 403 and 404 pages', () => {
  it('sends an anonymous visitor to the public home page', () => {
    render(
      <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <HomeLink />
      </MemoryRouter>,
    );

    expect(screen.getByRole('link', { name: 'Go to the TripCraft home page' })).toHaveAttribute('href', '/');
  });

  it('sends a Tourist on the 403 page to the mobile-app page, not the landing page', async () => {
    signInAs('Tourist');
    renderApp('/dashboard');

    expect(await screen.findByRole('link', { name: 'Go to your start page' })).toHaveAttribute(
      'href',
      '/mobile-app',
    );
  });

  it('sends an Admin on the 403 page to the dashboard', async () => {
    signInAs('Admin');
    renderApp('/approvals');

    expect(await screen.findByRole('link', { name: 'Go to the dashboard' })).toHaveAttribute(
      'href',
      '/dashboard',
    );
  });
});
