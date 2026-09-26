import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ID = trip().id;

const HISTORY = [
  {
    at: '2026-09-26T04:12:54Z',
    action: 'TripRequestCreated',
    entity: 'TripRequest',
    actor: 'Tourist',
    fromStatus: null,
    toStatus: 'Submitted',
  },
  {
    at: '2026-09-26T04:12:55Z',
    action: 'TripRequestStatusChanged',
    entity: 'TripRequest',
    actor: 'Tourist',
    fromStatus: 'Submitted',
    toStatus: 'Planning',
  },
  {
    at: '2026-09-26T04:13:43Z',
    action: 'AgentProposalReceived',
    entity: 'AgentWorkflow',
    actor: 'System',
    fromStatus: 'Planning',
    toStatus: 'FailedSafely',
  },
];

function givenTrip(status: string) {
  server.use(
    http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status }))),
    http.get(`${API}/api/trip-requests/${ID}/history`, () => HttpResponse.json(HISTORY)),
    http.get(`${API}/api/trip-requests/${ID}/itinerary`, () =>
      HttpResponse.json(
        { title: 'Not found', detail: 'This trip request has no itinerary yet.' },
        { status: 404 },
      ),
    ),
  );
}

describe('TripDetailPage', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows a Submitted trip as waiting for the tourist, with no Start planning button', async () => {
    givenTrip('Submitted');
    renderApp(`/trips/${ID}`);

    // start-planning is Tourist-only in the API, so staff are not offered a button that would 403.
    expect(
      await screen.findByText('Waiting for the tourist to start planning in the mobile app.'),
    ).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Start planning' })).not.toBeInTheDocument();
    expect(screen.getByText('No itinerary yet')).toBeInTheDocument();
    expect(screen.getByRole('list', { name: 'Status timeline' })).toHaveTextContent('Submitted');
  });

  it.each(['Planning', 'PendingApproval', 'Confirmed'])(
    'does not say it waits for the tourist when the trip is %s',
    async (status) => {
      givenTrip(status);
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByText(/Waiting for the tourist/)).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: 'Cancel request' })).not.toBeInTheDocument();
    },
  );

  it('shows the history of the trip and its workflow, oldest first, with who did it', async () => {
    givenTrip('Submitted');
    renderApp(`/trips/${ID}`);

    const history = await screen.findByRole('list', { name: 'Trip history' });
    const items = within(history).getAllByRole('listitem');
    expect(items).toHaveLength(3);
    expect(items[0]).toHaveTextContent('Trip request submitted');
    expect(items[1]).toHaveTextContent('Status changed');
    expect(items[1]).toHaveTextContent('by Tourist');
    expect(items[2]).toHaveTextContent('Agents returned a proposal');
    expect(items[2]).toHaveTextContent('by System');
  });

  it('cancels a Submitted request after confirmation', async () => {
    givenTrip('Submitted');
    let cancelled = false;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/cancel`, () => {
        cancelled = true;
        return HttpResponse.json(trip({ status: 'Cancelled' }));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    await user.click(await screen.findByRole('button', { name: 'Cancel request' }));
    const dialog = await screen.findByRole('dialog', { name: 'Cancel trip request' });
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));

    expect(await screen.findByText('Trip request cancelled.')).toBeInTheDocument();
    expect(cancelled).toBe(true);
  });

  it('shows the API message when cancelling is refused (409)', async () => {
    givenTrip('Submitted');
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/cancel`, () =>
        HttpResponse.json(
          {
            title: 'Conflict',
            status: 409,
            detail: 'Only a Submitted trip request can be cancelled; this one is Planning.',
          },
          { status: 409 },
        ),
      ),
    );
    const { user } = renderApp(`/trips/${ID}`);

    await user.click(await screen.findByRole('button', { name: 'Cancel request' }));
    const dialog = await screen.findByRole('dialog', { name: 'Cancel trip request' });
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));

    expect(await screen.findByText(/Only a Submitted trip request can be cancelled/)).toBeInTheDocument();
  });
});
