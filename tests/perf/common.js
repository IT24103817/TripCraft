import http from 'k6/http';
import { check, fail } from 'k6';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';

export const API_URL = (__ENV.API_URL || 'http://localhost:5080').replace(/\/$/, '');
export const PASSWORD = __ENV.PASSWORD || 'Passw0rd!';
const SUMMARY_DIR = __ENV.SUMMARY_DIR || 'docs/evidence/perf';

export function login(email) {
  const response = http.post(`${API_URL}/api/auth/login`, JSON.stringify({ email, password: PASSWORD }), {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'login' },
  });
  if (!check(response, { 'login 200': (r) => r.status === 200 })) fail(`login ${email} failed: ${response.status}`);
  return response.json('accessToken');
}

export const authHeaders = (token) => ({ headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' } });

/** Writes the k6 summary JSON to docs/evidence/perf/<name>-summary.json (run k6 from the repo root). */
export function summaryTo(name) {
  return (data) => ({
    [`${SUMMARY_DIR}/${name}-summary.json`]: JSON.stringify(data, null, 2),
    stdout: `${textSummary(data, { indent: ' ', enableColors: false })}\n${name}: summary saved to ${SUMMARY_DIR}/${name}-summary.json\n`,
  });
}
