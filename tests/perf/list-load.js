// PLAN.md section 11, Performance: 50 concurrent users on GET /api/trip-requests for 60 s.
import http from 'k6/http';
import { check } from 'k6';
import { API_URL, authHeaders, login, summaryTo } from './common.js';

export const options = {
  vus: 50,
  duration: '60s',
  thresholds: {
    http_req_duration: ['p(95)<800'], // ms
    http_req_failed: ['rate<0.01'],
  },
};

// One login for the whole run (the login endpoint allows 5 attempts per minute per IP).
export function setup() {
  return { token: login(__ENV.EMAIL || 'manager1@tripcraft.test') };
}

export default function ({ token }) {
  const page = 1 + Math.floor(Math.random() * 3);
  const response = http.get(`${API_URL}/api/trip-requests?page=${page}&pageSize=20&sort=-createdAt`, {
    ...authHeaders(token),
    tags: { name: 'GET /api/trip-requests' },
  });
  check(response, {
    'status 200': (r) => r.status === 200,
    'paged body': (r) => r.status === 200 && Array.isArray(r.json('items')),
  });
}

export const handleSummary = summaryTo('list-load');
