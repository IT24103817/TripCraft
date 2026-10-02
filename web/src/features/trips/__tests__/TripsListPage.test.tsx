import { screen, within, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, trip } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

describe('TripsListPage', () => {
  let requests: URL[];

  beforeEach(() => {
    signInAs('OperationsManager');
    requests = [];
    server.use(
      http.get(`${API}/api/trip-requests`, ({ request }) => {
        const url = new URL(request.url);
        requests.push(url);
        const page = Number(url.searchParams.get('page'));
        const rows = Array.from({ length: 2 }, (_, i) =>
          trip({
            id: `trip-${page}-${i}`,
            objective: `Trip ${page}-${i}`,
            status: i === 0 ? 'Submitted' : 'ClientAccepted',
          }),
        );
        return HttpResponse.json(paged(rows, 45, page, 20));
      }),
    );
  });

  it('renders rows and pagination from the API', async () => {
    const { user } = renderApp('/trips');

    const table = await screen.findByRole('table', { name: 'Trip requests' });
    expect(within(table).getByText('Trip 1-0')).toBeInTheDocument();
    expect(within(table).getByText('Accepted')).toBeInTheDocument();
    expect(within(table).getAllByText('Kandy, Ella')).toHaveLength(2);
    expect(screen.getByText('1–20 of 45')).toBeInTheDocument();
    expect(screen.getByText('Page 1 of 3')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Next' }));

    expect(await screen.findByText('Trip 2-0')).toBeInTheDocument();
    expect(requests.at(-1)?.searchParams.get('page')).toBe('2');
  });

  it('clicking a sortable header changes the sort query parameter', async () => {
    const { user } = renderApp('/trips');
    await screen.findByText('Trip 1-0');
    expect(requests.at(-1)?.searchParams.get('sort')).toBe('-createdAt');

    await user.click(screen.getByRole('button', { name: 'Sort by Start' }));
    await screen.findByRole('columnheader', { name: /Start/, description: undefined });
    await expectLastSort('startDate');
    expect(screen.getByRole('columnheader', { name: /Start/ })).toHaveAttribute('aria-sort', 'ascending');

    await user.click(screen.getByRole('button', { name: 'Sort by Start' }));
    await expectLastSort('-startDate');
  });

  it('filters by status through the API', async () => {
    const { user } = renderApp('/trips');
    await screen.findByText('Trip 1-0');

    await user.selectOptions(screen.getByLabelText('Status'), 'Submitted');
    await expectLast('status', 'Submitted');

    await user.selectOptions(screen.getByLabelText('Status'), 'Needs operator');
    await expectLast('status', 'NeedsOperator');
  });

  it('offers every v1.1 status in the status filter, with readable labels', async () => {
    renderApp('/trips');
    await screen.findByText('Trip 1-0');

    const options = within(screen.getByLabelText('Status')).getAllByRole('option');
    expect(options.map((o) => o.textContent)).toEqual([
      'All',
      'Submitted',
      'Planning',
      'Quotation sent',
      'Accepted',
      'Confirmed',
      'In progress',
      'Completed',
      'Declined',
      'Needs operator',
      'Cancelled',
    ]);
  });

  it('filters by several cities at once (cities=A&cities=B, a trip must visit all of them)', async () => {
    const { user } = renderApp('/trips');
    await screen.findByText('Trip 1-0');

    const cities = screen.getByRole('group', { name: 'Cities (all of)' });
    await user.click(within(cities).getByRole('checkbox', { name: 'Kandy' }));
    await user.click(within(cities).getByRole('checkbox', { name: 'Ella' }));

    await waitFor(() => expect(requests.at(-1)?.searchParams.getAll('cities')).toEqual(['Kandy', 'Ella']));
    expect(requests.at(-1)?.search).not.toContain('cities%5B');

    await user.click(within(cities).getByRole('checkbox', { name: 'Kandy' }));
    await waitFor(() => expect(requests.at(-1)?.searchParams.getAll('cities')).toEqual(['Ella']));
  });

  it('shows an error state with Retry on a 500 and recovers', async () => {
    let fail = true;
    server.use(
      http.get(`${API}/api/trip-requests`, () =>
        fail
          ? HttpResponse.json({ title: 'An unexpected error occurred', status: 500 }, { status: 500 })
          : HttpResponse.json(paged([trip({ objective: 'Recovered trip' })])),
      ),
    );
    const { user } = renderApp('/trips');

    expect(await screen.findByText('Could not load this page')).toBeInTheDocument();
    fail = false;
    await user.click(screen.getByRole('button', { name: 'Retry' }));

    expect(await screen.findByText('Recovered trip')).toBeInTheDocument();
  });

  async function expectLast(name: string, value: string) {
    await waitFor(() => expect(requests.at(-1)?.searchParams.get(name)).toBe(value));
  }

  async function expectLastSort(value: string) {
    await expectLast('sort', value);
  }
});
