import { screen, waitFor, within } from '@testing-library/react';
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
      HttpResponse.json(trip({ status: 'PendingReview' })),
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

    expect(await screen.findByRole('button', { name: 'Send to client' })).toBeEnabled();
    expect(screen.queryByRole('region', { name: 'Why this plan' })).not.toBeInTheDocument();
  });

  it('still sends to the client and asks for a revision with the panel shown', async () => {
    givenReview();
    const calls: string[] = [];
    server.use(
      http.get(`${API}/api/trip-requests/${TRIP_ID}/plan-explanation`, () => HttpResponse.json(EXPLANATION)),
      http.post(`${API}/api/quotations/${QUOTATION_ID}/:decision`, ({ params }) => {
        calls.push(String(params.decision));
        return HttpResponse.json({
          quotationId: QUOTATION_ID,
          tripRequestId: TRIP_ID,
          workflowId: WORKFLOW_ID,
          decision: params.decision === 'approve' ? 'Approved' : 'RevisionRequested',
          tripStatus: params.decision === 'approve' ? 'QuotationSent' : 'RevisionRequested',
          workflowStatus: 'Approved',
          holdsCreated: 0,
        });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);
    await screen.findByRole('region', { name: 'Why this plan' });

    await user.click(screen.getByRole('button', { name: 'Send to client' }));
    const send = screen.getByRole('dialog', { name: 'Send to client' });
    await user.click(within(send).getByRole('button', { name: 'Send to client' }));
    expect(await screen.findByText(/Sent to the client\. Trip is now quotation sent/)).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Request revision' }));
    const revision = screen.getByRole('dialog', { name: 'Request a revision' });
    await user.type(within(revision).getByLabelText(/Comment/), 'Use a cheaper hotel in Ella');
    await user.click(within(revision).getByRole('button', { name: 'Send to the planner' }));

    await waitFor(() => expect(calls).toEqual(['approve', 'request-revision']));
  });
});
