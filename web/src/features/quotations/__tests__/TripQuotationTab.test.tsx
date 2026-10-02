import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, quotation, trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ID = trip().id;
const V1 = quotation({ id: 'q1', version: 1, status: 'Superseded' });
const V2 = quotation({
  id: 'q2',
  version: 2,
  status: 'Approved',
  totalUsd: 603.94,
  acceptedAt: '2026-09-28T10:00:00Z',
  decisions: [
    { decision: 'Approved', comment: null, decidedAt: '2026-09-27T10:00:00Z' },
    { decision: 'Accepted', comment: null, decidedAt: '2026-09-28T10:00:00Z' },
  ],
});

/** A trip in `status` with versions v1 (replaced) and v2 (sent and accepted). */
function givenQuotations(status: string, newest = V2) {
  server.use(
    http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status }))),
    http.get(`${API}/api/quotations`, () => HttpResponse.json(paged([V1, newest]))),
    http.get(`${API}/api/quotations/q1`, () => HttpResponse.json(V1)),
    http.get(`${API}/api/quotations/q2`, () => HttpResponse.json(newest)),
  );
}

describe('Trip detail: Quotation tab', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('lists the versions and shows the newest with its lines, deposit and decisions', async () => {
    givenQuotations('ClientAccepted');
    renderApp(`/trips/${ID}?tab=quotation`);

    expect(await screen.findByRole('tab', { name: 'Quotation' })).toHaveAttribute('aria-selected', 'true');
    const versions = await screen.findByRole('table', { name: 'Quotation versions' });
    expect(within(versions).getAllByRole('row')).toHaveLength(3);
    expect(within(versions).getByText('Replaced by a newer version')).toBeInTheDocument();

    const newest = await screen.findByRole('article', { name: 'Quotation version 2' });
    expect(within(newest).getByText(/Guide Nimal Perera, 5 days/)).toBeInTheDocument();
    const deposit = within(newest).getByRole('region', { name: 'Deposit' });
    expect(deposit).toHaveTextContent('Deposit (30%)');
    expect(deposit).toHaveTextContent('LKR 56,166.00 (USD 187.22)');
    expect(deposit).toHaveTextContent('Unpaid');
    expect(within(newest).getByText(/Client accepted/)).toBeInTheDocument();
  });

  it('marks the deposit paid after the client accepted, then offers Mark unpaid', async () => {
    let newest: Record<string, unknown> = V2;
    let body: unknown = null;
    givenQuotations('Confirmed');
    server.use(
      http.get(`${API}/api/quotations/q2`, () => HttpResponse.json(newest)),
      http.post(`${API}/api/quotations/q2/payment`, async ({ request }) => {
        body = await request.json();
        newest = { ...V2, depositPaid: true, depositPaidAt: '2026-10-01T09:00:00Z' };
        return HttpResponse.json(newest);
      }),
    );
    const { user } = renderApp(`/trips/${ID}?tab=quotation`);

    await user.click(await screen.findByRole('button', { name: 'Mark deposit paid' }));

    expect(await screen.findByText('Deposit marked as paid.')).toBeInTheDocument();
    expect(body).toEqual({ paid: true });
    expect(await screen.findByRole('button', { name: 'Mark unpaid' })).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Deposit' })).toHaveTextContent('Paid on');
  });

  it('offers no payment action before the client accepted, nor on an older version', async () => {
    givenQuotations('QuotationSent');
    const { user } = renderApp(`/trips/${ID}?tab=quotation`);

    await screen.findByRole('article', { name: 'Quotation version 2' });
    expect(screen.queryByRole('button', { name: 'Mark deposit paid' })).not.toBeInTheDocument();
    expect(screen.getByText(/can be marked paid once the client has accepted/)).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Show v1' }));
    await screen.findByRole('article', { name: 'Quotation version 1' });
    expect(screen.getByText('Only the newest version can be marked paid.')).toBeInTheDocument();
  });

  it('shows the best-price note, in warning tone, on a version sent over the budget', async () => {
    const best = {
      ...V2,
      bestAvailablePrice: true,
      overBudgetUsd: 132,
      budgetNote: 'Best price we can offer — USD 132.00 above your budget',
    };
    givenQuotations('QuotationSent', best);
    const { user } = renderApp(`/trips/${ID}?tab=quotation`);

    const newest = await screen.findByRole('article', { name: 'Quotation version 2' });
    const note = within(newest).getByText('Best price we can offer — USD 132.00 above your budget');
    expect(note).toHaveClass('bg-amber-50', 'text-amber-900');
    const versions = screen.getByRole('table', { name: 'Quotation versions' });
    expect(within(versions).getByRole('row', { name: /v2/ })).toHaveTextContent('Best price');
    expect(within(versions).getByRole('row', { name: /v2/ })).toHaveTextContent('Sent to client');

    // Version 1 was within budget: no note.
    await user.click(screen.getByRole('button', { name: 'Show v1' }));
    const older = await screen.findByRole('article', { name: 'Quotation version 1' });
    expect(within(older).queryByText(/Best price we can offer/)).not.toBeInTheDocument();
  });

  it('says when the trip has no quotation yet, and switches back to the overview', async () => {
    server.use(
      http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status: 'Planning' }))),
      http.get(`${API}/api/trip-requests/${ID}/history`, () => HttpResponse.json([])),
      http.get(`${API}/api/trip-requests/${ID}/itinerary`, () =>
        HttpResponse.json({ title: 'Not found' }, { status: 404 }),
      ),
    );
    const { user, location } = renderApp(`/trips/${ID}?tab=quotation`);

    expect(await screen.findByText('No quotation yet')).toBeInTheDocument();
    await user.click(screen.getByRole('tab', { name: 'Overview' }));

    await waitFor(() => expect(location()).toBe(`/trips/${ID}`));
    expect(await screen.findByText('No itinerary yet')).toBeInTheDocument();
  });
});
