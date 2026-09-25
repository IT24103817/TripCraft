// Login under load: 20 VUs for 30 s on POST /api/auth/login.
// The API allows 5 login attempts per minute per IP (PLAN.md section 10), so from one machine most requests
// are answered 429 by design. 200 and 429 both count as expected; the test measures that the endpoint and the
// limiter stay fast and never fail (5xx) under load, and that some logins succeed.
import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';
import { API_URL, PASSWORD, summaryTo } from './common.js';

const successfulLogins = new Counter('logins_ok');
const rateLimited = new Counter('logins_rate_limited');

http.setResponseCallback(http.expectedStatuses(200, 429));

export const options = {
  vus: 20,
  duration: '30s',
  thresholds: {
    http_req_duration: ['p(95)<800'],
    http_req_failed: ['rate<0.01'], // anything other than 200/429
    logins_ok: ['count>=1'],
  },
};

export default function () {
  const email = `tourist${1 + (__VU % 3)}@tripcraft.test`;
  const response = http.post(`${API_URL}/api/auth/login`, JSON.stringify({ email, password: PASSWORD }), {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'POST /api/auth/login' },
  });
  if (response.status === 200) successfulLogins.add(1);
  if (response.status === 429) rateLimited.add(1);
  check(response, {
    '200 or 429': (r) => r.status === 200 || r.status === 429,
    'token when 200': (r) => r.status !== 200 || Boolean(r.json('accessToken')),
  });
}

export const handleSummary = summaryTo('auth-load');
