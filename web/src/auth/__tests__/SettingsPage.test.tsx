import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const SETTINGS = {
  llmProvider: 'ollama',
  cancellationCutoffDays: 3,
  marginPct: 15,
  depositPct: 30,
  operatorContact: 'ops@tripcraft.test, +94 11 234 5678',
  updatedAt: '2026-10-01T08:00:00Z',
};

describe('SettingsPage', () => {
  it('loads the settings, refuses invalid values and saves valid ones', async () => {
    signInAs('Admin');
    let body: unknown = null;
    server.use(
      http.get(`${API}/api/admin/settings`, () => HttpResponse.json(SETTINGS)),
      http.put(`${API}/api/admin/settings`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ ...(body as object), updatedAt: '2026-10-02T09:00:00Z' });
      }),
    );
    const { user } = renderApp('/admin/settings');

    const form = await screen.findByRole('form', { name: 'Settings' });
    expect(within(form).getByLabelText(/Ollama/)).toBeChecked();
    expect(within(form).getByLabelText('Margin (%)')).toHaveValue(15);

    await user.clear(within(form).getByLabelText('Cancellation notice (days)'));
    await user.type(within(form).getByLabelText('Cancellation notice (days)'), '31');
    await user.clear(within(form).getByLabelText('Deposit (%)'));
    await user.type(within(form).getByLabelText('Deposit (%)'), '120');
    await user.clear(within(form).getByLabelText('Operator contact'));
    await user.click(within(form).getByRole('button', { name: 'Save settings' }));

    expect(await within(form).findByText('Must be 0–30 days.')).toBeInTheDocument();
    expect(within(form).getByText('Must be 0–100.')).toBeInTheDocument();
    expect(within(form).getByText('The operator contact is required.')).toBeInTheDocument();
    expect(body).toBeNull();

    await user.clear(within(form).getByLabelText('Cancellation notice (days)'));
    await user.type(within(form).getByLabelText('Cancellation notice (days)'), '5');
    await user.clear(within(form).getByLabelText('Deposit (%)'));
    await user.type(within(form).getByLabelText('Deposit (%)'), '25');
    await user.type(within(form).getByLabelText('Operator contact'), 'ops@tripcraft.test');
    await user.click(within(form).getByLabelText(/Groq/));
    await user.click(within(form).getByRole('button', { name: 'Save settings' }));

    expect(await screen.findByText('Settings saved.')).toBeInTheDocument();
    expect(body).toEqual({
      llmProvider: 'groq',
      cancellationCutoffDays: 5,
      marginPct: 15,
      depositPct: 25,
      operatorContact: 'ops@tripcraft.test',
    });
  });

  it('shows the error state with Retry when the settings cannot be loaded', async () => {
    signInAs('Admin');
    server.use(
      http.get(`${API}/api/admin/settings`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })),
    );
    renderApp('/admin/settings');

    expect(await screen.findByRole('button', { name: 'Retry' })).toBeInTheDocument();
  });

  it('is 403 for an Operations Manager, who has no Settings link', async () => {
    signInAs('OperationsManager');
    renderApp('/admin/settings');

    expect(
      await screen.findByRole('heading', { name: 'You do not have access to this page' }),
    ).toBeInTheDocument();
    const nav = screen.getByRole('navigation', { name: 'Main' });
    expect(within(nav).queryByRole('link', { name: 'Settings' })).not.toBeInTheDocument();
  });
});
