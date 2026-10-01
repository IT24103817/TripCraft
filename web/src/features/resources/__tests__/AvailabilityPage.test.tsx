import { act, screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';
import { toIsoDate } from '@/shared/utils/format';
import { addDays, formatDayMonth, startOfWeek } from '../availabilityDates';

const MONDAY = startOfWeek(toIsoDate(new Date()));
const TUESDAY = addDays(MONDAY, 1);
const WEDNESDAY = addDays(MONDAY, 2);

function free(date: string, capacity = 1) {
  return { date, state: 'Free', heldQuantity: 0, freeQuantity: capacity };
}

/** Days from..to; every cell Free except the ones given per resource id and date. */
function grid(from: string, to: string, special: Record<string, Record<string, object>> = {}) {
  const days: string[] = [];
  for (let day = from; day <= to; day = addDays(day, 1)) days.push(day);
  const row = (resourceType: string, resourceId: string, name: string, detail: string, capacity: number) => ({
    resourceType,
    resourceId,
    name,
    detail,
    capacity,
    cells: days.map((date) => ({ ...free(date, capacity), ...special[resourceId]?.[date] })),
  });
  return {
    days,
    rows: [
      row('Guide', 'g1', 'Nimal Perera', 'en, de · up to 8', 1),
      row('Vehicle', 'v1', 'CAB-1234', 'Van · 6 seats', 1),
      row('Room', 'r1', 'Kandy Hills — Standard Double', 'Kandy · sleeps 2', 5),
    ],
  };
}

const SPECIAL = {
  g1: {
    [MONDAY]: {
      state: 'Held',
      holdId: 'hold1',
      tripRequestId: 'trip-1',
      touristName: 'Anna Silva',
      tripStatus: 'QuotationSent',
      heldQuantity: 1,
      freeQuantity: 0,
    },
  },
  v1: {
    [TUESDAY]: { state: 'Blocked', holdId: 'hold9', note: 'Maintenance', heldQuantity: 1, freeQuantity: 0 },
  },
  r1: {
    [MONDAY]: {
      state: 'Confirmed',
      holdId: 'hold3',
      tripRequestId: 'trip-2',
      touristName: 'Ben Fernando',
      tripStatus: 'Confirmed',
      heldQuantity: 2,
      freeQuantity: 3,
    },
  },
};

/** Answers GET /api/availability/grid for the asked range and records each query. */
function givenGrid() {
  const requests: URLSearchParams[] = [];
  server.use(
    http.get(`${API}/api/availability/grid`, ({ request }) => {
      const params = new URL(request.url).searchParams;
      requests.push(params);
      return HttpResponse.json(grid(params.get('from')!, params.get('to')!, SPECIAL));
    }),
  );
  return requests;
}

const cell = (name: RegExp) => screen.findByRole('button', { name });

describe('Availability grid', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows a week of guides, vehicles and room types with a text cue and a tooltip per cell', async () => {
    const requests = givenGrid();
    const { user } = renderApp('/availability');

    const table = await screen.findByRole('table', { name: 'Availability grid' });
    expect(requests[0]?.get('from')).toBe(MONDAY);
    expect(requests[0]?.get('to')).toBe(addDays(MONDAY, 6));
    for (const group of ['Guides', 'Vehicles', 'Hotel room types']) {
      expect(within(table).getByRole('columnheader', { name: group })).toBeInTheDocument();
    }

    const held = await cell(
      new RegExp(
        `^Nimal Perera, ${formatDayMonth(MONDAY)}: Held for trip of Anna Silva \\(Quotation sent\\)$`,
      ),
    );
    expect(held).toHaveTextContent('◐');
    await user.hover(held);
    const tooltip = await screen.findByRole('tooltip');
    expect(tooltip).toHaveTextContent('Held for trip of Anna Silva (Quotation sent)');
    expect(held).toHaveAttribute('aria-describedby', tooltip.id);
    await user.unhover(held);
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    const room = await cell(
      new RegExp(`^Kandy Hills — Standard Double, ${formatDayMonth(MONDAY)}: Confirmed`),
    );
    expect(room).toHaveTextContent('✓3');
    // Keyboard users get the same tooltip on focus.
    act(() => room.focus());
    expect(await screen.findByRole('tooltip')).toHaveTextContent('3 of 5 rooms free');
  });

  it('switches to the month and moves between months, and sends the filters', async () => {
    const requests = givenGrid();
    const { user } = renderApp('/availability');
    await screen.findByRole('table', { name: 'Availability grid' });

    await user.click(screen.getByRole('button', { name: 'Month' }));
    const first = `${MONDAY.slice(0, 7)}-01`;
    await waitFor(() => expect(requests.at(-1)?.get('from')).toBe(first));
    await user.click(screen.getByRole('button', { name: 'Next month' }));
    const next = new Date(`${first}T00:00:00`);
    next.setMonth(next.getMonth() + 1);
    await waitFor(() => expect(requests.at(-1)?.get('from')).toBe(toIsoDate(next)));

    await user.selectOptions(screen.getByLabelText('Resource type'), 'Vehicle');
    await user.type(screen.getByLabelText('Min seats (vehicles)'), '6');
    await waitFor(() => expect(requests.at(-1)?.get('seats')).toBe('6'));
    expect(requests.at(-1)?.get('type')).toBe('Vehicle');
  });

  it('blocks a free cell from the side panel', async () => {
    givenGrid();
    let body: unknown = null;
    server.use(
      http.post(`${API}/api/resource-holds`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ id: 'hold10' }, { status: 201 });
      }),
    );
    const { user } = renderApp('/availability');

    await user.click(await cell(new RegExp(`^CAB-1234, ${formatDayMonth(WEDNESDAY)}: Free$`)));
    const panel = await screen.findByRole('dialog', { name: `CAB-1234, ${formatDayMonth(WEDNESDAY)}` });
    const form = within(panel).getByRole('form', { name: 'Block' });
    await user.selectOptions(within(form).getByLabelText('Reason'), 'Other');
    await user.click(within(form).getByRole('button', { name: 'Block' }));
    expect(await within(form).findByText('Write what the block is for.')).toBeInTheDocument();
    expect(body).toBeNull();

    await user.selectOptions(within(form).getByLabelText('Reason'), 'Maintenance');
    await user.type(within(form).getByLabelText('Note'), 'Brake service');
    await user.click(within(form).getByRole('button', { name: 'Block' }));

    expect(await screen.findByText('Blocked CAB-1234.')).toBeInTheDocument();
    expect(body).toEqual({
      resourceType: 'Vehicle',
      resourceId: 'v1',
      fromDate: WEDNESDAY,
      toDate: WEDNESDAY,
      quantity: 1,
      note: 'Maintenance: Brake service',
    });
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('edits and releases a manual block', async () => {
    givenGrid();
    let updated: unknown = null;
    let released = false;
    server.use(
      http.get(`${API}/api/resource-holds/hold9`, () =>
        HttpResponse.json({
          id: 'hold9',
          resourceType: 'Vehicle',
          resourceId: 'v1',
          resourceName: 'CAB-1234',
          tripRequestId: null,
          fromDate: TUESDAY,
          toDate: TUESDAY,
          quantity: 1,
          status: 'Held',
          note: 'Maintenance',
        }),
      ),
      http.put(`${API}/api/resource-holds/hold9`, async ({ request }) => {
        updated = await request.json();
        return HttpResponse.json({ id: 'hold9' });
      }),
      http.post(`${API}/api/resource-holds/hold9/release`, () => {
        released = true;
        return HttpResponse.json({ id: 'hold9', status: 'Released' });
      }),
    );
    const { user } = renderApp('/availability');
    const blocked = new RegExp(`^CAB-1234, ${formatDayMonth(TUESDAY)}: Blocked: Maintenance$`);

    await user.click(await cell(blocked));
    let panel = await screen.findByRole('dialog', { name: `CAB-1234, ${formatDayMonth(TUESDAY)}` });
    const form = await within(panel).findByRole('form', { name: 'Edit block' });
    expect(within(form).getByLabelText('Note (required)')).toHaveValue('Maintenance');
    await user.clear(within(form).getByLabelText('To'));
    await user.type(within(form).getByLabelText('To'), WEDNESDAY);
    await user.click(within(form).getByRole('button', { name: 'Save block' }));

    expect(await screen.findByText('Saved the block on CAB-1234.')).toBeInTheDocument();
    expect(updated).toEqual({ fromDate: TUESDAY, toDate: WEDNESDAY, quantity: 1, note: 'Maintenance' });

    await user.click(await cell(blocked));
    panel = await screen.findByRole('dialog', { name: `CAB-1234, ${formatDayMonth(TUESDAY)}` });
    await user.click(await within(panel).findByRole('button', { name: 'Release block' }));
    await user.click(within(panel).getByRole('button', { name: 'Yes, release' }));

    expect(await screen.findByText('Released CAB-1234.')).toBeInTheDocument();
    expect(released).toBe(true);
  });

  it("shows a trip's hold with a link to the trip, and no block form", async () => {
    givenGrid();
    const { user } = renderApp('/availability');

    await user.click(await cell(new RegExp(`^Nimal Perera, ${formatDayMonth(MONDAY)}: Held`)));
    const panel = await screen.findByRole('dialog', { name: `Nimal Perera, ${formatDayMonth(MONDAY)}` });

    expect(panel).toHaveTextContent('Anna Silva');
    expect(within(panel).getByRole('link', { name: 'Open trip' })).toHaveAttribute('href', '/trips/trip-1');
    expect(within(panel).queryByRole('form')).not.toBeInTheDocument();
  });
});
