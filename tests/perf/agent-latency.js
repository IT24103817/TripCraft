// PLAN.md section 11: agent workflow latency over 5 runs. Each run creates the section 6 demo request,
// starts planning and polls GET /api/workflows/{id} until it leaves Planning. The time to PendingApproval
// is the metric; any other final status fails the check and is counted per status.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { API_URL, authHeaders, login, summaryTo } from './common.js';

const timeToPendingApproval = new Trend('time_to_pending_approval', true);
const timeToFinalStatus = new Trend('time_to_final_status', true);
const finalStatus = new Counter('final_status');

export const options = {
  scenarios: { runs: { executor: 'per-vu-iterations', vus: 1, iterations: 5, maxDuration: '20m' } },
  thresholds: { checks: ['rate==1.0'] },
};

export function setup() {
  return { token: login(__ENV.EMAIL || 'tourist1@tripcraft.test') };
}

export default function ({ token }) {
  const params = authHeaders(token);
  const trip = http.post(`${API_URL}/api/trip-requests`, JSON.stringify({
    objective: '5 days for 4 people, 10-14 October, Kandy and Ella, prefer the hill-country train, English-speaking guide.',
    startDate: '2026-10-10', endDate: '2026-10-14', pax: 4, budgetUsd: 1500,
    preferences: { transport: 'train', language: 'en' }, nationality: 'United Kingdom', passportNumber: 'N1234567',
  }), params);
  check(trip, { 'trip created': (r) => r.status === 201 });

  const started = Date.now();
  const planning = http.post(`${API_URL}/api/trip-requests/${trip.json('id')}/start-planning`, null, params);
  check(planning, { 'planning started': (r) => r.status === 202 });
  const workflowId = planning.json('workflowId');

  let status = planning.json('workflowStatus');
  let workflow;
  while (status === 'Planning' && Date.now() - started < 3 * 60_000) {
    sleep(2);
    workflow = http.get(`${API_URL}/api/workflows/${workflowId}`, { ...params, tags: { name: 'GET /api/workflows/{id}' } });
    status = workflow.json('status');
  }

  const elapsed = Date.now() - started;
  timeToFinalStatus.add(elapsed, { status });
  finalStatus.add(1, { status });
  if (status === 'PendingApproval') timeToPendingApproval.add(elapsed);
  check(status, { 'reached PendingApproval': (s) => s === 'PendingApproval' });
  console.log(`run ${__ITER + 1}: ${status} after ${(elapsed / 1000).toFixed(1)} s` +
    (workflow && workflow.json('errorSummary') ? ` — ${workflow.json('errorSummary')}` : ''));
}

export const handleSummary = summaryTo('agent-latency');
