import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { paged } from './fixtures';

export const API = 'http://api.test';

/** Defaults so pages that load extra data (e.g. the dashboard) do not fail. Tests override with server.use. */
export const server = setupServer(
  http.get(`${API}/api/workflows`, () => HttpResponse.json(paged([]))),
  http.get(`${API}/api/trip-requests`, () => HttpResponse.json(paged([]))),
  http.get(`${API}/api/reports/:report`, () => HttpResponse.json([])),
  // No stored quotation yet: the review page falls back to the agents' proposal.
  http.get(`${API}/api/quotations/:id`, () =>
    HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
  ),
);
