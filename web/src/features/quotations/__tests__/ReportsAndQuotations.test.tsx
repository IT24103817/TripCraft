import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, pendingWorkflow, trip, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const QUOTATION = {
  id: 'q1',
  tripRequestId: '11111111-1111-1111-1111-111111111111',
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
  fxStale: true,
  acceptedAt: null,
  lines: [
    { lineType: 'guide', description: 'Guide Nimal Perera, 5 days', qty: 5, unitLkr: 6000, amountLkr: 30000 },
  ],
  decisions: [],
  createdAt: '2026-09-26T04:00:00Z',
};

describe('Reports, quotations and re-pricing (Component C)', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows revenue and utilisation from the reports API', async () => {
    server.use(
      http.get(`${API}/api/reports/revenue`, () =>
        HttpResponse.json([{ month: '2026-08', quotations: 1, totalLkr: 115000, totalUsd: 383.33 }]),
      ),
      http.get(`${API}/api/reports/utilisation`, () =>
        HttpResponse.json([
          {
            resourceType: 'Guide',
            resourceId: 'g1',
            name: 'Nimal Perera',
            heldDays: 3,
            daysInRange: 365,
            utilisationPct: 0.8,
          },
        ]),
      ),
      http.get(`${API}/api/reports/trips-by-status`, () =>
        HttpResponse.json([{ status: 'Completed', count: 1 }]),
      ),
    );
    renderApp('/reports');

    const revenue = await screen.findByRole('table', { name: 'Approved quotations per month, in USD' });
    expect(within(revenue).getByRole('rowheader', { name: '2026-08' })).toBeInTheDocument();
    expect(screen.getByText(/from 1 approved quotations/)).toBeInTheDocument();
    const utilisation = screen.getByRole('table', {
      name: 'Held days as a percentage of the days in the period',
    });
    expect(within(utilisation).getByText('0.8%')).toBeInTheDocument();
    expect(
      screen.getByRole('table', { name: 'Trip requests starting in the period, per status' }),
    ).toHaveTextContent('Completed');
  });

  it('lists quotations and sends the status filter', async () => {
    const requests: URLSearchParams[] = [];
    server.use(
      http.get(`${API}/api/quotations`, ({ request }) => {
        requests.push(new URL(request.url).searchParams);
        return HttpResponse.json(paged([QUOTATION]));
      }),
    );
    const { user } = renderApp('/quotations');

    const table = await screen.findByRole('table', { name: 'Quotations' });
    expect(within(table).getByText('300 (stale)')).toBeInTheDocument();
    await user.selectOptions(screen.getByLabelText('Status'), 'Approved');
    await waitFor(() => expect(requests.at(-1)?.get('status')).toBe('Approved'));
    expect(requests[0]?.get('sort')).toBe('-createdAt');
  });

  it('re-prices the stored quotation from the approval review', async () => {
    const workflow = pendingWorkflow();
    workflow.finalOutcome!.proposal.quotationId = 'q1';
    let recalculated = false;
    server.use(
      http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () => HttpResponse.json(workflow)),
      http.get(`${API}/api/quotations/q1`, () => HttpResponse.json(QUOTATION)),
      http.get(`${API}/api/trip-requests/:id`, () => HttpResponse.json(trip())),
      http.post(`${API}/api/quotations/q1/calculate`, () => {
        recalculated = true;
        return HttpResponse.json({
          quotation: { ...QUOTATION, fxRate: 310, fxStale: false, totalUsd: 603.94 },
          previousTotalLkr: 187220,
          previousTotalUsd: 624.07,
          changed: true,
        });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    expect(await screen.findByRole('heading', { name: 'Quotation v1 (Pending)' })).toBeInTheDocument();
    expect(screen.getByText(/Guide Nimal Perera, 5 days/)).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: "Re-price with today's rates" }));

    expect(await screen.findByText(/Re-priced: .*624\.07.* → .*603\.94/)).toBeInTheDocument();
    expect(recalculated).toBe(true);
  });

  it("shows this month's revenue on the manager dashboard", async () => {
    server.use(
      http.get(`${API}/api/reports/revenue`, () =>
        HttpResponse.json([{ month: '2026-09', quotations: 2, totalLkr: 300000, totalUsd: 1000 }]),
      ),
    );
    renderApp('/');

    const card = (await screen.findByText('Revenue this month')).closest('div')!;
    await waitFor(() => expect(card).toHaveTextContent('1,000'));
  });
});
