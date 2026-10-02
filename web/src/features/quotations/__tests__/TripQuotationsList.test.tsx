import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, quotation, trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const SENT = quotation({
  id: 'q-sent',
  tripRequestId: 'trip-a',
  version: 2,
  status: 'Approved',
  totalUsd: 1603.94,
  totalLkr: 481182,
  depositPaid: true,
  tripObjective: 'Honeymoon in Ella',
  tripStatus: 'ClientAccepted',
  bestAvailablePrice: true,
  overBudgetUsd: 103.94,
  budgetNote: 'Best price we can offer — USD 103.94 above your budget',
});
const WAITING = quotation({
  id: 'q-waiting',
  tripRequestId: 'trip-b',
  version: 1,
  status: 'Pending',
  tripObjective: 'Cultural triangle for 4',
  tripStatus: 'NeedsOperator',
});

describe('Trips page: Quotations tab', () => {
  let requests: URL[];

  beforeEach(() => {
    signInAs('OperationsManager');
    requests = [];
    server.use(
      http.get(`${API}/api/quotations`, ({ request }) => {
        const url = new URL(request.url);
        requests.push(url);
        return HttpResponse.json(paged([SENT, WAITING]));
      }),
    );
  });

  it('lists the newest version of each trip with readable statuses, totals and the deposit', async () => {
    renderApp('/trips?tab=quotations');

    expect(await screen.findByRole('tab', { name: 'Quotations' })).toHaveAttribute('aria-selected', 'true');
    const table = await screen.findByRole('table', { name: /Quotations/ });
    const [, sentRow, waitingRow] = within(table).getAllByRole('row');

    const sent = within(sentRow!);
    expect(sent.getByText('Honeymoon in Ella')).toBeInTheDocument();
    expect(sent.getByText('v2')).toBeInTheDocument();
    expect(sent.getByText('Sent to client')).toBeInTheDocument();
    expect(sent.getByText('Accepted')).toBeInTheDocument();
    expect(sent.getByText('USD 1,603.94')).toBeInTheDocument();
    // Sent over budget after the agents' lowest-cost re-plans: the best-price sentence is shown in warning tone.
    expect(sent.getByText('Best price we can offer — USD 103.94 above your budget')).toHaveClass(
      'text-amber-800',
    );
    expect(sent.getByText('LKR 481,182.00')).toBeInTheDocument();
    expect(sent.getByText('Paid')).toBeInTheDocument();

    const waiting = within(waitingRow!);
    expect(waiting.getByText('Not sent yet')).toBeInTheDocument();
    expect(waiting.getByText('Needs operator')).toBeInTheDocument();
    expect(waiting.getByText('Unpaid')).toBeInTheDocument();
    expect(waiting.queryByText(/Best price/)).not.toBeInTheDocument();

    expect(requests.at(-1)?.searchParams.get('latestOnly')).toBe('true');
    expect(requests.at(-1)?.searchParams.get('sort')).toBe('-createdAt');
  });

  it('sends the minimum total (USD) and keeps latestOnly=true', async () => {
    renderApp('/trips?tab=quotations');
    await screen.findByText('Honeymoon in Ella');

    fireEvent.change(screen.getByLabelText('Minimum total (USD)'), { target: { value: '1500' } });

    await waitFor(() => expect(requests.at(-1)?.searchParams.get('minTotalUsd')).toBe('1500'));
    expect(requests.at(-1)?.searchParams.get('latestOnly')).toBe('true');
  });

  it('filters by quotation status with the readable labels', async () => {
    const { user } = renderApp('/trips?tab=quotations');
    await screen.findByText('Honeymoon in Ella');

    await user.selectOptions(screen.getByLabelText('Quotation status'), 'Sent to client');

    await waitFor(() => expect(requests.at(-1)?.searchParams.get('status')).toBe('Approved'));
  });

  it("opens the trip on its Quotation tab from the row's link", async () => {
    server.use(
      http.get(`${API}/api/trip-requests/trip-a`, () =>
        HttpResponse.json(trip({ id: 'trip-a', status: 'ClientAccepted' })),
      ),
    );
    const { user, location } = renderApp('/trips?tab=quotations');

    const link = await screen.findByRole('link', { name: 'Honeymoon in Ella' });
    expect(link).toHaveAttribute('href', '/trips/trip-a?tab=quotation');

    await user.click(link);
    await waitFor(() => expect(location()).toBe('/trips/trip-a?tab=quotation'));
  });

  it('switches between the Trips and Quotations tabs through the URL', async () => {
    const { user, location } = renderApp('/trips');
    expect(await screen.findByRole('tab', { name: 'Trips' })).toHaveAttribute('aria-selected', 'true');

    await user.click(screen.getByRole('tab', { name: 'Quotations' }));

    await waitFor(() => expect(location()).toBe('/trips?tab=quotations'));
    expect(await screen.findByText('Honeymoon in Ella')).toBeInTheDocument();
  });

  it('shows an empty state when no trip has a quotation yet', async () => {
    server.use(http.get(`${API}/api/quotations`, () => HttpResponse.json(paged([]))));
    renderApp('/trips?tab=quotations');

    expect(await screen.findByText('No quotations yet')).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
});
