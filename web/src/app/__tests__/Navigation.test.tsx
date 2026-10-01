import { screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';

/** The sidebar's group headings and, per group, its link names. */
function sidebarGroups() {
  const nav = screen.getByRole('navigation', { name: 'Main' });
  return within(nav)
    .getAllByRole('region')
    .map((group) => ({
      group: within(group).getByRole('heading').textContent,
      links: within(group)
        .getAllByRole('link')
        .map((link) => link.textContent),
    }));
}

describe('Sidebar groups', () => {
  it('gives the Operations Manager Operations, Resources and Insights, each link once', async () => {
    signInAs('OperationsManager');
    renderApp('/dashboard');
    await screen.findByRole('heading', { name: 'Dashboard', level: 1 });

    expect(sidebarGroups()).toEqual([
      { group: 'Operations', links: ['Dashboard', 'Trips', 'Review queue'] },
      { group: 'Resources', links: ['Guides', 'Vehicles', 'Hotels', 'Availability'] },
      { group: 'Insights', links: ['Reports', 'Agent runs'] },
    ]);
  });

  it('gives the Admin the dashboard, Agent runs and the Admin group with Settings', async () => {
    signInAs('Admin');
    renderApp('/dashboard');
    await screen.findByRole('heading', { name: 'Dashboard', level: 1 });

    expect(sidebarGroups()).toEqual([
      { group: 'Operations', links: ['Dashboard'] },
      { group: 'Insights', links: ['Agent runs'] },
      { group: 'Admin', links: ['Users', 'Audit log', 'Settings'] },
    ]);
  });

  it('keeps Attractions one click away on the Trips page', async () => {
    signInAs('OperationsManager');
    renderApp('/trips');

    expect(await screen.findByRole('link', { name: 'Attractions' })).toHaveAttribute('href', '/attractions');
  });
});
