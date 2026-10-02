import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { attraction, paged, pendingWorkflow, quotation, trip, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ID = trip().id;

const ACCEPTED = quotation({
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
const DECLINED = quotation({
  id: 'q1',
  status: 'Declined',
  decisions: [
    { decision: 'Approved', comment: null, decidedAt: '2026-09-27T10:00:00Z' },
    { decision: 'Declined', comment: 'Too expensive for us', decidedAt: '2026-09-28T10:00:00Z' },
  ],
});

type Json = Record<string, unknown>;

/** What the mocked API answers right now; tests change it to play the next step. */
interface State {
  workflow: Json | null;
  versions: Json[];
}

/** A trip in `status` whose workflow and quotation versions come from `state` at request time. */
function givenTrip(status: string, state: State) {
  server.use(
    http.get(`${API}/api/trip-requests/${ID}`, () => HttpResponse.json(trip({ status }))),
    http.get(`${API}/api/trip-requests/${ID}/history`, () => HttpResponse.json([])),
    http.get(`${API}/api/trip-requests/${ID}/itinerary`, () =>
      HttpResponse.json({ title: 'Not found' }, { status: 404 }),
    ),
    http.get(`${API}/api/trip-requests/${ID}/workflow`, () =>
      state.workflow
        ? HttpResponse.json(state.workflow)
        : HttpResponse.json({ title: 'Not found' }, { status: 404 }),
    ),
    http.get(`${API}/api/quotations`, () => HttpResponse.json(paged(state.versions))),
    http.get(`${API}/api/quotations/:id`, ({ params }) => {
      const found = state.versions.find((v) => v.id === params.id);
      return found ? HttpResponse.json(found) : HttpResponse.json({ title: 'Not found' }, { status: 404 });
    }),
    // The proposal editor's choices.
    http.get(`${API}/api/attractions`, () =>
      HttpResponse.json(paged([attraction('a1', 'Temple of the Tooth'), attraction('a3', 'Kandy Lake')])),
    ),
    http.get(`${API}/api/availability`, () => HttpResponse.json([])),
  );
}

/** The workflow as the trip's GET .../workflow returns it, optionally edited since it was priced. */
function workflow(overrides: Json = {}, editedSinceQuotation = false): Json {
  const base = pendingWorkflow({ status: 'Approved', ...overrides });
  return { ...base, finalOutcome: { ...base.finalOutcome, editedSinceQuotation } };
}

/** QuotationDecisionResponse, as Confirm, Send and Replan answer. */
function decision(name: string, tripStatus: string) {
  return {
    quotationId: 'q2',
    tripRequestId: ID,
    workflowId: WORKFLOW_ID,
    decision: name,
    tripStatus,
    workflowStatus: 'Approved',
    holdsCreated: 0,
  };
}

const nextStep = () => screen.findByRole('region', { name: /^Next step/ });

describe('Trip page actions: Accepted', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('confirms an accepted trip in one step', async () => {
    givenTrip('ClientAccepted', { workflow: workflow(), versions: [ACCEPTED] });
    let confirmed = false;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/confirm`, () => {
        confirmed = true;
        return HttpResponse.json({ ...decision('Confirmed', 'Confirmed'), holdsCreated: 6 });
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    expect(panel).toHaveAccessibleName('Next step: Accepted — confirm to book');
    expect(await within(panel).findByText('The client accepted version 2 (USD 603.94).')).toBeInTheDocument();
    const confirm = within(panel).getByRole('button', { name: 'Confirm' });
    expect(confirm).toBeEnabled();

    await user.click(confirm);
    const dialog = screen.getByRole('dialog', { name: 'Confirm trip' });
    await user.click(within(dialog).getByRole('button', { name: 'Confirm trip' }));

    expect(
      await screen.findByText('Trip confirmed: 6 holds created, vouchers issued and the tourist emailed.'),
    ).toBeInTheDocument();
    expect(confirmed).toBe(true);
  });

  it('shows the 409 message when Confirm is refused', async () => {
    givenTrip('ClientAccepted', { workflow: workflow(), versions: [ACCEPTED] });
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/confirm`, () =>
        HttpResponse.json(
          { title: 'Conflict', status: 409, detail: 'Van CAB-1234 is no longer free on 2026-10-11.' },
          { status: 409 },
        ),
      ),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    await user.click(await within(panel).findByRole('button', { name: 'Confirm' }));
    await user.click(
      within(screen.getByRole('dialog', { name: 'Confirm trip' })).getByRole('button', {
        name: 'Confirm trip',
      }),
    );

    expect(await within(panel).findByRole('alert')).toHaveTextContent(
      'Van CAB-1234 is no longer free on 2026-10-11.',
    );
  });

  it('disables Confirm, with the reason, while the proposal was edited since it was priced', async () => {
    givenTrip('ClientAccepted', { workflow: workflow({}, true), versions: [ACCEPTED] });
    renderApp(`/trips/${ID}`);

    const confirm = await within(await nextStep()).findByRole('button', { name: 'Confirm' });
    await waitFor(() => expect(confirm).toBeDisabled());
    expect(confirm).toHaveAccessibleDescription(/edited after it was priced/);
  });

  it('disables Confirm while the newest version is not the accepted one', async () => {
    const repriced = quotation({ id: 'q3', version: 3, status: 'Pending' });
    givenTrip('ClientAccepted', {
      workflow: workflow(),
      versions: [{ ...ACCEPTED, status: 'Superseded' }, repriced],
    });
    renderApp(`/trips/${ID}`);

    const confirm = await within(await nextStep()).findByRole('button', { name: 'Confirm' });
    await waitFor(() => expect(confirm).toHaveAccessibleDescription(/Version 3 has not been accepted/));
    expect(confirm).toBeDisabled();
  });

  it('Edit & resend: edit a day, re-price, then send the new version to the client', async () => {
    const state: State = { workflow: workflow(), versions: [ACCEPTED] };
    givenTrip('ClientAccepted', state);
    const calls: string[] = [];
    let sendBody: unknown;
    server.use(
      http.put(`${API}/api/trip-requests/${ID}/proposal/days/1`, () => {
        calls.push('edit day 1');
        state.workflow = workflow({}, true);
        return HttpResponse.json({
          workflowId: WORKFLOW_ID,
          tripRequestId: ID,
          editedSinceQuotation: true,
          days: [],
          resources: null,
        });
      }),
      http.post(`${API}/api/quotations/q2/calculate`, () => {
        calls.push('calculate q2');
        state.workflow = workflow();
        state.versions = [
          { ...ACCEPTED, status: 'Superseded' },
          quotation({ id: 'q3', version: 3, status: 'Pending', totalUsd: 590 }),
        ];
        return HttpResponse.json({
          quotationId: 'q3',
          version: 3,
          totalLkr: 177000,
          totalUsd: 590,
          previousTotalLkr: 181182,
          previousTotalUsd: 603.94,
          workflowStatus: 'PendingApproval',
          validation: { isValid: true, violations: [], hasHard: false, hasSoft: false },
        });
      }),
      http.post(`${API}/api/quotations/:id/send`, async ({ params, request }) => {
        calls.push(`send ${String(params.id)}`);
        sendBody = await request.json();
        return HttpResponse.json(decision('Approved', 'QuotationSent'));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    await user.click(await within(panel).findByRole('button', { name: 'Edit & resend' }));
    expect(
      within(panel).getByText('The tourist will be asked to accept the updated quote.'),
    ).toBeInTheDocument();
    // Version 2 was already sent and accepted: nothing new to send yet.
    const send = within(panel).getByRole('button', { name: 'Send to client' });
    expect(send).toBeDisabled();
    expect(send).toHaveAccessibleDescription(/Version 2 is already sent to client/);

    // 1. Edit day 1.
    await user.click(within(panel).getByRole('button', { name: 'Edit day 1' }));
    const dialog = await screen.findByRole('dialog', { name: 'Edit day 1 — Kandy' });
    await user.click(await within(dialog).findByRole('checkbox', { name: /Kandy Lake/ }));
    await user.click(within(dialog).getByRole('button', { name: 'Save' }));
    expect(await screen.findByText(/Saved day 1\. Re-price before sending/)).toBeInTheDocument();
    await waitFor(() => expect(send).toHaveAccessibleDescription(/edited after it was priced/));
    expect(within(panel).getByRole('button', { name: 'Confirm' })).toBeDisabled();

    // 2. Re-price: version 3, not sent yet.
    await user.click(within(panel).getByRole('button', { name: 'Re-price' }));
    expect(await screen.findByText(/Re-priced as version 3: USD 603\.94 → USD 590\.00/)).toBeInTheDocument();
    await waitFor(() => expect(within(panel).getByRole('button', { name: 'Send to client' })).toBeEnabled());
    expect(within(panel).getByRole('button', { name: 'Confirm' })).toHaveAccessibleDescription(
      /Version 3 has not been accepted/,
    );

    // 3. Send to client, with a comment for the tourist.
    await user.click(within(panel).getByRole('button', { name: 'Send to client' }));
    const sendDialog = screen.getByRole('dialog', { name: 'Send to client' });
    expect(sendDialog).toHaveTextContent('The tourist will be asked to accept the updated quote.');
    await user.type(within(sendDialog).getByLabelText(/Comment for the tourist/), 'Kandy Lake added');
    await user.click(within(sendDialog).getByRole('button', { name: 'Send to client' }));

    expect(
      await screen.findByText('Version 3 sent to the client. Waiting for them to accept it.'),
    ).toBeInTheDocument();
    expect(calls).toEqual(['edit day 1', 'calculate q2', 'send q3']);
    expect(sendBody).toEqual({ comment: 'Kandy Lake added' });
  });

  it('Edit & resend: swaps the guide for a free one from the availability search', async () => {
    givenTrip('ClientAccepted', { workflow: workflow(), versions: [ACCEPTED] });
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
      http.put(`${API}/api/trip-requests/${ID}/proposal/resources`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({
          workflowId: WORKFLOW_ID,
          tripRequestId: ID,
          editedSinceQuotation: true,
          days: [],
          resources: null,
        });
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    await user.click(await within(panel).findByRole('button', { name: 'Edit & resend' }));
    const select = await within(panel).findByLabelText('Swap guide for');
    await within(select).findByRole('option', { name: /Kamal Silva/ });
    await user.selectOptions(select, 'guide-2');
    await user.click(within(panel).getAllByRole('button', { name: 'Swap' })[0]!);

    expect(await screen.findByText(/Guide swapped\. Re-price before sending/)).toBeInTheDocument();
    expect(body).toEqual({ guideId: 'guide-2' });
    expect(availability?.get('language')).toBe('en');
    expect(availability?.get('pax')).toBe('4');
    expect(availability?.get('from')).toBe('2026-10-10');
  });
});

describe('Trip page actions: Declined', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it("shows the client's reason and replans only with a note, which it posts", async () => {
    givenTrip('ClientDeclined', { workflow: workflow(), versions: [DECLINED] });
    let body: unknown;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/replan`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(decision('Replanned', 'Planning'));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    expect(panel).toHaveAccessibleName('Next step: Declined — needs a decision');
    expect(
      await within(panel).findByText('The client declined version 1: “Too expensive for us”'),
    ).toBeInTheDocument();

    await user.click(within(panel).getByRole('button', { name: 'Replan with note' }));
    const dialog = screen.getByRole('dialog', { name: 'Replan with note' });
    await user.click(within(dialog).getByRole('button', { name: 'Replan' }));
    expect(within(dialog).getByRole('alert')).toHaveTextContent('Write a note for the Planner agent.');
    expect(body).toBeUndefined();

    await user.type(within(dialog).getByLabelText(/Note for the Planner agent/), 'Use 3-star hotels in Ella');
    await user.click(within(dialog).getByRole('button', { name: 'Replan' }));

    expect(await screen.findByText(/Re-planning started/)).toBeInTheDocument();
    expect(body).toEqual({ note: 'Use 3-star hotels in Ella' });
  });

  it('cancels with a reason from the panel (and the header does not repeat Cancel)', async () => {
    givenTrip('ClientDeclined', { workflow: workflow(), versions: [DECLINED] });
    let body: unknown;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/cancel`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json(trip({ status: 'Cancelled' }));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    const cancel = await within(panel).findByRole('button', { name: 'Cancel with reason' });
    expect(screen.queryByRole('button', { name: 'Cancel request' })).not.toBeInTheDocument();
    await user.click(cancel);
    const dialog = await screen.findByRole('dialog', { name: 'Cancel trip request' });
    await user.type(within(dialog).getByLabelText(/Reason/), 'The client found another operator');
    await user.click(within(dialog).getByRole('button', { name: 'Cancel request' }));

    expect(await screen.findByText('Trip request cancelled.')).toBeInTheDocument();
    expect(body).toEqual({ reason: 'The client found another operator' });
  });
});

describe('Trip page actions: Needs operator', () => {
  beforeEach(() => signInAs('OperationsManager'));

  const failed = () =>
    workflow({
      status: 'FailedSafely',
      errorSummary: 'A Hard rule failed: no free vehicle with 6 seats on 2026-10-11.',
      validationResult: {
        isValid: false,
        hasHard: true,
        hasSoft: false,
        violations: [
          { code: 'VEHICLE_FREE', message: 'Van CAB-1234 is held on 2026-10-11.', severity: 'Hard' },
        ],
      },
    });

  it('shows the error summary and the failed checks, and retries planning', async () => {
    givenTrip('NeedsOperator', { workflow: failed(), versions: [] });
    let retried = false;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/start-planning`, () => {
        retried = true;
        return HttpResponse.json({ workflowId: WORKFLOW_ID, workflowStatus: 'Planning' }, { status: 202 });
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    expect(panel).toHaveAccessibleName('Next step: Needs operator');
    const wrong = await within(panel).findByRole('region', { name: 'What went wrong' });
    expect(wrong).toHaveTextContent('A Hard rule failed: no free vehicle with 6 seats on 2026-10-11.');
    expect(within(wrong).getByRole('list', { name: 'Failed checks' })).toHaveTextContent(
      'Hard: Van CAB-1234 is held on 2026-10-11.',
    );
    expect(within(panel).getByRole('button', { name: 'Cancel with reason' })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Cancel request' })).not.toBeInTheDocument();

    await user.click(within(panel).getByRole('button', { name: 'Retry planning' }));

    expect(await screen.findByText(/Planning again/)).toBeInTheDocument();
    expect(retried).toBe(true);
  });

  it('Edit & send manually: prices the trip (no quotation yet) and sends the new version', async () => {
    const state: State = { workflow: failed(), versions: [] };
    givenTrip('NeedsOperator', state);
    const calls: string[] = [];
    let sendBody: unknown;
    server.use(
      http.post(`${API}/api/trip-requests/${ID}/proposal/reprice`, () => {
        calls.push('reprice trip');
        state.versions = [quotation({ id: 'q1', version: 1, status: 'Pending' })];
        return HttpResponse.json({
          quotationId: 'q1',
          version: 1,
          totalLkr: 187220,
          totalUsd: 624.07,
          previousTotalLkr: 0,
          previousTotalUsd: 0,
          workflowStatus: 'PendingApproval',
          validation: { isValid: true, violations: [], hasHard: false, hasSoft: false },
        });
      }),
      http.post(`${API}/api/quotations/:id/calculate`, () => {
        calls.push('calculate');
        return HttpResponse.json({}, { status: 500 });
      }),
      http.post(`${API}/api/quotations/:id/send`, async ({ params, request }) => {
        calls.push(`send ${String(params.id)}`);
        sendBody = await request.json();
        return HttpResponse.json(decision('Approved', 'QuotationSent'));
      }),
    );
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    await user.click(await within(panel).findByRole('button', { name: 'Edit & send manually' }));
    const send = within(panel).getByRole('button', { name: 'Send to client' });
    expect(send).toBeDisabled();
    expect(send).toHaveAccessibleDescription(/no priced version yet/);

    await user.click(within(panel).getByRole('button', { name: 'Re-price' }));
    expect(await screen.findByText('Priced as version 1: USD 624.07.')).toBeInTheDocument();
    await waitFor(() => expect(within(panel).getByRole('button', { name: 'Send to client' })).toBeEnabled());

    await user.click(within(panel).getByRole('button', { name: 'Send to client' }));
    const dialog = screen.getByRole('dialog', { name: 'Send to client' });
    await user.click(within(dialog).getByRole('button', { name: 'Send to client' }));

    expect(await screen.findByText(/Version 1 sent to the client/)).toBeInTheDocument();
    expect(calls).toEqual(['reprice trip', 'send q1']);
    expect(sendBody).toEqual({});
  });

  it('says there is nothing to edit when the agents produced no proposal', async () => {
    givenTrip('NeedsOperator', {
      workflow: {
        ...failed(),
        finalOutcome: null,
        errorSummary: 'The agent service did not answer in time.',
      },
      versions: [],
    });
    const { user } = renderApp(`/trips/${ID}`);

    const panel = await nextStep();
    expect(await within(panel).findByText('The agent service did not answer in time.')).toBeInTheDocument();
    await user.click(within(panel).getByRole('button', { name: 'Edit & send manually' }));
    expect(within(panel).getByText(/no proposal to edit\. Retry planning instead/)).toBeInTheDocument();
  });
});

describe('Trip page actions: nothing to do', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it.each(['Submitted', 'Planning', 'QuotationSent', 'Confirmed', 'Completed', 'Cancelled'])(
    'shows no next-step panel when the trip is %s',
    async (status) => {
      givenTrip(status, { workflow: workflow(), versions: [ACCEPTED] });
      renderApp(`/trips/${ID}`);

      expect(await screen.findByRole('heading', { name: 'Trip request' })).toBeInTheDocument();
      expect(screen.queryByRole('region', { name: /^Next step/ })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: 'Confirm' })).not.toBeInTheDocument();
    },
  );
});
