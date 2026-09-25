# Performance tests (k6)

PLAN.md section 11, row "Performance". Run **from the repository root** so the summaries land in
`docs/evidence/perf/`. The stack must be running (see the root README for the startup order).

```bash
brew install k6                                   # or https://k6.io/docs/get-started/installation/
k6 run tests/perf/list-load.js                    # 50 VUs, 60 s, GET /api/trip-requests
k6 run tests/perf/auth-load.js                    # 20 VUs, 30 s, POST /api/auth/login
k6 run tests/perf/agent-latency.js                # 5 sequential workflow runs (agent service + LLM needed)
```

| Variable | Default | Meaning |
|----------|---------|---------|
| `API_URL` | `http://localhost:5080` | API under test (use the Render URL for the deployed system) |
| `EMAIL` | manager1 / tourist1 | Seeded account used by the script (password `Passw0rd!`) |
| `SUMMARY_DIR` | `docs/evidence/perf` | Where `<script>-summary.json` is written |

| Script | Thresholds (fail the run if missed) |
|--------|-------------------------------------|
| `list-load.js` | p95 `http_req_duration` < 800 ms, error rate < 1 % |
| `auth-load.js` | p95 < 800 ms, no errors other than 200/429, at least one successful login. The API limits logins to 5 per minute per IP (PLAN.md section 10), so from one machine most answers are 429 **by design** — the script checks the limiter stays fast. |
| `agent-latency.js` | every run reaches `PendingApproval`; reports `time_to_pending_approval` (avg/p95) and the final status of each run |

**Evidence for the report:** keep the generated `docs/evidence/perf/*-summary.json` and put a screenshot of each
terminal summary in `docs/evidence/perf/` (e.g. `list-load.png`).
