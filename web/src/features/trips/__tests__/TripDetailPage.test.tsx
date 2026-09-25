import { screen, waitFor } from '@testing-library/react';
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

  it('shows Start planning when the trip is Submitted', async () => {
    givenTrip('Submitted');
    renderApp(`/trips/${ID}`);

    expect(await screen.findByRole('button', { name: 'Start planning' })).toBeInTheDocument();
    expect(screen.getByText('No itinerary yet')).toBeInTheDocument();
    expect(screen.getByRole('list', { name: 'Status timeline' })).toHaveTextContent('Submitted');
  });

  it.each(['Planning', 'PendingApproval', 'Confirmed'])(
    'hides Start planning when the trip is %s',
    async (status) => {
      givenTrip(status);
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByRole('button', { name: 'Start planning' })).not.toBeInTheDocument();
    },
  );

  it('starts planning and opens the workflow timeline', async () => {
    givenTrip('Submitted');
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/start-planning`, () =>
        HttpResponse.json(
          {
            workflowId: 'wf-1',
            tripRequestId: ID,
            workflowStatus: 'Planning',
            tripStatus: 'Planning',
            skeleton: [],
            errorSummary: null,
          },
          { status: 202 },
        ),
      ),
      http.get(`${API}/api/workflows/wf-1`, () => HttpResponse.json({ title: 'Not found' }, { status: 404 })),
    );
    const { user, location } = renderApp(`/trips/${ID}`);

    await user.click(await screen.findByRole('button', { name: 'Start planning' }));

    expect(await screen.findByText('Planning started. The agents are working on it.')).toBeInTheDocument();
    await waitFor(() => expect(location()).toBe('/workflows/wf-1'));
    // Let the workflow page finish loading before the test ends (its mocked GET answers 404).
    expect(await screen.findByText('Could not load this page')).toBeInTheDocument();
  });
});
