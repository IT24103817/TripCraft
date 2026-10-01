import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const NIMAL = {
  id: 'g1',
  name: 'Nimal Perera',
  phone: '+94 77 123 4567',
  languages: ['en', 'si'],
  dayRateLkr: 6000,
  maxPax: 10,
  isActive: true,
};

describe('GuidesPage', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('lists guides from the API and sends the language filter', async () => {
    const requests: URLSearchParams[] = [];
    server.use(
      http.get(`${API}/api/guides`, ({ request }) => {
        requests.push(new URL(request.url).searchParams);
        return HttpResponse.json(paged([NIMAL]));
      }),
    );
    const { user } = renderApp('/resources/guides');

    const table = await screen.findByRole('table', { name: 'Guides' });
    expect(within(table).getByText('Nimal Perera')).toBeInTheDocument();
    expect(within(table).getByText('en, si')).toBeInTheDocument();

    await user.selectOptions(screen.getByLabelText('Language'), 'de');
    await waitFor(() => expect(requests.at(-1)?.get('language')).toBe('de'));
  });

  it('validates the form like the API, then shows the one-time temporary password', async () => {
    server.use(http.get(`${API}/api/guides`, () => HttpResponse.json(paged([]))));
    let body: unknown = null;
    server.use(
      http.post(`${API}/api/guides`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(
          {
            guide: { ...NIMAL, id: 'g2', name: 'Sunil' },
            email: 'sunil@tripcraft.test',
            temporaryPassword: 'Temp-4821-Kx',
          },
          { status: 201 },
        );
      }),
    );
    const { user } = renderApp('/resources/guides');

    await user.click(await screen.findByRole('button', { name: 'Add guide' }));
    const dialog = await screen.findByRole('dialog', { name: 'Add guide' });
    await user.type(within(dialog).getByLabelText('Name'), 'Sunil');
    await user.type(within(dialog).getByLabelText('Login email'), 'not-an-email');
    await user.type(within(dialog).getByLabelText('Phone'), '12');
    await user.clear(within(dialog).getByLabelText('Languages'));
    await user.type(within(dialog).getByLabelText('Languages'), 'english');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByText(/Phone must be 7–20 digits/)).toBeInTheDocument();
    expect(within(dialog).getByText(/Use two-letter codes/)).toBeInTheDocument();
    expect(within(dialog).getByText('Enter a valid email address.')).toBeInTheDocument();
    expect(body).toBeNull();

    await user.clear(within(dialog).getByLabelText('Login email'));
    await user.type(within(dialog).getByLabelText('Login email'), 'sunil@tripcraft.test');
    await user.clear(within(dialog).getByLabelText('Phone'));
    await user.type(within(dialog).getByLabelText('Phone'), '+94 77 000 1111');
    await user.clear(within(dialog).getByLabelText('Languages'));
    await user.type(within(dialog).getByLabelText('Languages'), 'en, IT');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Added Sunil.')).toBeInTheDocument();
    expect(body).toEqual({
      name: 'Sunil',
      email: 'sunil@tripcraft.test',
      phone: '+94 77 000 1111',
      languages: ['en', 'it'],
      dayRateLkr: 6000,
      maxPax: 8,
      isActive: true,
    });

    const shown = await screen.findByRole('dialog', { name: 'Guide login created' });
    expect(shown).toHaveTextContent('sunil@tripcraft.test');
    expect(shown).toHaveTextContent('Temp-4821-Kx');
    expect(within(shown).getByRole('alert')).toHaveTextContent('This password is shown only once.');

    await user.click(within(shown).getByRole('button', { name: 'Copy password' }));
    expect(await within(shown).findByText('Copied to the clipboard.')).toBeInTheDocument();
    expect(await navigator.clipboard.readText()).toBe('Temp-4821-Kx');

    // The ready-made message for the guide holds the login and the password.
    const message =
      'Your TripCraft guide login: sunil@tripcraft.test / temporary password Temp-4821-Kx. ' +
      'Open the TripCraft app and sign in; you will be asked to choose a new password.';
    const share = within(shown).getByRole('region', { name: 'Share with guide' });
    expect(share).toHaveTextContent(message);
    await user.click(within(share).getByRole('button', { name: 'Copy message' }));
    expect(await within(shown).findByText('Message copied to the clipboard.')).toBeInTheDocument();
    expect(await navigator.clipboard.readText()).toBe(message);

    await user.click(within(shown).getByRole('button', { name: 'Done' }));
    expect(screen.queryByText('Temp-4821-Kx')).not.toBeInTheDocument();
  });

  it('edits a guide without any login fields (no userId is sent)', async () => {
    let body: unknown = null;
    server.use(
      http.get(`${API}/api/guides`, () => HttpResponse.json(paged([NIMAL]))),
      http.put(`${API}/api/guides/g1`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(NIMAL);
      }),
    );
    const { user } = renderApp('/resources/guides');

    await user.click(await screen.findByRole('button', { name: /Open Nimal Perera/ }));
    const dialog = await screen.findByRole('dialog', { name: 'Edit guide' });
    expect(within(dialog).queryByLabelText('Login email')).not.toBeInTheDocument();
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Saved Nimal Perera.')).toBeInTheDocument();
    expect(body).toEqual({
      name: 'Nimal Perera',
      phone: '+94 77 123 4567',
      languages: ['en', 'si'],
      dayRateLkr: 6000,
      maxPax: 10,
      isActive: true,
    });
  });

  it('resets a guide password and shows the new temporary password once', async () => {
    let resetId = '';
    server.use(
      http.get(`${API}/api/guides`, () => HttpResponse.json(paged([NIMAL]))),
      http.post(`${API}/api/guides/:id/reset-password`, ({ params }) => {
        resetId = String(params.id);
        return HttpResponse.json({
          guide: NIMAL,
          email: 'guide1@tripcraft.test',
          temporaryPassword: 'New-9310-Qp',
        });
      }),
    );
    const { user } = renderApp('/resources/guides');

    await user.click(await screen.findByRole('button', { name: 'Reset password for Nimal Perera' }));
    const confirm = await screen.findByRole('dialog', { name: 'Reset password' });
    await user.click(within(confirm).getByRole('button', { name: 'Reset password' }));

    const shown = await screen.findByRole('dialog', { name: 'Password reset' });
    expect(shown).toHaveTextContent('guide1@tripcraft.test');
    expect(shown).toHaveTextContent('New-9310-Qp');
    expect(resetId).toBe('g1');
  });

  it('shows the API message when deleting a held guide is refused (409)', async () => {
    server.use(
      http.get(`${API}/api/guides`, () => HttpResponse.json(paged([NIMAL]))),
      http.delete(`${API}/api/guides/g1`, () =>
        HttpResponse.json(
          {
            title: 'Conflict',
            status: 409,
            detail: 'This guide is held for an upcoming trip; release the hold first.',
          },
          { status: 409 },
        ),
      ),
    );
    const { user } = renderApp('/resources/guides');

    await user.click(await screen.findByRole('button', { name: 'Delete Nimal Perera' }));
    const dialog = await screen.findByRole('dialog', { name: 'Delete guide' });
    await user.click(within(dialog).getByRole('button', { name: 'Delete' }));

    expect(await screen.findByText(/held for an upcoming trip/)).toBeInTheDocument();
  });
});
