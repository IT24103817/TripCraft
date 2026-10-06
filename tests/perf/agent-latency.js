// PLAN.md section 11: agent workflow latency over 5 runs. Each run creates the section 6 demo request,
// starts planning and polls GET /api/workflows/{id} until it leaves Planning. In v1.1 a valid proposal is sent to
// the client automatically (workflow Approved), so the time until the quotation is sent is the metric; any other
// final status fails the check and is counted per status.
// PAUSE_SECONDS (default 0) waits between runs, outside the measured time: on a hosted free tier with a per-minute
// token limit (Groq: 8,000 tokens a minute; one trip uses about 12,000) a pause of 120 s measures one trip at a time,
// as in a demo, instead of how fast back-to-back trips drain the quota.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { API_URL, authHeaders, login, summaryTo } from './common.js';

const timeToQuotationSent = new Trend('time_to_quotation_sent', true);
const timeToFinalStatus = new Trend('time_to_final_status', true);
const finalStatus = new Counter('final_status');
const PAUSE_SECONDS = Number(__ENV.PAUSE_SECONDS || 0);

export const options = {
  scenarios: { runs: { executor: 'per-vu-iterations', vus: 1, iterations: 5, maxDuration: '20m' } },
  thresholds: { checks: ['rate==1.0'] },
};

export function setup() {
  return { token: login(__ENV.EMAIL || 'tourist1@tripcraft.test') };
}

/** yyyy-MM-dd, `days` after today (each run gets its own week, so runs do not compete for the same guide). */
function dayFromToday(days) {
  const d = new Date();
  d.setUTCDate(d.getUTCDate() + days);
  return d.toISOString().slice(0, 10);
}

export default function ({ token }) {
  const params = authHeaders(token);
  const offset = 60 + __ITER * 7;
  const trip = http.post(`${API_URL}/api/trip-requests`, JSON.stringify({
    objective: '5 days for 4 people, Kandy and Ella, prefer the hill-country train, English-speaking guide.',
    startDate: dayFromToday(offset), endDate: dayFromToday(offset + 4), pax: 4, budgetUsd: 1500,
    cities: ['Kandy', 'Ella'],
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
  if (status === 'Approved') timeToQuotationSent.add(elapsed);
  check(status, { 'quotation sent to the client (workflow Approved)': (s) => s === 'Approved' });
  console.log(`run ${__ITER + 1}: ${status} after ${(elapsed / 1000).toFixed(1)} s` +
    (workflow && workflow.json('errorSummary') ? ` — ${workflow.json('errorSummary')}` : ''));
  if (PAUSE_SECONDS > 0 && __ITER < 4) sleep(PAUSE_SECONDS); // not part of the measured time
}

export const handleSummary = summaryTo('agent-latency');
