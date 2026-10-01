import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { attraction, paged, pendingWorkflow, QUOTATION_ID, trip, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const TRIP_ID = trip().id;

/** The stored newest version (GET /api/quotations/{id}). */
function quotation(overrides: Record<string, unknown> = {}) {
  return {
    id: QUOTATION_ID,
    tripRequestId: TRIP_ID,
    workflowId: WORKFLOW_ID,
    version: 1,
    status: 'Pending',
    subtotalLkr: 162800,
    marginPct: 15,
    marginLkr: 24420,
    totalLkr: 187220,
    totalUsd: 624.07,
    fxRate: 300,
    fxAsOf: '2026-10-01T00:00:00Z',
    fxStale: false,
    acceptedAt: null,
    lines: [
      {
        lineType: 'guide',
        description: 'Guide Nimal Perera, 5 days',
        qty: 5,
        unitLkr: 6000,
        amountLkr: 30000,
      },
    ],
    decisions: [],
    createdAt: '2026-09-26T04:00:00Z',
    proposalSnapshot: null,
    ...overrides,
  };
}

function decision(decisionName: string, tripStatus: string) {
  return {
    quotationId: QUOTATION_ID,
    tripRequestId: TRIP_ID,
    workflowId: WORKFLOW_ID,
    decision: decisionName,
    tripStatus,
    workflowStatus: 'Approved',
    holdsCreated: 0,
  };
}

function givenReview(options: { workflow?: object; tripStatus?: string; stored?: object | null } = {}) {
  const { workflow = pendingWorkflow(), tripStatus = 'PendingReview', stored = quotation() } = options;
  server.use(
    http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () => HttpResponse.json(workflow)),
    http.get(`${API}/api/trip-requests/${TRIP_ID}`, () => HttpResponse.json(trip({ status: tripStatus }))),
  );
  if (stored) server.use(http.get(`${API}/api/quotations/${QUOTATION_ID}`, () => HttpResponse.json(stored)));
}

const banner = () => screen.findByRole('region', { name: 'Trip status' });

describe('ApprovalReviewPage', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('renders the validation checklist, itinerary and quotation in LKR and USD', async () => {
    givenReview({ stored: null });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(within(checklist).getAllByRole('listitem')).toHaveLength(13);
    expect(checklist).toHaveTextContent('Every day has 1–3 stops — passed');
    expect(screen.getByText('All deterministic checks passed.')).toBeInTheDocument();
    expect(screen.getByText(/Day 2 — Ella/)).toBeInTheDocument();
    expect(screen.getByText('LKR 187,220.00')).toBeInTheDocument();
    expect(screen.getByText('USD 624.07')).toBeInTheDocument();
    expect(screen.getByText(/1 USD = 300 LKR, as of/)).toBeInTheDocument();
    // Names from the API instead of raw ids.
    expect(screen.getByText('Nimal Perera')).toBeInTheDocument();
    expect(screen.getByText('Van CAB-1234')).toBeInTheDocument();
    expect(screen.getByText(/2 × Kandy Hills — Standard Double/)).toBeInTheDocument();
  });

  it.each([
    ['PendingReview', 'Pending review', /edit it directly/],
    ['RevisionRequested', 'Re-planning', /re-planning with your comment/],
    ['QuotationSent', 'Waiting for the client', /can accept or decline it\. Nothing is booked yet\./],
    ['ClientAccepted', 'Client accepted', /Confirm books the guide, vehicle and rooms/],
    ['Confirmed', 'Confirmed', /The trip is booked/],
  ])('explains the %s status in the banner', async (tripStatus, title, body) => {
    givenReview({ tripStatus });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const region = await banner();
    expect(within(region).getByRole('heading', { name: title })).toBeInTheDocument();
    expect(region).toHaveTextContent(body);
    // The review buttons only exist while the trip is in review.
    const sendButtons = screen.queryAllByRole('button', { name: 'Send to client' });
    expect(sendButtons).toHaveLength(tripStatus === 'PendingReview' ? 1 : 0);
    expect(screen.queryAllByRole('button', { name: 'Confirm trip' })).toHaveLength(
      tripStatus === 'ClientAccepted' ? 1 : 0,
    );
  });

  it('explains what each review button does next', async () => {
    givenReview();
    renderApp(`/approvals/${WORKFLOW_ID}`);

    expect(
      await screen.findByText(/the tourist gets the quotation in the app and can accept or decline/),
    ).toBeInTheDocument();
    expect(screen.getByText(/the Planner agent re-plans with your comment/)).toBeInTheDocument();
    expect(screen.getByText(/makes a new version with today's rates/)).toBeInTheDocument();
  });

  it('disables Send to client for an over-budget proposal and says why', async () => {
    givenReview({
      workflow: pendingWorkflow({
        status: 'RevisionRequested',
        validationResult: {
          isValid: false,
          hasHard: false,
          hasSoft: true,
          violations: [
            {
              code: 'OVER_BUDGET',
              message: 'Total USD 624.07 is over the budget of USD 400.',
              severity: 'Soft',
            },
          ],
        },
      }),
    });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(checklist).toHaveTextContent("Total is within the tourist's budget — failed");
    expect(checklist).toHaveTextContent('Soft: Total USD 624.07 is over the budget of USD 400.');
    const send = await screen.findByRole('button', { name: 'Send to client' });
    expect(send).toBeDisabled();
    expect(send).toHaveAccessibleDescription(/over the budget/);
  });

  it('disables Send to client after an edit until the proposal is re-priced', async () => {
    const workflow = pendingWorkflow();
    givenReview({
      workflow: { ...workflow, finalOutcome: { ...workflow.finalOutcome, editedSinceQuotation: true } },
    });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const send = await screen.findByRole('button', { name: 'Send to client' });
    expect(send).toBeDisabled();
    expect(send).toHaveAccessibleDescription(/edited after it was priced\. Re-price it first/);
  });

  it('shows the client decline reason and blocks sending the declined version again', async () => {
    givenReview({
      stored: quotation({
        status: 'Declined',
        decisions: [
          { decision: 'Approved', comment: null, decidedAt: '2026-09-27T08:00:00Z' },
          { decision: 'Declined', comment: 'Too expensive for us', decidedAt: '2026-09-28T08:00:00Z' },
        ],
      }),
    });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    expect(await banner()).toHaveTextContent('The client declined version 1: Too expensive for us');
    const send = await screen.findByRole('button', { name: 'Send to client' });
    await waitFor(() => expect(send).toBeDisabled());
  });

  it('Send to client calls the approve endpoint and shows the new trip status', async () => {
    givenReview();
    let approvedId: string | undefined;
    server.use(
      http.post(`${API}/api/quotations/:id/approve`, ({ params }) => {
        approvedId = params.id as string;
        return HttpResponse.json(decision('Approved', 'QuotationSent'));
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    const send = await screen.findByRole('button', { name: 'Send to client' });
    await waitFor(() => expect(send).toBeEnabled());
    await user.click(send);
    const dialog = screen.getByRole('dialog', { name: 'Send to client' });
    expect(dialog).toHaveTextContent('Nothing is booked yet.');
    await user.click(within(dialog).getByRole('button', { name: 'Send to client' }));

    expect(await screen.findByText('Sent to the client. Trip is now quotation sent.')).toBeInTheDocument();
    expect(approvedId).toBe(QUOTATION_ID);
  });

  it('requires a comment before requesting a revision', async () => {
    givenReview();
    let body: unknown;
    server.use(
      http.post(`${API}/api/quotations/:id/request-revision`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(decision('RevisionRequested', 'RevisionRequested'));
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Request revision' }));
    const dialog = screen.getByRole('dialog', { name: 'Request a revision' });
    await user.click(within(dialog).getByRole('button', { name: 'Send to the planner' }));
    expect(within(dialog).getByRole('alert')).toHaveTextContent('A comment is required for a revision.');
    expect(body).toBeUndefined();

    await user.type(within(dialog).getByLabelText(/Comment/), 'Use a cheaper hotel tier');
    await user.click(within(dialog).getByRole('button', { name: 'Send to the planner' }));

    expect(
      await screen.findByText(/Revision requested\. Trip is now revision requested/),
    ).toBeInTheDocument();
    expect(body).toEqual({ comment: 'Use a cheaper hotel tier' });
  });

  it('confirms an accepted trip in one step', async () => {
    givenReview({ tripStatus: 'ClientAccepted', stored: quotation({ status: 'Approved' }) });
    let confirmed = false;
    server.use(
      http.post(`${API}/api/trip-requests/${TRIP_ID}/confirm`, () => {
        confirmed = true;
        return HttpResponse.json({ ...decision('Confirmed', 'Confirmed'), holdsCreated: 6 });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Confirm trip' }));
    const dialog = screen.getByRole('dialog', { name: 'Confirm trip' });
    await user.click(within(dialog).getByRole('button', { name: 'Confirm trip' }));

    expect(
      await screen.findByText('Trip confirmed: 6 holds created, vouchers issued and the tourist emailed.'),
    ).toBeInTheDocument();
    expect(confirmed).toBe(true);
  });

  it('shows the 409 message when Confirm fails', async () => {
    givenReview({ tripStatus: 'ClientAccepted', stored: quotation({ status: 'Approved' }) });
    server.use(
      http.post(`${API}/api/trip-requests/${TRIP_ID}/confirm`, () =>
        HttpResponse.json(
          { title: 'Conflict', status: 409, detail: 'Van CAB-1234 is no longer free on 2026-10-11.' },
          { status: 409 },
        ),
      ),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Confirm trip' }));
    await user.click(
      within(screen.getByRole('dialog', { name: 'Confirm trip' })).getByRole('button', {
        name: 'Confirm trip',
      }),
    );

    expect(await within(await banner()).findByRole('alert')).toHaveTextContent(
      'Van CAB-1234 is no longer free on 2026-10-11.',
    );
  });

  it('reopens the review with a reason', async () => {
    givenReview({ tripStatus: 'ClientAccepted', stored: quotation({ status: 'Approved' }) });
    let body: unknown;
    server.use(
      http.post(`${API}/api/trip-requests/${TRIP_ID}/reopen-review`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(decision('ReviewReopened', 'PendingReview'));
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Reopen review' }));
    const dialog = screen.getByRole('dialog', { name: 'Reopen review' });
    await user.type(within(dialog).getByLabelText(/Reason/), 'Client wants a sea-view room');
    await user.click(within(dialog).getByRole('button', { name: 'Reopen review' }));

    expect(await screen.findByText(/Review reopened/)).toBeInTheDocument();
    expect(body).toEqual({ reason: 'Client wants a sea-view room' });
  });

  it('edits a proposal day directly with 1–3 attractions of that city', async () => {
    givenReview();
    let body: unknown;
    server.use(
      http.get(`${API}/api/attractions`, () =>
        HttpResponse.json(paged([attraction('a1', 'Temple of the Tooth'), attraction('a3', 'Kandy Lake')])),
      ),
      http.get(`${API}/api/availability`, () => HttpResponse.json([])),
      http.put(`${API}/api/trip-requests/${TRIP_ID}/proposal/days/1`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({
          workflowId: WORKFLOW_ID,
          tripRequestId: TRIP_ID,
          editedSinceQuotation: true,
          days: [],
          resources: null,
        });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Edit directly' }));
    await user.click(screen.getByRole('button', { name: 'Edit day 1' }));
    const dialog = await screen.findByRole('dialog', { name: 'Edit day 1 — Kandy' });
    await user.click(await within(dialog).findByRole('checkbox', { name: /Kandy Lake/ }));
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));

    expect(await screen.findByText(/Saved day 1\. Re-price before sending/)).toBeInTheDocument();
    expect(body).toEqual({ attractionIds: ['a1', 'a3'] });
  });

  it('swaps the guide for a free one from the availability search', async () => {
    givenReview();
    let availability: URLSearchParams | undefined;
    let body: unknown;
    server.use(
      http.get(`${API}/api/availability`, ({ request }) => {
        const params = new URL(request.url).searchParams;
        if (params.get('type') === 'Guide') availability = params;
        return HttpResponse.json(
          params.get('type') === 'Guide'
            ? [
                {
                  type: 'Guide',
                  id: 'guide-2',
                  name: 'Kamal Silva',
                  detail: 'Speaks en',
                  rateLkr: 5000,
                  freeRooms: null,
                },
              ]
            : [],
        );
      }),
      http.put(`${API}/api/trip-requests/${TRIP_ID}/proposal/resources`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({
          workflowId: WORKFLOW_ID,
          tripRequestId: TRIP_ID,
          editedSinceQuotation: true,
          days: [],
          resources: null,
        });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Edit directly' }));
    const select = await screen.findByLabelText('Swap guide for');
    await within(select).findByRole('option', { name: /Kamal Silva/ });
    await user.selectOptions(select, 'guide-2');
    const swapButtons = screen.getAllByRole('button', { name: 'Swap' });
    await user.click(swapButtons[0]!);

    expect(await screen.findByText(/Guide swapped\. Re-price before sending/)).toBeInTheDocument();
    expect(body).toEqual({ guideId: 'guide-2' });
    expect(availability?.get('language')).toBe('en');
    expect(availability?.get('pax')).toBe('4');
    expect(availability?.get('from')).toBe('2026-10-10');
  });

  it('shows the previous and the newest version side by side with the decisions', async () => {
    const v1 = quotation({
      id: 'q-v1',
      version: 1,
      status: 'Superseded',
      totalUsd: 700,
      totalLkr: 210000,
      decisions: [
        { decision: 'RevisionRequested', comment: 'Use a cheaper hotel', decidedAt: '2026-09-27T08:00:00Z' },
      ],
      proposalSnapshot: {
        days: [
          {
            ...pendingWorkflow().finalOutcome.proposal.days[0],
            stops: [{ attraction_id: 'a1', name: 'Temple of the Tooth', entry_fee_lkr: 2000 }],
          },
        ],
        resources: { guide_id: 'guide-1', vehicle_id: 'vehicle-1', rooms: [], gaps: [] },
      },
    });
    const v2 = quotation({
      version: 2,
      decisions: [],
      proposalSnapshot: {
        days: [
          {
            ...pendingWorkflow().finalOutcome.proposal.days[0],
            stops: [{ attraction_id: 'a3', name: 'Kandy Lake', entry_fee_lkr: 0 }],
          },
        ],
        resources: { guide_id: 'guide-1', vehicle_id: 'vehicle-1', rooms: [], gaps: [] },
      },
    });
    givenReview({ stored: v2 });
    server.use(
      http.get(`${API}/api/quotations`, ({ request }) => {
        const params = new URL(request.url).searchParams;
        expect(params.get('tripRequestId')).toBe(TRIP_ID);
        expect(params.get('sort')).toBe('version');
        return HttpResponse.json(paged([v1, v2]));
      }),
      http.get(`${API}/api/quotations/q-v1`, () => HttpResponse.json(v1)),
    );
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const versions = await screen.findByRole('region', { name: 'Versions side by side' });
    const first = await within(versions).findByRole('article', { name: 'Version 1' });
    const second = await within(versions).findByRole('article', { name: 'Version 2' });
    expect(first).toHaveTextContent('Replaced by a newer version');
    expect(first).toHaveTextContent('Temple of the Tooth');
    expect(first).toHaveTextContent('Manager asked for a revision');
    expect(first).toHaveTextContent('Use a cheaper hotel');
    expect(first).toHaveTextContent('Guide: Nimal Perera');
    expect(second).toHaveTextContent('Kandy Lake');
    expect(second).toHaveTextContent('USD 624.07');
  });
});
