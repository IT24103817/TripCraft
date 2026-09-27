import { expect, type APIRequestContext } from '@playwright/test';

export const API_URL = (process.env.API_URL ?? 'http://localhost:5080').replace(/\/$/, '');
export const PASSWORD = 'Passw0rd!';
export const TOURIST = 'tourist1@tripcraft.test';
export const MANAGER = 'manager1@tripcraft.test';

const MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October',
  'November', 'December'];
const isoDate = (d: Date) => d.toISOString().slice(0, 10);

/**
 * A 5-day window starting 30-729 days after today, chosen at random for every trip. Reruns and earlier demo trips
 * then (almost) never hold the same guide, vehicle or rooms, so no database clean-up is needed between runs.
 */
export function freshTripDates(today = new Date()) {
  const offset = 30 + Math.floor(Math.random() * 700);
  const start = new Date(Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate() + offset));
  const end = new Date(start);
  end.setUTCDate(start.getUTCDate() + 4);
  const label = start.getUTCMonth() === end.getUTCMonth()
    ? `${start.getUTCDate()}-${end.getUTCDate()} ${MONTHS[end.getUTCMonth()]}`
    : `${start.getUTCDate()} ${MONTHS[start.getUTCMonth()]} - ${end.getUTCDate()} ${MONTHS[end.getUTCMonth()]}`;
  return { startDate: isoDate(start), endDate: isoDate(end), label };
}

/** The demo request from PLAN.md section 6, on fresh dates (see freshTripDates). */
export function demoTrip(budgetUsd = 1500) {
  const { startDate, endDate, label } = freshTripDates();
  return {
    objective: `5 days for 4 people, ${label}, Kandy and Ella, prefer the hill-country train, English-speaking guide.`,
    startDate,
    endDate,
    pax: 4,
    budgetUsd,
    preferences: { transport: 'train', language: 'en' },
    nationality: 'United Kingdom',
    passportNumber: 'N1234567',
  };
}

/** The login limit is 5 per minute per IP; the whole suite needs more, so a 429 waits for the next window once. */
export async function login(request: APIRequestContext, email: string): Promise<string> {
  const post = () => request.post(`${API_URL}/api/auth/login`, { data: { email, password: PASSWORD } });
  let response = await post();
  if (response.status() === 429) {
    await new Promise((resolve) => setTimeout(resolve, 61_000));
    response = await post();
  }
  expect(response.status(), `login ${email}`).toBe(200);
  return (await response.json()).accessToken as string;
}

const auth = (token: string) => ({ Authorization: `Bearer ${token}` });

export async function createTripAndStartPlanning(request: APIRequestContext, token: string, budgetUsd = 1500) {
  const created = await request.post(`${API_URL}/api/trip-requests`, { headers: auth(token), data: demoTrip(budgetUsd) });
  expect(created.status()).toBe(201);
  const trip = await created.json();

  const started = await request.post(`${API_URL}/api/trip-requests/${trip.id}/start-planning`, { headers: auth(token) });
  expect(started.status()).toBe(202);
  const planning = await started.json();
  expect(planning.workflowStatus, planning.errorSummary ?? '').toBe('Planning');
  return { tripId: trip.id as string, workflowId: planning.workflowId as string };
}

/** Polls GET /api/workflows/{id} every 5 s until it leaves Planning (max 3 minutes). */
export async function waitForWorkflow(request: APIRequestContext, token: string, workflowId: string) {
  const deadline = Date.now() + 3 * 60_000;
  for (;;) {
    const response = await request.get(`${API_URL}/api/workflows/${workflowId}`, { headers: auth(token) });
    expect(response.status()).toBe(200);
    const workflow = await response.json();
    if (workflow.status !== 'Planning') return workflow;
    if (Date.now() > deadline) throw new Error(`Workflow ${workflowId} still Planning after 3 minutes`);
    await new Promise((resolve) => setTimeout(resolve, 5000));
  }
}

export async function tripStatus(request: APIRequestContext, token: string, tripId: string): Promise<string> {
  const response = await request.get(`${API_URL}/api/trip-requests/${tripId}`, { headers: auth(token) });
  expect(response.status()).toBe(200);
  return (await response.json()).status;
}
