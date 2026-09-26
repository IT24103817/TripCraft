import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { pendingWorkflow, step, WORKFLOW_ID } from '@/test/fixtures';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

describe('WorkflowDetailPage', () => {
  beforeEach(() => signInAs('Admin'));

  it('renders the agent steps in order with tools, duration, retries and status', async () => {
    server.use(
      http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () => HttpResponse.json(pendingWorkflow())),
      http.get(`${API}/api/workflows/${WORKFLOW_ID}/steps`, () =>
        HttpResponse.json([
          step(1, 'planner'),
          step(2, 'itinerary', { retries: 1 }),
          step(3, 'resources'),
          step(4, 'validation', { status: 'Failed', durationMs: 450 }),
        ]),
      ),
    );
    const { user } = renderApp(`/workflows/${WORKFLOW_ID}`);

    const timeline = await screen.findByRole('list', { name: 'Agent steps' });
    const items = within(timeline)
      .getAllByRole('listitem')
      .filter((li) => li.parentElement === timeline);
    expect(items.map((li) => li.querySelector('span.font-semibold')?.textContent)).toEqual([
      '1. Planner / Coordinator',
      '2. Itinerary Analysis',
      '3. Resource & Action',
      '4. Validation & Safety',
    ]);
    expect(items[1]).toHaveTextContent('1 retry');
    expect(items[3]).toHaveTextContent('Failed');
    expect(items[3]).toHaveTextContent('450 ms');
    expect(within(timeline).getByRole('list', { name: 'Tool calls of step 1' })).toHaveTextContent(
      'get_attractions · 120 ms',
    );

    await user.click(within(items[0]!).getByText('Summaries and validation result'));
    expect(items[0]).toHaveTextContent('"cities"');
  });

  it('says it is refreshing while the agents are still planning', async () => {
    server.use(
      http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () =>
        HttpResponse.json(
          pendingWorkflow({ status: 'Planning', validationResult: null, finalOutcome: null }),
        ),
      ),
      http.get(`${API}/api/workflows/${WORKFLOW_ID}/steps`, () => HttpResponse.json([step(1, 'planner')])),
    );
    renderApp(`/workflows/${WORKFLOW_ID}`);

    expect(await screen.findByText('Planning — refreshing every 5 s')).toBeInTheDocument();
  });

  it('shows the rules as not checked when the agents failed before a proposal existed', async () => {
    server.use(
      http.get(`${API}/api/workflows/${WORKFLOW_ID}`, () =>
        HttpResponse.json(
          pendingWorkflow({
            status: 'FailedSafely',
            finalOutcome: null,
            validationResult: {
              isValid: false,
              hasHard: true,
              hasSoft: false,
              violations: [
                { code: 'AGENT_FAILED', message: 'resources: tool returned 503', severity: 'Hard' },
              ],
            },
          }),
        ),
      ),
      http.get(`${API}/api/workflows/${WORKFLOW_ID}/steps`, () => HttpResponse.json([step(1, 'planner')])),
    );
    renderApp(`/workflows/${WORKFLOW_ID}`);

    const checklist = await screen.findByRole('list', { name: 'Validation checklist' });
    expect(within(checklist).getByText('Guide exists').closest('li')).toHaveTextContent('— not checked');
    expect(checklist).not.toHaveTextContent('— passed');
    expect(checklist).toHaveTextContent('AGENT_FAILED: resources: tool returned 503');
    expect(screen.getByText(/no rule was checked/)).toBeInTheDocument();
  });
});
