import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { pendingWorkflow, QUOTATION_ID, quotation, trip, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const TRIP_ID = trip().id;

const EXPLANATION = {
  items: [
    {
      topic: 'guide',
      title: 'Guide',
      text: 'Nimal Perera was chosen: the cheapest free English-speaking guide for 4 people (LKR 6,000 a day).',
    },
    { topic: 'vehicle', title: 'Vehicle', text: 'Van CAB-1234 has 6 seats for 4 travellers.' },
    { topic: 'driving', title: 'Driving', text: 'Day 3: 140 km, about 3 h 10 min of driving.' },
    {
      topic: 'budget',
      title: 'Budget',
      text: 'Total USD 624 against a budget of USD 1,500 — 58% under budget.',
    },
  ],
};

function givenReview() {
  server.use(
    http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () => HttpResponse.json(pendingWorkflow())),
    http.get(`${API}/api/trip-requests/${TRIP_ID}`, () =>
      HttpResponse.json(trip({ status: 'QuotationSent' })),
    ),
    http.get(`${API}/api/quotations/${QUOTATION_ID}`, () => HttpResponse.json(quotation())),
  );
}

describe('Review page: Why this plan', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('explains the plan in plain language, one reason per topic', async () => {
    givenReview();
    server.use(
      http.get(`${API}/api/trip-requests/${TRIP_ID}/plan-explanation`, () => HttpResponse.json(EXPLANATION)),
    );
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const panel = await screen.findByRole('region', { name: 'Why this plan' });
    const reasons = within(panel).getAllByRole('listitem');
    expect(reasons).toHaveLength(4);
    expect(within(reasons[0]!).getByRole('heading', { name: 'Guide' })).toBeInTheDocument();
    expect(reasons[0]).toHaveTextContent('the cheapest free English-speaking guide');
    expect(reasons[3]).toHaveTextContent('58% under budget');
  });

  it('hides the panel when there is no proposal to explain (404)', async () => {
    givenReview();
    renderApp(`/approvals/${WORKFLOW_ID}`);

    expect(await screen.findByRole('list', { name: 'Validation checklist' })).toBeInTheDocument();
    expect(screen.queryByRole('region', { name: 'Why this plan' })).not.toBeInTheDocument();
  });

  it('keeps the panel next to the checks and the versions, with no decision buttons', async () => {
    givenReview();
    server.use(
      http.get(`${API}/api/trip-requests/${TRIP_ID}/plan-explanation`, () => HttpResponse.json(EXPLANATION)),
    );
    renderApp(`/approvals/${WORKFLOW_ID}`);

    await screen.findByRole('region', { name: 'Why this plan' });
    expect(screen.getByRole('list', { name: 'Validation checklist' })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Send to client' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Request revision' })).not.toBeInTheDocument();
  });
});
