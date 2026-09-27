import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { attraction, itinerary, paged, trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ID = trip().id;
const KANDY = [
  attraction('a1', 'Temple of the Tooth'),
  attraction('a2', 'Royal Botanical Gardens'),
  attraction('a3', 'Kandy Lake'),
  attraction('a4', 'Bahirawakanda Buddha'),
];
const DAY_URL = `${API}/api/trip-requests/${ID}/itinerary/days/:dayNumber`;

function givenTrip(status: string, attractions: () => Response = () => HttpResponse.json(paged(KANDY))) {
  let saved = itinerary();
  server.use(
    http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status }))),
    http.get(`${API}/api/trip-requests/${ID}/history`, () => HttpResponse.json([])),
    http.get(`${API}/api/trip-requests/${ID}/itinerary`, () => HttpResponse.json(saved)),
    http.get(`${API}/api/attractions`, attractions),
  );
  return { setSaved: (next: ReturnType<typeof itinerary>) => (saved = next) };
}

async function openDayOne(user: ReturnType<typeof renderApp>['user']) {
  await user.click(await screen.findByRole('button', { name: 'Edit day 1' }));
  return screen.findByRole('dialog', { name: 'Edit day 1 — Kandy' });
}

describe('Itinerary editor', () => {
  it('lists the city attractions with the current stops ticked and allows at most 3', async () => {
    signInAs('OperationsManager');
    let city: string | null = null;
    givenTrip('Confirmed');
    server.use(
      http.get(`${API}/api/attractions`, ({ request }) => {
        city = new URL(request.url).searchParams.get('city');
        return HttpResponse.json(paged(KANDY));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const dialog = await openDayOne(user);
    const temple = await within(dialog).findByRole('checkbox', { name: /Temple of the Tooth/ });
    expect(city).toBe('Kandy');
    expect(temple).toBeChecked();
    expect(within(dialog).getByLabelText('Notes (optional)')).toHaveValue('Arrive and settle in');

    await user.click(within(dialog).getByRole('checkbox', { name: /Royal Botanical Gardens/ }));
    await user.click(within(dialog).getByRole('checkbox', { name: /Kandy Lake/ }));

    expect(within(dialog).getByText(/3 stops chosen — the most for one day/)).toBeInTheDocument();
    expect(within(dialog).getByRole('checkbox', { name: /Bahirawakanda Buddha/ })).toBeDisabled();
  });

  it('saves the day with PUT, shows a toast and the refreshed itinerary', async () => {
    signInAs('OperationsManager');
    const stored = givenTrip('Confirmed');
    let body: unknown = null;
    let dayNumber = '';
    server.use(
      http.put(DAY_URL, async ({ request, params }) => {
        body = await request.json();
        dayNumber = String(params.dayNumber);
        const updated = itinerary({ version: 2, generatedBy: 'Manual' });
        updated.days[0] = {
          ...updated.days[0]!,
          notes: 'Lake walk at sunset',
          stops: [
            {
              sequence: 1,
              arrivalTime: null,
              attractionId: 'a3',
              attractionName: 'Kandy Lake',
              durationMinutes: 90,
            },
          ],
        };
        stored.setSaved(updated);
        return HttpResponse.json(updated);
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const dialog = await openDayOne(user);
    await user.click(await within(dialog).findByRole('checkbox', { name: /Temple of the Tooth/ }));
    await user.click(within(dialog).getByRole('checkbox', { name: /Kandy Lake/ }));
    const notes = within(dialog).getByLabelText('Notes (optional)');
    await user.clear(notes);
    await user.type(notes, 'Lake walk at sunset');
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText('Saved day 1.')).toBeInTheDocument();
    expect(dayNumber).toBe('1');
    expect(body).toEqual({ attractionIds: ['a3'], notes: 'Lake walk at sunset' });
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
    expect(screen.getByText('Version 2 · Manual')).toBeInTheDocument();
    expect(screen.getByText('Kandy Lake (90 min)')).toBeInTheDocument();
  });

  it('asks for at least one attraction before calling the API', async () => {
    signInAs('OperationsManager');
    givenTrip('Confirmed');
    let called = false;
    server.use(http.put(DAY_URL, () => ((called = true), HttpResponse.json(itinerary()))));
    const { user } = renderApp(`/trips/${ID}`);

    const dialog = await openDayOne(user);
    await user.click(await within(dialog).findByRole('checkbox', { name: /Temple of the Tooth/ }));
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(within(dialog).getByRole('alert')).toHaveTextContent('Pick at least one attraction.');
    expect(called).toBe(false);
  });

  it('shows the API field errors (400) inside the dialog', async () => {
    signInAs('OperationsManager');
    givenTrip('Confirmed');
    server.use(
      http.put(DAY_URL, () =>
        HttpResponse.json(
          {
            title: 'Validation failed',
            status: 400,
            detail: 'Validation failed',
            errors: {
              AttractionIds: ['Each attraction must be active and in Kandy.'],
              Notes: ['Notes must be 500 characters or fewer.'],
            },
          },
          { status: 400 },
        ),
      ),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const dialog = await openDayOne(user);
    await within(dialog).findByRole('checkbox', { name: /Temple of the Tooth/ });
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(
      await within(dialog).findByText('Each attraction must be active and in Kandy.'),
    ).toBeInTheDocument();
    expect(within(dialog).getByText('Notes must be 500 characters or fewer.')).toBeInTheDocument();
    expect(within(dialog).getByLabelText('Notes (optional)')).toHaveAttribute('aria-invalid', 'true');
  });

  it('shows the API message when the trip can no longer be edited (409)', async () => {
    signInAs('OperationsManager');
    givenTrip('Confirmed');
    server.use(
      http.put(DAY_URL, () =>
        HttpResponse.json(
          { title: 'Conflict', status: 409, detail: 'Only a Confirmed trip itinerary can be edited.' },
          { status: 409 },
        ),
      ),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const dialog = await openDayOne(user);
    await within(dialog).findByRole('checkbox', { name: /Temple of the Tooth/ });
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'Only a Confirmed trip itinerary can be edited.',
    );
  });

  it('shows the empty and error states of the attraction list', async () => {
    signInAs('OperationsManager');
    givenTrip('Confirmed', () => HttpResponse.json(paged([])));
    const empty = renderApp(`/trips/${ID}`);
    const dialog = await openDayOne(empty.user);
    expect(await within(dialog).findByText('No attractions in Kandy')).toBeInTheDocument();
    empty.unmount();

    givenTrip('Confirmed', () => HttpResponse.json({ title: 'Boom' }, { status: 500 }));
    const failing = renderApp(`/trips/${ID}`);
    const failed = await openDayOne(failing.user);
    expect(await within(failed).findByRole('button', { name: 'Retry' })).toBeInTheDocument();
  });

  it.each(['Submitted', 'Planning', 'PendingApproval', 'Approved', 'InProgress', 'Completed'])(
    'offers no Edit day button when the trip is %s',
    async (status) => {
      signInAs('OperationsManager');
      givenTrip(status);
      renderApp(`/trips/${ID}`);

      expect(await screen.findByText('Temple of the Tooth (90 min)')).toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /Edit day/ })).not.toBeInTheDocument();
    },
  );

  it('is not available to an Admin (the trip page is Operations Manager only)', async () => {
    signInAs('Admin');
    givenTrip('Confirmed');
    renderApp(`/trips/${ID}`);

    expect(await screen.findByText('403')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Edit day/ })).not.toBeInTheDocument();
  });
});
