import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, pendingWorkflow, QUOTATION_ID, trip, WORKFLOW_ID } from '@/test/fixtures';
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

function givenReview(options: { workflow?: object; tripStatus?: string; stored?: object | null } = {}) {
  const { workflow = pendingWorkflow(), tripStatus = 'QuotationSent', stored = quotation() } = options;
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
    ['QuotationSent', 'Waiting for the client', /can accept or decline it\. Nothing is booked yet\./],
    ['ClientAccepted', 'Accepted — confirm to book', /Confirm holds the guide, vehicle and rooms/],
    ['ClientDeclined', 'Declined — needs a decision', /Replan with a note for the Planner agent/],
    ['NeedsOperator', 'Needs operator', /nothing was sent/],
    ['Confirmed', 'Confirmed', /The trip is booked/],
  ])('explains the %s status in the banner', async (tripStatus, title, body) => {
    givenReview({ tripStatus });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const region = await banner();
    expect(within(region).getByRole('heading', { name: title })).toBeInTheDocument();
    expect(region).toHaveTextContent(body);
    // The actions live on the trip page; the banner links there when the manager has something to do.
    const link = within(region).queryByRole('link', { name: 'Open the trip to act' });
    if (['ClientAccepted', 'ClientDeclined', 'NeedsOperator'].includes(tripStatus)) {
      expect(link).toHaveAttribute('href', `/trips/${TRIP_ID}`);
    } else {
      expect(link).not.toBeInTheDocument();
    }
  });

  it.each(['QuotationSent', 'ClientAccepted', 'ClientDeclined', 'NeedsOperator'])(
    'has no Approve, Request revision, Reject or Send buttons (%s): quotations are sent automatically',
    async (tripStatus) => {
      givenReview({ tripStatus });
      renderApp(`/approvals/${WORKFLOW_ID}`);

      await screen.findByRole('list', { name: 'Validation checklist' });
      for (const name of [
        /approve/i,
        /request revision/i,
        /^reject/i,
        /send to client/i,
        /reopen review/i,
        /^confirm/i,
        /edit directly/i,
      ]) {
        expect(screen.queryByRole('button', { name })).not.toBeInTheDocument();
      }
    },
  );

  it('shows the client decline reason in the banner', async () => {
    givenReview({
      tripStatus: 'ClientDeclined',
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
  });

  it('shows the failed budget rule and the best-price note of a version sent over budget', async () => {
    givenReview({
      workflow: pendingWorkflow({
        status: 'Approved',
        validationResult: {
          isValid: false,
          hasHard: false,
          hasSoft: true,
          violations: [
            {
              code: 'OVER_BUDGET',
              message: 'Total USD 532.00 is over the budget of USD 400.',
              severity: 'Soft',
            },
          ],
        },
      }),
      stored: quotation({
        status: 'Approved',
        bestAvailablePrice: true,
        overBudgetUsd: 132,
        budgetNote: 'Best price we can offer — USD 132.00 above your budget',
      }),
    });
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(checklist).toHaveTextContent("Total is within the tourist's budget — failed");
    expect(
      await screen.findByText('Best price we can offer — USD 132.00 above your budget'),
    ).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Quotation v1 (Sent to client)' })).toBeInTheDocument();
  });

  it('shows the previous and the newest version side by side with the decisions', async () => {
    const v1 = quotation({
      id: 'q-v1',
      version: 1,
      status: 'Superseded',
      totalUsd: 700,
      totalLkr: 210000,
      decisions: [
        { decision: 'Approved', comment: null, decidedAt: '2026-09-27T08:00:00Z' },
        { decision: 'Declined', comment: 'Use a cheaper hotel', decidedAt: '2026-09-27T09:00:00Z' },
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
    expect(first).toHaveTextContent('Client declined');
    expect(first).toHaveTextContent('Use a cheaper hotel');
    expect(first).toHaveTextContent('Guide: Nimal Perera');
    expect(second).toHaveTextContent('Kandy Lake');
    expect(second).toHaveTextContent('USD 624.07');
  });
});
