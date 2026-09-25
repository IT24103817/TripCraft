import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

describe('Attraction form', () => {
  let posted: unknown[];

  beforeEach(() => {
    signInAs('OperationsManager');
    posted = [];
    server.use(
      http.get(`${API}/api/attractions`, () => HttpResponse.json(paged([]))),
      http.post(`${API}/api/attractions`, async ({ request }) => {
        posted.push(await request.json());
        return HttpResponse.json({ id: 'a-new', ...(posted[0] as object) }, { status: 201 });
      }),
    );
  });

  it('blocks submit on invalid input and shows field errors', async () => {
    const { user } = renderApp('/attractions');
    await user.click((await screen.findAllByRole('button', { name: 'Add attraction' }))[0]!);

    await user.clear(screen.getByLabelText('Duration (minutes)'));
    await user.type(screen.getByLabelText('Duration (minutes)'), '5');
    await user.clear(screen.getByLabelText('Latitude'));
    await user.type(screen.getByLabelText('Latitude'), '95');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Name is required.')).toBeInTheDocument();
    expect(screen.getByText('At least 15 minutes.')).toBeInTheDocument();
    expect(screen.getByText('Between -90 and 90.')).toBeInTheDocument();
    expect(posted).toHaveLength(0);
  });

  it('saves a valid attraction and shows a success toast', async () => {
    const { user } = renderApp('/attractions');
    await user.click((await screen.findAllByRole('button', { name: 'Add attraction' }))[0]!);

    const dialog = screen.getByRole('dialog', { name: 'Add attraction' });
    await user.type(within(dialog).getByLabelText('Name'), 'Sigiriya Rock');
    await user.type(within(dialog).getByLabelText('City'), 'Sigiriya');
    await user.type(within(dialog).getByLabelText('Category'), 'Heritage');
    expect(within(dialog).getByTitle('Map of Sigiriya Rock')).toHaveAttribute(
      'src',
      expect.stringContaining('marker=7.2906,80.6337'),
    );
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Added Sigiriya Rock.')).toBeInTheDocument();
    expect(posted[0]).toMatchObject({ name: 'Sigiriya Rock', durationMinutes: 60, entryFeeLkr: 0 });
  });
});
