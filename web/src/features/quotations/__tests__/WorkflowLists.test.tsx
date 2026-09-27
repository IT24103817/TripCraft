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

describe('Workflow monitor and approval inbox lists', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows the trip objective and sends the default sort, search and status to the API', async () => {
    const requests = recordWorkflowRequests();
    const { user } = renderApp('/workflows');

    const table = await screen.findByRole('table', { name: 'Agent workflows' });
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
    const { user } = renderApp('/workflows');

    await screen.findByRole('table', { name: 'Agent workflows' });
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
    renderApp('/workflows?search=nothing');

    expect(await screen.findByText('No workflows match these filters')).toBeInTheDocument();
  });

  it('searches the approval inbox and keeps the tab status and the search together', async () => {
    const requests = recordWorkflowRequests();
    const { user } = renderApp('/approvals');

    const table = await screen.findByRole('table', { name: 'Workflows waiting for a decision' });
    expect(within(table).getByText(workflowSummary().objective)).toBeInTheDocument();
    expect(requests[0]?.get('status')).toBe('PendingApproval');

    await user.type(screen.getByRole('searchbox'), 'Kandy');
    await waitFor(() => expect(requests.at(-1)?.get('search')).toBe('Kandy'));

    await user.click(screen.getByRole('tab', { name: 'Revision requested' }));
    await waitFor(() => expect(requests.at(-1)?.get('status')).toBe('RevisionRequested'));
    expect(requests.at(-1)?.get('search')).toBe('Kandy');

    await user.click(screen.getByRole('button', { name: 'Sort by Started' }));
    await waitFor(() => expect(requests.at(-1)?.get('sort')).toBe('startedAt'));
  });

  it('shows the error state with Retry and an empty state for a search with no match', async () => {
    server.use(http.get(`${API}/api/workflows`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })));
    const failing = renderApp('/approvals');
    expect(await screen.findByRole('button', { name: 'Retry' })).toBeInTheDocument();
    failing.unmount();

    recordWorkflowRequests();
    renderApp('/approvals?search=nothing');
    expect(await screen.findByText('No proposals match your search')).toBeInTheDocument();
  });
});
