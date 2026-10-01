import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
import { paged } from './fixtures';

export const API = 'http://api.test';

export const CITIES = ['Colombo', 'Ella', 'Galle', 'Kandy', 'Nuwara Eliya', 'Sigiriya'];

export const DASHBOARD_ACTIONS = {
  proposalsToReview: 0,
  clientAcceptedToConfirm: 0,
  guideChangeRequests: 0,
  recentCancellations: 0,
  declinedQuotations: 0,
};

/** Defaults so pages that load extra data (e.g. the dashboard) do not fail. Tests override with server.use. */
export const server = setupServer(
  http.get(`${API}/api/workflows`, () => HttpResponse.json(paged([]))),
  http.get(`${API}/api/trip-requests`, () => HttpResponse.json(paged([]))),
  http.get(`${API}/api/reports/:report`, () => HttpResponse.json([])),
  // No stored quotation yet: the review page falls back to the agents' proposal.
  http.get(`${API}/api/quotations/:id`, () =>
    HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
  ),
  // One version only: the review page shows no side-by-side comparison.
  http.get(`${API}/api/quotations`, () => HttpResponse.json(paged([]))),
  // The bell in the top bar of every staff page.
  http.get(`${API}/api/notifications/mine`, () => HttpResponse.json({ unreadCount: 0, items: [] })),
  // The manager dashboard's guide change requests panel.
  http.get(`${API}/api/guide-change-requests`, () => HttpResponse.json([])),
  // The trips list's city filter.
  http.get(`${API}/api/attractions/cities`, () => HttpResponse.json(CITIES)),
  // A trip without an agent workflow (the trip page then has no "Open review" link).
  http.get(`${API}/api/trip-requests/:id/workflow`, () =>
    HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
  ),
  // No proposal to explain yet: the review page hides "Why this plan".
  http.get(`${API}/api/trip-requests/:id/plan-explanation`, () =>
    HttpResponse.json({ title: 'Not found', status: 404 }, { status: 404 }),
  ),
  // The manager dashboard: nothing waiting and no trips today or tomorrow.
  http.get(`${API}/api/dashboard/actions`, () => HttpResponse.json(DASHBOARD_ACTIONS)),
  http.get(`${API}/api/dashboard/upcoming`, () => HttpResponse.json([])),
);
