import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';
import { toIsoDate } from '@/shared/utils/format';

describe('Resource empty states and the hold calendar limit', () => {
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

  it('says when the window has more holds than the calendar shows', async () => {
    const today = toIsoDate(new Date());
    const hold = {
      id: 'hold1',
      resourceType: 'Vehicle',
      resourceId: 'v1',
      resourceName: 'CAB-1234',
      tripRequestId: null,
      fromDate: today,
      toDate: today,
      quantity: 1,
      status: 'Held',
      note: 'Service',
    };
    let pageSize: string | null = null;
    server.use(
      http.get(`${API}/api/resource-holds`, ({ request }) => {
        pageSize = new URL(request.url).searchParams.get('pageSize');
        return HttpResponse.json(paged([hold], 130, 1, 100));
      }),
    );
    renderApp('/availability');

    expect(
      await screen.findByText('Showing the first 1 of 130 holds in this window — narrow the dates.'),
    ).toBeInTheDocument();
    expect(pageSize).toBe('100');
  });

  it('shows no notice when every hold in the window is shown', async () => {
    server.use(http.get(`${API}/api/resource-holds`, () => HttpResponse.json(paged([]))));
    renderApp('/availability');

    expect(await screen.findByText('Nothing is held in these two weeks')).toBeInTheDocument();
    expect(screen.queryByText(/Showing the first/)).not.toBeInTheDocument();
  });
});
