import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { lastMonths } from '@/shared/utils/format';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ACTIONS = {
  proposalsToReview: 3,
  clientAcceptedToConfirm: 1,
  guideChangeRequests: 2,
  recentCancellations: 0,
  declinedQuotations: 4,
};

function upcoming(overrides: Record<string, unknown> = {}) {
  return {
    tripRequestId: 'trip-1',
    objective: '5 days in Kandy and Ella',
    startDate: '2026-10-02',
    endDate: '2026-10-06',
    pax: 4,
    status: 'InProgress',
    touristName: 'Anna Silva',
    guideName: 'Nimal Perera',
    vehicleRegistrationNo: 'CAB-1234',
    vehicleType: 'Van',
    day: 'today',
    dayNumber: 1,
    ...overrides,
  };
}

describe('Manager dashboard', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('puts "Needs your action" first, with five counts that open the right place', async () => {
    server.use(http.get(`${API}/api/dashboard/actions`, () => HttpResponse.json(ACTIONS)));
    renderApp('/dashboard');

    const section = await screen.findByRole('region', { name: 'Needs your action' });
    const link = (name: string) => within(section).findByRole('link', { name });
    expect(await link('Proposals to review: 3')).toHaveAttribute('href', '/approvals');
    expect(await link('Accepted by the client: 1')).toHaveAttribute('href', '/trips?status=ClientAccepted');
    expect(await link('Guide change requests: 2')).toHaveAttribute('href', '#guide-change-requests');
    expect(await link('Cancellations (7 days): 0')).toHaveAttribute('href', '/trips?status=Cancelled');
    expect(await link('Declined quotations: 4')).toHaveAttribute('href', '/approvals');
    // The guide change link lands on the panel further down this page.
    expect(document.getElementById('guide-change-requests')).toContainElement(
      screen.getByRole('heading', { name: 'Guide change requests' }),
    );
    // "Needs your action" comes before the figures.
    const headings = screen.getAllByRole('heading', { level: 2 }).map((h) => h.textContent);
    expect(headings.indexOf('Needs your action')).toBeLessThan(headings.indexOf('Today and tomorrow'));
  });

  it('opens the trips list filtered by status from a tile', async () => {
    server.use(http.get(`${API}/api/dashboard/actions`, () => HttpResponse.json(ACTIONS)));
    const { user, location } = renderApp('/dashboard');

    await user.click(await screen.findByRole('link', { name: 'Accepted by the client: 1' }));

    await waitFor(() => expect(location()).toBe('/trips?status=ClientAccepted'));
    expect(await screen.findByLabelText('Status')).toHaveValue('ClientAccepted');
  });

  it("lists today's and tomorrow's trips with their guide and vehicle", async () => {
    server.use(
      http.get(`${API}/api/dashboard/upcoming`, () =>
        HttpResponse.json([
          upcoming(),
          upcoming({
            tripRequestId: 'trip-2',
            objective: 'Galle day trip',
            status: 'Confirmed',
            guideName: null,
            vehicleRegistrationNo: null,
            day: 'tomorrow',
          }),
        ]),
      ),
    );
    renderApp('/dashboard');

    const today = await screen.findByRole('list', { name: 'Trips today' });
    expect(within(today).getByRole('link', { name: '5 days in Kandy and Ella' })).toHaveAttribute(
      'href',
      '/trips/trip-1',
    );
    expect(today).toHaveTextContent('Guide: Nimal Perera · Van CAB-1234');
    expect(today).toHaveTextContent('Day 1 · Anna Silva · 4 travellers');
    const tomorrow = screen.getByRole('list', { name: 'Trips tomorrow' });
    expect(tomorrow).toHaveTextContent('Guide: none · No vehicle');
  });

  it('says when no trip runs today or tomorrow', async () => {
    renderApp('/dashboard');
    expect(await screen.findByText('No trips today or tomorrow')).toBeInTheDocument();
  });

  it('charts the revenue of the last six months, with a table for screen readers', async () => {
    const months = lastMonths(6);
    let asked: URLSearchParams | null = null;
    server.use(
      http.get(`${API}/api/reports/revenue`, ({ request }) => {
        const params = new URL(request.url).searchParams;
        // The six-month chart asks from the 1st of the oldest month.
        if (params.get('from') === `${months[0]}-01`) asked = params;
        return HttpResponse.json([{ month: months[5], quotations: 2, totalLkr: 300000, totalUsd: 1000 }]);
      }),
    );
    renderApp('/dashboard');

    const table = await screen.findByRole('table', { name: 'Revenue for the last 6 months, in USD' });
    expect(within(table).getAllByRole('row')).toHaveLength(6);
    await waitFor(() =>
      expect(within(table).getByRole('row', { name: `${months[5]} USD 1,000.00` })).toBeInTheDocument(),
    );
    expect(within(table).getByRole('row', { name: `${months[0]} USD 0.00` })).toBeInTheDocument();
    expect(asked).not.toBeNull();
  });

  it('shows the error state with Retry when the action counts fail', async () => {
    server.use(
      http.get(`${API}/api/dashboard/actions`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })),
    );
    renderApp('/dashboard');

    const section = await screen.findByRole('region', { name: 'Needs your action' });
    expect(await within(section).findByRole('button', { name: 'Retry' })).toBeInTheDocument();
  });
});

describe('Admin dashboard', () => {
  it('has no manager action tiles and lists the latest agent runs', async () => {
    signInAs('Admin');
    renderApp('/dashboard');

    expect(await screen.findByRole('heading', { name: 'Latest agent runs' })).toBeInTheDocument();
    expect(screen.queryByRole('region', { name: 'Needs your action' })).not.toBeInTheDocument();
  });
});
