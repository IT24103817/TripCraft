import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { paged } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const ROW = {
  id: 'a1',
  at: '2026-09-26T04:12:55Z',
  actorEmail: 'tourist1@tripcraft.test',
  actorRole: 'Tourist',
  action: 'TripRequestStatusChanged',
  entity: 'TripRequest',
  entityId: '3cec1d1e-0bcd-44ff-b4b4-70b9a2930786',
  before: '{"status":"Submitted"}',
  after: '{"status":"Planning"}',
};

describe('AuditLogPage', () => {
  it('lists audit rows for an Admin and sends the entity filter and sort to the API', async () => {
    signInAs('Admin');
    const requests: URLSearchParams[] = [];
    server.use(
      http.get(`${API}/api/admin/audit-logs`, ({ request }) => {
        requests.push(new URL(request.url).searchParams);
        return HttpResponse.json(paged([ROW]));
      }),
    );
    const { user } = renderApp('/admin/audit-logs');

    const table = await screen.findByRole('table', { name: 'Audit log' });
    expect(within(table).getByText('tourist1@tripcraft.test')).toBeInTheDocument();
    expect(within(table).getByText('TripRequestStatusChanged')).toBeInTheDocument();
    expect(requests[0]?.get('sort')).toBe('-at');

    await user.selectOptions(screen.getByLabelText('Entity'), 'AgentWorkflow');
    await vi.waitFor(() => expect(requests.at(-1)?.get('entity')).toBe('AgentWorkflow'));
  });

  it('shows the error state when the API fails', async () => {
    signInAs('Admin');
    server.use(
      http.get(`${API}/api/admin/audit-logs`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })),
    );
    renderApp('/admin/audit-logs');

    expect(await screen.findByText('Could not load this page')).toBeInTheDocument();
  });

  it('is 403 for an Operations Manager', async () => {
    signInAs('OperationsManager');
    renderApp('/admin/audit-logs');

    expect(
      await screen.findByRole('heading', { name: 'You do not have access to this page' }),
    ).toBeInTheDocument();
  });
});
