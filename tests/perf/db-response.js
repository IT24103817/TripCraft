// Spec section 12, Performance: database response time under concurrent load.
// GET /health times one real database round trip and returns it as dbLatencyMs; this script records it as a
// k6 Trend (db_latency_ms) next to the HTTP timings, 30 users for 30 s.
import http from 'k6/http';
import { check } from 'k6';
import { Trend } from 'k6/metrics';
import { API_URL, summaryTo } from './common.js';

const dbLatency = new Trend('db_latency_ms', true);

export const options = {
  vus: 30,
  duration: '30s',
  thresholds: {
    db_latency_ms: ['p(95)<100'], // ms, one database round trip
    http_req_failed: ['rate<0.01'],
    checks: ['rate>0.99'],
  },
};

export default function () {
  const response = http.get(`${API_URL}/health`, { tags: { name: 'GET /health' } });
  const ok = check(response, {
    'status 200': (r) => r.status === 200,
    'db ok': (r) => r.status === 200 && r.json('db') === 'ok',
  });
  if (ok) dbLatency.add(response.json('dbLatencyMs'));
}

export const handleSummary = summaryTo('db-response');
