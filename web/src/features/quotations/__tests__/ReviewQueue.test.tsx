import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { attentionItem } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ROWS: Record<string, object[]> = {
  ClientAccepted: [attentionItem({ tripRequestId: 'trip-a', objective: 'Honeymoon in Ella' })],
  ClientDeclined: [
    attentionItem({
      tripRequestId: 'trip-d',
      objective: 'Cultural triangle for 4',
      status: 'ClientDeclined',
      detail: 'Too expensive for us',
    }),
  ],
  NeedsOperator: [
    attentionItem({
      tripRequestId: 'trip-n',
      objective: 'South coast surf week',
      status: 'NeedsOperator',
      detail: 'No free vehicle with 6 seats on 2026-10-11.',
      totalUsd: null,
    }),
  ],
};

/** Answers GET /api/dashboard/attention per status and records the statuses asked for. */
function givenAttention() {
  const asked: (string | null)[] = [];
  server.use(
    http.get(`${API}/api/dashboard/attention`, ({ request }) => {
      const status = new URL(request.url).searchParams.get('status');
      asked.push(status);
      return HttpResponse.json(ROWS[status ?? ''] ?? []);
    }),
  );
  return asked;
}

describe('Review queue (trips that need the operator)', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('has Accepted, Declined and Needs operator tabs, each listing its trips with a link and the detail', async () => {
    const asked = givenAttention();
    const { user, location } = renderApp('/approvals');

    const tabs = await screen.findByRole('tablist', { name: 'Trips that need you' });
    expect(
      within(tabs)
        .getAllByRole('tab')
        .map((t) => t.textContent),
    ).toEqual(['Accepted', 'Declined', 'Needs operator']);
    const accepted = await screen.findByRole('list', { name: 'Accepted trips' });
    expect(within(accepted).getByRole('link', { name: 'Honeymoon in Ella' })).toHaveAttribute(
      'href',
      '/trips/trip-a',
    );
    expect(accepted).toHaveTextContent('Version 2 accepted');
    expect(accepted).toHaveTextContent('USD 603.94');
    expect(accepted).toHaveTextContent('Anna Silva · 10 Oct 2026 – 14 Oct 2026 · 4 travellers');
    expect(asked[0]).toBe('ClientAccepted');

    await user.click(screen.getByRole('tab', { name: 'Declined' }));
    const declined = await screen.findByRole('list', { name: 'Declined trips' });
    expect(declined).toHaveTextContent('Too expensive for us');
    await waitFor(() => expect(location()).toBe('/approvals?tab=ClientDeclined'));

    await user.click(screen.getByRole('tab', { name: 'Needs operator' }));
    const stuck = await screen.findByRole('list', { name: 'Needs operator trips' });
    expect(stuck).toHaveTextContent('No free vehicle with 6 seats on 2026-10-11.');
    expect(stuck).toHaveTextContent('Not priced');
    expect(asked).toEqual(['ClientAccepted', 'ClientDeclined', 'NeedsOperator']);
  });

  it('opens the first tab for an old link (e.g. the removed Pending review tab)', async () => {
    givenAttention();
    renderApp('/approvals?tab=PendingApproval');

    expect(await screen.findByRole('tab', { name: 'Accepted' })).toHaveAttribute('aria-selected', 'true');
    expect(await screen.findByRole('list', { name: 'Accepted trips' })).toBeInTheDocument();
  });

  it('shows an empty state, and the error state with Retry', async () => {
    const empty = renderApp('/approvals?tab=NeedsOperator');
    expect(await screen.findByText('Nothing is waiting here')).toBeInTheDocument();
    expect(screen.getByText(/the agents handled everything/)).toBeInTheDocument();
    empty.unmount();

    server.use(
      http.get(`${API}/api/dashboard/attention`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })),
    );
    renderApp('/approvals');
    expect(await screen.findByRole('button', { name: 'Retry' })).toBeInTheDocument();
  });
});
