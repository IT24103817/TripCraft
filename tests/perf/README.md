# Performance tests (k6)

PLAN.md section 11, row "Performance". Run **from the repository root** so the summaries land in
`docs/evidence/perf/`. The stack must be running (see the root README for the startup order).

```bash
brew install k6                                   # or https://k6.io/docs/get-started/installation/
k6 run tests/perf/list-load.js                    # 50 VUs, 60 s, GET /api/trip-requests
k6 run tests/perf/auth-load.js                    # 20 VUs, 30 s, POST /api/auth/login
k6 run tests/perf/db-response.js                  # 30 VUs, 30 s, database round trip via GET /health
k6 run tests/perf/agent-latency.js                # 5 sequential workflow runs (agent service + LLM needed)
k6 run -e PAUSE_SECONDS=120 tests/perf/agent-latency.js  # paced: one trip every ~2.5 min (hosted free-tier LLMs)
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
| `db-response.js` | p95 `db_latency_ms` (one database round trip, from `/health`) < 100 ms, error rate < 1 % |
| `agent-latency.js` | every run's quotation is sent to the client (workflow `Approved`, v1.1); reports `time_to_quotation_sent` (avg/p95) and the final status of each run |

**Evidence for the report:** keep the generated `docs/evidence/perf/*-summary.json` and put a screenshot of each
terminal summary in `docs/evidence/perf/` (e.g. `list-load.png`).
