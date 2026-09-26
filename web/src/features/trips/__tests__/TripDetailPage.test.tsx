import { screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ID = trip().id;

function givenTrip(status: string) {
  server.use(
    http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status }))),
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
    },
  );
});
