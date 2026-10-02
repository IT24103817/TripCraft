import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { trip, WORKFLOW_ID } from '@/test/fixtures';
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
    toStatus: 'NeedsOperator',
    reason: 'The agent service did not answer in time.',
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
    // The itinerary loads inside the Overview tab, after the trip itself.
    expect(await screen.findByText('No itinerary yet')).toBeInTheDocument();
    expect(screen.getByRole('list', { name: 'Status timeline' })).toHaveTextContent('Submitted');
  });

  it.each(['Planning', 'NeedsOperator', 'Confirmed'])(
    'does not say it waits for the tourist when the trip is %s',
    async (status) => {
      givenTrip(status);
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByText(/Waiting for the tourist/)).not.toBeInTheDocument();
    },
  );

  it.each(['Planning', 'InProgress', 'Completed', 'Cancelled'])(
    'offers no Cancel request button when the trip is %s',
    async (status) => {
      givenTrip(status);
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByRole('button', { name: 'Cancel request' })).not.toBeInTheDocument();
    },
  );

  it('shows the cities and what the status means', async () => {
    givenTrip('QuotationSent');
    renderApp(`/trips/${ID}`);

    expect(await screen.findByText('Kandy, Ella')).toBeInTheDocument();
    expect(screen.getByText(/Waiting for the client to accept or decline/)).toBeInTheDocument();
    expect(screen.getByRole('list', { name: 'Status timeline' })).toHaveTextContent('Quotation sent');
    // Nothing for the manager to do while the client decides.
    expect(screen.queryByRole('region', { name: /Next step/ })).not.toBeInTheDocument();
  });

  it('draws the main path Submitted → Completed in the status timeline', async () => {
    givenTrip('ClientAccepted');
    renderApp(`/trips/${ID}`);

    const timeline = await screen.findByRole('list', { name: 'Status timeline' });
    expect(
      within(timeline)
        .getAllByRole('listitem')
        .map((li) => li.textContent?.replace(/^[✓\d]+/, '')),
    ).toEqual([
      'Submitted',
      'Planning',
      'Quotation sent',
      'Accepted',
      'Confirmed',
      'In progress',
      'Completed',
    ]);
    expect(within(timeline).getByText('Accepted').closest('li')).toHaveAttribute('aria-current', 'step');
  });

  it.each([
    ['NeedsOperator', ['Submitted', 'Planning', 'Needs operator']],
    ['ClientDeclined', ['Submitted', 'Planning', 'Quotation sent', 'Declined']],
  ])('shows the side state %s after the step it branches off from', async (status, steps) => {
    givenTrip(status);
    renderApp(`/trips/${ID}`);

    const timeline = await screen.findByRole('list', { name: 'Status timeline' });
    expect(
      within(timeline)
        .getAllByRole('listitem')
        .map((li) => li.textContent?.replace(/^[✓\d]+/, '')),
    ).toEqual(steps);
  });

  it('links a trip with a proposal to its review page', async () => {
    givenTrip('QuotationSent');
    server.use(
      http.get(`${API}/api/trip-requests/${ID}/workflow`, () => HttpResponse.json({ id: WORKFLOW_ID })),
    );
    renderApp(`/trips/${ID}`);

    expect(await screen.findByRole('link', { name: 'Open review' })).toHaveAttribute(
      'href',
      `/approvals/${WORKFLOW_ID}`,
    );
  });

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
    expect(items[2]).toHaveTextContent('Reason: The agent service did not answer in time.');
  });

  it('cancels a trip only with a reason, and sends the reason', async () => {
    givenTrip('Confirmed');
    let body: unknown = null;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/cancel`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(trip({ status: 'Cancelled' }));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    await user.click(await screen.findByRole('button', { name: 'Cancel request' }));
    const dialog = await screen.findByRole('dialog', { name: 'Cancel trip request' });
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));
    expect(within(dialog).getByRole('alert')).toHaveTextContent('Give a reason; the tourist sees it.');
    expect(body).toBeNull();

    await user.type(within(dialog).getByLabelText(/Reason/), 'Flooding on the Kandy road');
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));

    expect(await screen.findByText('Trip request cancelled.')).toBeInTheDocument();
    expect(body).toEqual({ reason: 'Flooding on the Kandy road' });
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
    await user.type(within(dialog).getByLabelText(/Reason/), 'Tourist asked by phone');
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));

    expect(await screen.findByText(/Only a Submitted trip request can be cancelled/)).toBeInTheDocument();
  });

  describe('PDF downloads', () => {
    // jsdom has no object URLs; the download link only needs some URL. Put the originals back afterwards.
    const original = { create: URL.createObjectURL, revoke: URL.revokeObjectURL };
    afterEach(() => {
      URL.createObjectURL = original.create;
      URL.revokeObjectURL = original.revoke;
    });

    it('downloads the vouchers PDF of a confirmed trip with the bearer token', async () => {
      givenTrip('Confirmed');
      let authorization: string | null = null;
      server.use(
        http.get(`${API}/api/trips/${ID}/vouchers.pdf`, ({ request }) => {
          authorization = request.headers.get('Authorization');
          return new HttpResponse(new Blob(['%PDF-1.4'], { type: 'application/pdf' }), {
            headers: { 'Content-Type': 'application/pdf' },
          });
        }),
      );
      const createObjectURL = vi.fn(() => 'blob:vouchers');
      URL.createObjectURL = createObjectURL;
      URL.revokeObjectURL = vi.fn();
      const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);
      const { user } = renderApp(`/trips/${ID}`);

      await user.click(await screen.findByRole('button', { name: 'Download vouchers (PDF)' }));

      await waitFor(() => expect(click).toHaveBeenCalled());
      expect(authorization).toBe('Bearer test-token');
      expect(createObjectURL).toHaveBeenCalled();
      click.mockRestore();
    });

    it('offers no vouchers before the trip is confirmed', async () => {
      givenTrip('ClientAccepted');
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByRole('button', { name: 'Download vouchers (PDF)' })).not.toBeInTheDocument();
    });

    it('downloads the itinerary PDF once a quotation was sent, with the bearer token', async () => {
      givenTrip('QuotationSent');
      let authorization: string | null = null;
      server.use(
        http.get(`${API}/api/trips/${ID}/itinerary.pdf`, ({ request }) => {
          authorization = request.headers.get('Authorization');
          return new HttpResponse(new Blob(['%PDF-1.4'], { type: 'application/pdf' }), {
            headers: { 'Content-Type': 'application/pdf' },
          });
        }),
      );
      URL.createObjectURL = vi.fn(() => 'blob:itinerary');
      URL.revokeObjectURL = vi.fn();
      const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => undefined);
      const { user } = renderApp(`/trips/${ID}`);

      await user.click(await screen.findByRole('button', { name: 'Download itinerary PDF' }));

      await waitFor(() => expect(click).toHaveBeenCalled());
      expect(authorization).toBe('Bearer test-token');
      click.mockRestore();
    });

    it.each(['Submitted', 'NeedsOperator', 'ClientDeclined', 'Cancelled'])(
      'offers no itinerary PDF before a quotation is sent (%s)',
      async (status) => {
        givenTrip(status);
        renderApp(`/trips/${ID}`);

        expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Download itinerary PDF' })).not.toBeInTheDocument();
      },
    );
  });
});
