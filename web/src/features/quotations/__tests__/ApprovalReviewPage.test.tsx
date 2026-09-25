import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { pendingWorkflow, QUOTATION_ID, trip, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

function givenWorkflow(workflow: object) {
  server.use(
    http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () => HttpResponse.json(workflow)),
    http.get(`${API}/api/trip-requests/${trip().id}`, () =>
      HttpResponse.json(trip({ status: 'PendingApproval' })),
    ),
  );
}

describe('ApprovalReviewPage', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('renders the validation checklist, itinerary and quotation in LKR and USD', async () => {
    givenWorkflow(pendingWorkflow());
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(within(checklist).getAllByRole('listitem')).toHaveLength(13);
    expect(checklist).toHaveTextContent('Every day has 1–3 stops — passed');
    expect(screen.getByText('All deterministic checks passed.')).toBeInTheDocument();
    expect(screen.getByText(/Day 2 — Ella/)).toBeInTheDocument();
    expect(screen.getByText('LKR 187,220.00')).toBeInTheDocument();
    expect(screen.getByText('USD 624.07')).toBeInTheDocument();
    expect(screen.getByText(/1 USD = 300 LKR, as of/)).toBeInTheDocument();
  });

  it('marks failed rules red and disables Approve for an over-budget proposal', async () => {
    givenWorkflow(
      pendingWorkflow({
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
    );
    renderApp(`/approvals/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(checklist).toHaveTextContent("Total is within the tourist's budget — failed");
    expect(checklist).toHaveTextContent('Soft: Total USD 624.07 is over the budget of USD 400.');
    expect(screen.getByRole('button', { name: 'Approve' })).toBeDisabled();
  });

  it('clicking Approve calls the approve endpoint and shows a success toast', async () => {
    givenWorkflow(pendingWorkflow());
    let approvedId: string | undefined;
    server.use(
      http.post(`${API}/api/quotations/:id/approve`, ({ params }) => {
        approvedId = params.id as string;
        return HttpResponse.json({
          quotationId: QUOTATION_ID,
          tripRequestId: trip().id,
          workflowId: WORKFLOW_ID,
          decision: 'Approved',
          tripStatus: 'Confirmed',
          workflowStatus: 'Completed',
          holdsCreated: 6,
        });
      }),
    );
    const { user } = renderApp(`/approvals/${WORKFLOW_ID}`);

    await user.click(await screen.findByRole('button', { name: 'Approve' }));
    const dialog = screen.getByRole('dialog', { name: 'Approve quotation' });
    await user.click(within(dialog).getByRole('button', { name: 'Approve' }));

    expect(await screen.findByText('Approved. Trip is now confirmed; 6 holds created.')).toBeInTheDocument();
    expect(approvedId).toBe(QUOTATION_ID);
  });

  it('requires a comment before requesting a revision', async () => {
    givenWorkflow(pendingWorkflow());
    let body: unknown;
    server.use(
      http.post(`${API}/api/quotations/:id/request-revision`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({
          quotationId: QUOTATION_ID,
          tripRequestId: trip().id,
          workflowId: WORKFLOW_ID,
          decision: 'RevisionRequested',
          tripStatus: 'RevisionRequested',
          workflowStatus: 'RevisionRequested',
          holdsCreated: 0,
        });
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
});
