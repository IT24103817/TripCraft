import { screen, within } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { renderApp, signInAs } from '@/test/render';

describe('LandingPage', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
    vi.resetModules();
  });

  it('is public and explains TripCraft in four steps for three audiences', async () => {
    renderApp('/');

    expect(
      await screen.findByRole('heading', { level: 1, name: /We plan it, price it and book it for you/ }),
    ).toBeInTheDocument();
    const steps = within(screen.getByRole('region', { name: 'How it works' })).getAllByRole('listitem');
    expect(steps.map((s) => s.querySelector('h3')?.textContent)).toEqual([
      'Tell us your trip',
      'We plan it for you',
      'Checked and sent to you',
      'You accept, we confirm',
    ]);
    const audiences = within(screen.getByRole('region', { name: 'Who it’s for' })).getAllByRole('heading', {
      level: 3,
    });
    expect(audiences.map((h) => h.textContent)).toEqual(['Tourists', 'Guides', 'Operations']);
    expect(screen.getByText(/Tourist\? Use the app/)).toBeInTheDocument();
    expect(screen.getByText(/iOS build is available on request/)).toBeInTheDocument();
  });

  it('links Staff login to /login and shows the group in the footer', async () => {
    const { user, location } = renderApp('/');

    const footer = await screen.findByRole('contentinfo');
    expect(footer).toHaveTextContent('SE3090 Software Engineering Frameworks');
    expect(footer).not.toHaveTextContent('SE3090 · SE3090');
    await user.click(screen.getByRole('link', { name: 'Staff login' }));

    expect(location()).toBe('/login');
    expect(await screen.findByRole('heading', { name: 'TripCraft operations' })).toBeInTheDocument();
  });

  it('shows the Download APK button with the configured release URL', async () => {
    vi.stubEnv('VITE_APK_URL', 'https://github.com/example/SE3090_G07/releases/tag/v1.0');
    vi.stubEnv('VITE_GROUP_NUMBER', '07');
    vi.resetModules();
    const { renderApp: render } = await import('@/test/render');
    render('/');

    expect(await screen.findByRole('link', { name: 'Download APK' })).toHaveAttribute(
      'href',
      'https://github.com/example/SE3090_G07/releases/tag/v1.0',
    );
    expect(screen.getByRole('contentinfo')).toHaveTextContent('SE3090_G07');
  });

  it('sends a signed-in manager from /login to the dashboard, which now lives at /dashboard', async () => {
    signInAs('OperationsManager');
    const { location } = renderApp('/login');

    expect(await screen.findByRole('heading', { name: 'Dashboard' })).toBeInTheDocument();
    expect(location()).toBe('/dashboard');
  });
});
