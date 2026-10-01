import { screen, within } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { renderApp, signInAs } from '@/test/render';
import { API, server } from '@/test/server';

const REQUEST = {
  id: 'gcr-1',
  tripRequestId: '11111111-1111-1111-1111-111111111111',
  tripObjective: '5 days in Kandy and Ella',
  startDate: '2026-10-10',
  endDate: '2026-10-14',
  pax: 4,
  language: 'en',
  guideId: 'g1',
  guideName: 'Nimal Perera',
  reason: 'Family emergency',
  status: 'Open',
  candidates: [
    { id: 'g2', name: 'Kamal Silva', languages: ['en'], maxPax: 8 },
    { id: 'g3', name: 'Sunil Fernando', languages: ['en', 'de'], maxPax: 10 },
  ],
};

describe('Guide change requests on the dashboard', () => {
  beforeEach(() => signInAs('OperationsManager'));

  it('shows an empty state when no guide asked to be replaced', async () => {
    renderApp('/dashboard');

    expect(await screen.findByText('No guide change requests')).toBeInTheDocument();
  });

  it('lists an open request and swaps the guide for a chosen candidate', async () => {
    let body: unknown = null;
    server.use(
      http.get(`${API}/api/guide-change-requests`, () => HttpResponse.json([REQUEST])),
      http.post(`${API}/api/guide-change-requests/gcr-1/resolve`, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({
          ...REQUEST,
          status: 'Resolved',
          replacementGuideId: 'g3',
          replacementGuideName: 'Sunil Fernando',
          candidates: [],
        });
      }),
    );
    const { user } = renderApp('/dashboard');

    const list = await screen.findByRole('list', { name: 'Guide change requests' });
    expect(list).toHaveTextContent('Nimal Perera asked to be replaced on 5 days in Kandy and Ella');
    expect(list).toHaveTextContent('10 Oct 2026 – 14 Oct 2026');
    expect(list).toHaveTextContent('Reason: Family emergency');

    const swap = within(list).getByRole('button', { name: 'Swap guide' });
    expect(swap).toBeDisabled();
    await user.selectOptions(within(list).getByLabelText('Replacement for Nimal Perera'), 'g3');
    await user.click(swap);

    expect(await screen.findByText('Sunil Fernando replaces Nimal Perera on this trip.')).toBeInTheDocument();
    expect(body).toEqual({ replacementGuideId: 'g3' });
  });

  it('is not shown to an Admin (the API is Operations Manager only)', async () => {
    signInAs('Admin');
    renderApp('/dashboard');

    expect(await screen.findByRole('heading', { name: 'Dashboard' })).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: 'Guide change requests' })).not.toBeInTheDocument();
  });
});
