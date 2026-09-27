import { screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';

describe('NotFoundPage', () => {
  it('links staff to the dashboard (not the public landing page) and goes there', async () => {
    signInAs('OperationsManager');
    const { user, location } = renderApp('/no-such-page');

    expect(await screen.findByRole('heading', { name: 'Page not found' })).toBeInTheDocument();
    const link = screen.getByRole('link', { name: 'Go to the dashboard' });
    expect(link).toHaveAttribute('href', '/dashboard');

    await user.click(link);
    expect(await screen.findByRole('heading', { name: 'Dashboard' })).toBeInTheDocument();
    expect(location()).toBe('/dashboard');
  });
});
