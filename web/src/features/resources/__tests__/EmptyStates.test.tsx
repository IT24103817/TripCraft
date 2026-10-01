import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

describe('Resource empty states', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it.each([
    ['/resources/guides', '/api/guides', 'No guides found', 'Add guide'],
    ['/resources/vehicles', '/api/vehicles', 'No vehicles found', 'Add vehicle'],
    ['/resources/hotels', '/api/hotels', 'No hotels found', 'Add hotel'],
  ])('%s: the empty state opens the create dialog', async (path, api, emptyTitle, action) => {
    server.use(http.get(`${API}${api}`, () => HttpResponse.json(paged([]))));
    const { user } = renderApp(path);

    const empty = (await screen.findByText(emptyTitle)).closest('div')!;
    await user.click(within(empty).getByRole('button', { name: action }));

    expect(await screen.findByRole('dialog', { name: action })).toBeInTheDocument();
  });
});
