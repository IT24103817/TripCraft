import { expect, type APIRequestContext } from '@playwright/test';

export const API_URL = (process.env.API_URL ?? 'http://localhost:5080').replace(/\/$/, '');
export const PASSWORD = 'Passw0rd!';
export const TOURIST = 'tourist1@tripcraft.test';
export const MANAGER = 'manager1@tripcraft.test';

/** The demo request from PLAN.md section 6. */
export function demoTrip(budgetUsd = 1500) {
  return {
    objective: '5 days for 4 people, 10-14 October, Kandy and Ella, prefer the hill-country train, English-speaking guide.',
    startDate: '2026-10-10',
    endDate: '2026-10-14',
    pax: 4,
    budgetUsd,
    preferences: { transport: 'train', language: 'en' },
    nationality: 'United Kingdom',
    passportNumber: 'N1234567',
  };
}

export async function login(request: APIRequestContext, email: string): Promise<string> {
  const response = await request.post(`${API_URL}/api/auth/login`, { data: { email, password: PASSWORD } });
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
