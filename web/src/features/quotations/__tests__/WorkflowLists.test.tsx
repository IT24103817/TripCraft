import { screen, waitFor, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { paged, workflowSummary } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

/** Records every GET /api/workflows query and answers with `rows`, or an empty page once searched. */
function recordWorkflowRequests(rows = [workflowSummary()]) {
  const requests: URLSearchParams[] = [];
  server.use(
    http.get(`${API}/api/workflows`, ({ request }) => {
      const params = new URL(request.url).searchParams;
      requests.push(params);
      return HttpResponse.json(paged(params.get('search') === 'nothing' ? [] : rows));
    }),
  );
  return requests;
}

describe('Agent runs list', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows the trip objective and sends the default sort, search and status to the API', async () => {
    const requests = recordWorkflowRequests();
    const { user } = renderApp('/agent-runs');

    const table = await screen.findByRole('table', { name: 'Agent runs' });
    expect(within(table).getByText(workflowSummary().objective)).toBeInTheDocument();
    expect(requests[0]?.get('sort')).toBe('-startedAt');

    await user.type(screen.getByRole('searchbox'), 'Ella');
    await waitFor(() => expect(requests.at(-1)?.get('search')).toBe('Ella'));

    await user.selectOptions(screen.getByLabelText('Status'), 'Completed');
    await waitFor(() => expect(requests.at(-1)?.get('status')).toBe('Completed'));
    expect(requests.at(-1)?.get('search')).toBe('Ella');
  });

  it('sorts by Started, Finished and Status when a header is clicked', async () => {
    const requests = recordWorkflowRequests();
    const { user } = renderApp('/agent-runs');

    await screen.findByRole('table', { name: 'Agent runs' });
    // Default "-startedAt": clicking Started switches to ascending.
    await user.click(screen.getByRole('button', { name: 'Sort by Started' }));
    await waitFor(() => expect(requests.at(-1)?.get('sort')).toBe('startedAt'));

    await user.click(screen.getByRole('button', { name: 'Sort by Finished' }));
    await waitFor(() => expect(requests.at(-1)?.get('sort')).toBe('finishedAt'));

    await user.click(screen.getByRole('button', { name: 'Sort by Status' }));
    await waitFor(() => expect(requests.at(-1)?.get('sort')).toBe('status'));
    expect(screen.getByRole('columnheader', { name: /Status/ })).toHaveAttribute('aria-sort', 'ascending');
  });

  it('says when no workflow matches the search', async () => {
    recordWorkflowRequests();
    renderApp('/agent-runs?search=nothing');

    expect(await screen.findByText('No agent runs match these filters')).toBeInTheDocument();
  });

  it('redirects the old /workflows links to Agent runs, keeping the filters', async () => {
    recordWorkflowRequests();
    const list = renderApp('/workflows?status=Completed');
    await waitFor(() => expect(list.location()).toBe('/agent-runs?status=Completed'));
    expect(await screen.findByRole('heading', { name: 'Agent runs' })).toBeInTheDocument();
    list.unmount();

    server.use(
      http.get(`${API}/api/workflows/abc`, () => HttpResponse.json({ title: 'Not found' }, { status: 404 })),
    );
    const detail = renderApp('/workflows/abc');
    await waitFor(() => expect(detail.location()).toBe('/agent-runs/abc'));
  });
});
