import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const NIMAL = {
  id: 'g1',
  userId: 'u1',
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

  it('validates the form like the API before saving a new guide', async () => {
    server.use(http.get(`${API}/api/guides`, () => HttpResponse.json(paged([]))));
    let body: unknown = null;
    server.use(
      http.post(`${API}/api/guides`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ ...NIMAL, id: 'g2', name: 'Sunil' }, { status: 201 });
      }),
    );
    const { user } = renderApp('/resources/guides');

    await user.click(await screen.findByRole('button', { name: 'Add guide' }));
    const dialog = await screen.findByRole('dialog', { name: 'Add guide' });
    await user.type(within(dialog).getByLabelText('Name'), 'Sunil');
    await user.type(within(dialog).getByLabelText('Phone'), '12');
    await user.clear(within(dialog).getByLabelText('Languages'));
    await user.type(within(dialog).getByLabelText('Languages'), 'english');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByText(/Phone must be 7–20 digits/)).toBeInTheDocument();
    expect(within(dialog).getByText(/Use two-letter codes/)).toBeInTheDocument();
    expect(body).toBeNull();

    await user.clear(within(dialog).getByLabelText('Phone'));
    await user.type(within(dialog).getByLabelText('Phone'), '+94 77 000 1111');
    await user.clear(within(dialog).getByLabelText('Languages'));
    await user.type(within(dialog).getByLabelText('Languages'), 'en, IT');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Added Sunil.')).toBeInTheDocument();
    expect(body).toMatchObject({ name: 'Sunil', languages: ['en', 'it'], userId: null });
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
