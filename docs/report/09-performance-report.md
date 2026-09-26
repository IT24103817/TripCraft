# 9. Performance report

TODO: 3–5 pages with the k6 terminal screenshots and graphs. Scripts and how to run: `tests/perf/README.md`;
summaries: `docs/evidence/perf/*-summary.json`.

Environment: local stack on one Apple Silicon MacBook (API via `dotnet run`, Production environment, PostgreSQL 16, agent service with
Ollama llama3.1:8b), 26 Sep 2026. TODO: repeat against the deployed Render + Neon stack and compare.

| Script | Load | Result | Thresholds |
|--------|------|--------|------------|
| `list-load.js` | 50 VUs × 60 s, `GET /api/trip-requests`, no think time | 831,741 requests (13,850/s), p95 7.7 ms, avg 3.6 ms, max 217.5 ms, 0.00 % errors | p95 < 800 ms ✓, errors < 1 % ✓ |
| `auth-load.js` | 20 VUs × 30 s, `POST /api/auth/login` | 5 logins 200 (the limit), 2,135,392 × 429 from the rate limiter, p95 0.6 ms, 0.00 % other errors | p95 < 800 ms ✓, errors < 1 % ✓, ≥ 1 login ✓ |
| `agent-latency.js` | 5 sequential workflow runs | all 5 ended FailedSafely at the Resource agent after avg 40.6 s (min 38.1, max 44.4) | reached PendingApproval ✗ (until Students B and C merge) |

## Discussion

TODO: interpret — the trip list is far below the threshold on local hardware; the login limiter answers in well
under a millisecond, so brute force is cheap to refuse; agent latency is dominated by the local 8B model
(Groq would be faster, ADR-006); expected effect of Render's cold start (~50 s) and Neon's suspend.
