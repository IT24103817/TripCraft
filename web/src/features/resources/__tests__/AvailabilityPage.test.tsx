import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';
import { toIsoDate } from '@/shared/utils/format';

const today = toIsoDate(new Date());
const HOLD = {
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

describe('AvailabilityPage', () => {
  beforeEach(() => {
    signInAs('OperationsManager');
    server.use(http.get(`${API}/api/resource-holds`, () => HttpResponse.json(paged([HOLD]))));
  });

  it('searches free vehicles by seats and blocks one', async () => {
    let searched: URLSearchParams | null = null;
    let blocked: unknown = null;
    server.use(
      http.get(`${API}/api/availability`, ({ request }) => {
        searched = new URL(request.url).searchParams;
        return HttpResponse.json([
          {
            type: 'Vehicle',
            id: 'v2',
            name: 'NC-4455',
            detail: 'Coach · 15 seats',
            rateLkr: 180,
            freeRooms: null,
          },
        ]);
      }),
      http.post(`${API}/api/resource-holds`, async ({ request }) => {
        blocked = await request.json();
        return HttpResponse.json({ ...HOLD, id: 'hold2', resourceName: 'NC-4455' }, { status: 201 });
      }),
    );
    const { user } = renderApp('/availability');

    const form = await screen.findByRole('form', { name: 'Availability search' });
    await user.clear(within(form).getByLabelText('Seats'));
    await user.type(within(form).getByLabelText('Seats'), '10');
    await user.click(within(form).getByRole('button', { name: 'Search' }));

    const results = await screen.findByRole('list', { name: 'Available resources' });
    expect(results).toHaveTextContent('NC-4455');
    expect(searched!.get('type')).toBe('Vehicle');
    expect(searched!.get('seats')).toBe('10');

    await user.click(within(results).getByRole('button', { name: 'Block NC-4455' }));
    expect(await screen.findByText('Blocked NC-4455.')).toBeInTheDocument();
    expect(blocked).toMatchObject({ resourceType: 'Vehicle', resourceId: 'v2', quantity: 1 });
  });

  it('shows holds in the calendar and releases one', async () => {
    let released = false;
    server.use(
      http.post(`${API}/api/resource-holds/hold1/release`, () => {
        released = true;
        return HttpResponse.json({ ...HOLD, status: 'Released' });
      }),
    );
    const { user } = renderApp('/availability');

    const calendar = await screen.findByRole('table', { name: 'Hold calendar' });
    expect(within(calendar).getByRole('rowheader', { name: 'Vehicle: CAB-1234' })).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Release CAB-1234' }));

    expect(await screen.findByText('Released CAB-1234.')).toBeInTheDocument();
    expect(released).toBe(true);
  });
});
