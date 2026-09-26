# Test evidence

Maps every row of PLAN.md section 11 to the tests that prove it, how to run them, the latest counts, and
where the screenshots for the report go. Counts are from the local run on 26 Sep 2026 on this branch (`docs/spec-compliance`). With Students B and C's
draft branches stacked on top, every suite is larger and the workflow e2e passes — see `docs/COMPLIANCE.md`.

## How to run each suite

| Suite | Command (from the repo root) | CI workflow |
|-------|------------------------------|-------------|
| Backend (unit, integration, database) | `cd backend && TEST_DATABASE_URL="Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=…" dotnet test` — without `TEST_DATABASE_URL` the database tests start a `postgres:16-alpine` container with Testcontainers (Docker needed; with colima also set `DOCKER_HOST=unix://$HOME/.colima/default/docker.sock TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE=/var/run/docker.sock`) | `backend-ci.yml` (postgres:16 service, TRX artifact) |
| Agent evaluation | `cd agents && .venv/bin/python -m pytest -q` | `agents-ci.yml` (`LLM_PROVIDER=fake`) |
| React | `cd web && npm test` (also `npm run lint`, `npm run build`) | `web-ci.yml` |
| Flutter | `cd mobile && flutter analyze && flutter test` | `mobile-ci.yml` |
| End to end | `cd tests/e2e && npm install && npx playwright install chromium && BASE_URL=… API_URL=… E2E_DATABASE_URL=… npx playwright test` (full stack running) | not in CI (needs the stack and a model) |
| Performance | `k6 run tests/perf/list-load.js`, `auth-load.js`, `agent-latency.js` (see `tests/perf/README.md`) | not in CI |

## Counts (latest local run)

| Layer | Component / folder | Tests | Result |
|-------|--------------------|------:|--------|
| Backend | `Tests/Trips` (Student A) | 86 | pass |
| Backend | `Tests/Workflows` (+ `External`: third-party wrappers incl. 429, base-URL overrides, agent client) | 51 + 24 | pass |
| Backend | `Tests/Quotations` (approval gate) | 21 | pass |
| Backend | `Tests/Identity` (auth, tokens, validators, admin audit log) | 21 | pass |
| Backend | `Tests/Shared/Database` (real PostgreSQL) | 18 | pass |
| Backend | `Tests/Common` (health, connection strings, timestamps, error handling, Swagger responses) | 14 | pass |
| Backend | `Tests/Resources` (Student B) | 0 | **not merged yet** |
| **Backend total** | | **235** | **235 passed** (PostgreSQL 16) |
| Agents | `agents/tests/golden` (7 cases) | 17 | pass |
| Agents | `tests/` unit (planner, itinerary, resources, validation, registry, LLM helper, auth) | 26 | pass |
| **Agents total** | | **43** | **43 passed** |
| React | `auth` 10, `features/trips` 13, `features/quotations` 7, `features/resources` 1 | 31 | **31 passed** |
| Flutter | `core` 21, `trips` 18, `quotations` 3, `resources` 3, `shared` 3 | 48 | **48 passed** |
| E2E | `roles.spec.ts` (4), `workflow.spec.ts`, `safe-failure.spec.ts` | 6 | **4 passed, 2 failed** — see below |
| Performance | `list-load`, `auth-load`, `agent-latency` | 3 | list-load and auth-load pass; agent-latency fails its PendingApproval check — see below |

## PLAN.md section 11, row by row

| Row | Required | Where | Status |
|-----|----------|-------|--------|
| Backend unit | Quotation calculation (3) | server recompute in `Workflows/ProposalValidatorTests` (5 price cases), agent formula in `agents/tests/test_validation.py` | done for the proposal check; **Student C's `QuotationCalculator` does not exist yet** |
| | Availability overlap logic (3) | `Workflows/ProposalValidatorTests` (guide/vehicle overlap facts) | **Student B's availability service does not exist yet** |
| | Itinerary rule validators (3) | `Trips/TripPlanningRulesTests` (13), `Workflows/ProposalValidatorTests` (stops, rooms, seats, language) | done |
| | DTO validators | `Trips/TripsValidatorTests`, `Identity/IdentityValidatorsTests`, `Workflows/WorkflowValidatorsTests`, `Quotations/QuotationDecisionValidatorsTests` | done |
| Backend integration | Auth: login OK / wrong password 401 / tourist on approve 403 | `Identity/AuthEndpointsTests`, `Identity/TokenValidationTests` (expired, wrong key, tampered → 401), `Quotations/QuotationApprovalTests` (tourist and admin → 403) | done |
| | One CRUD / create→list→paging flow per component | `Trips/TripsCreateAndListFlowTests`, `Trips/AttractionsEndpointsTests`, `Workflows/WorkflowsListFlowTests` | done for A and Workflows; Resources (B) and Quotations list (C) not built |
| | Status codes 201/204/400/403/404/409 | `Trips/TripRequestsEndpointsTests`, `Trips/AttractionsEndpointsTests` (201/204/400/403/404/409), `Workflows/WorkflowStatusCodeTests` (201/400/401/403/404/409), `Quotations/QuotationStatusCodeTests` (200/401/403/404/409) | done |
| | Approve transaction rolls back on conflict | `Quotations/QuotationApprovalTests` (InMemory), `Quotations/QuotationApprovalServiceTests` (unit), `Shared/Database/ApprovalTransactionPostgresTests` (real PostgreSQL transaction) | done |
| Database | Migrations apply from empty | `Shared/Database/MigrationsTests` (all 4 migrations, every table, column types, model = migrations) | done |
| | Unique and check constraints reject bad rows | `Shared/Database/ConstraintTests` (8 cases) | done |
| | Hold overlap rejected | — | **`resource_holds` belongs to Student B and does not exist yet** |
| | Approval rollback leaves zero holds | `Shared/Database/ApprovalTransactionPostgresTests` | done (holds via the transactional fake until B merges) |
| React | Login validation, protected route, trips list, error on 500, approve calls API | `auth/__tests__/*`, `features/trips/__tests__/TripsListPage.test.tsx`, `features/quotations/__tests__/ApprovalReviewPage.test.tsx` | done |
| Flutter | Trip form validation, navigation to itinerary, API client mock, secure storage read | `test/trips/new_trip_form_test.dart`, `test/trips/navigation_test.dart`, `test/core/api_client_test.dart`, `test/trips/trip_detail_test.dart`, `test/core/secure_storage_test.dart` | done |
| End to end | Submit → agents → approve → confirmed | `tests/e2e/workflow.spec.ts`, `tests/e2e/safe-failure.spec.ts` | written; **fails until Students B and C merge** (below) |
| Performance | 50 users, 60 s, p95; agent latency over 5 runs | `tests/perf/list-load.js`, `auth-load.js`, `agent-latency.js` | list-load and auth-load pass; agent latency measured, PendingApproval not reachable yet |
| Agent evaluation | Golden, over-budget, injection, tool failure, schema violation, approval enforced | `agents/tests/golden/*` — see `agents/tests/EVALUATION.md` | done (no LLM-as-judge) |

## Results that are not green, and why

**E2E (local stack: PostgreSQL 16, API, agent service on Ollama `llama3.1:8b`, React on port 5199).**
Both specs reach the agents for real: the Planner and Itinerary agents succeed through the real internal API
(attractions from the database, distance from `city_distances`, weather skipped without a key), then the
Resource agent's first tool, `GET /api/internal/availability/guides`, returns **503** from the Resource
Management placeholder, and the workflow ends `FailedSafely` as designed:

```
Error: Agents failed safely: resources: GET /api/internal/availability/guides returned 503
Expected: "PendingApproval"   Received: "FailedSafely"
```

They will pass once Student B registers the real `IResourceCatalog` / `IResourceHoldService` and Student C the
`IQuotationStore` (`backend/src/TripCraft.Infrastructure/Workflows/WorkflowsSetup.cs`). The `resource_holds`
count also needs B's table. The specs are not skipped.

**Performance (same local stack, MacBook, Apple Silicon).**

| Script | Result |
|--------|--------|
| `list-load.js` 50 VUs × 60 s, no think time | 831,741 requests (13,850/s), p95 **7.7 ms**, 0.00 % errors — both thresholds pass |
| `auth-load.js` 20 VUs × 30 s | exactly 5 logins succeeded, 2,135,392 answered 429 by the 5/min/IP limiter (by design), p95 0.6 ms, 0.00 % real errors — thresholds pass |
| `agent-latency.js` 5 sequential runs | 5/5 ended `FailedSafely` at the resources step (same 503 as above) after avg **40.6 s** (min 38.1, max 44.4) of real Planner + Itinerary work; the PendingApproval check fails until B/C merge |

Summaries: `docs/evidence/perf/*-summary.json`.

## Where screenshots go

| Folder | What to put there |
|--------|-------------------|
| `docs/evidence/perf/` | `*-summary.json` (written by k6) and a screenshot of each k6 terminal summary |
| `docs/evidence/e2e/` | Playwright screenshots (`approval-review.png`, `approved-toast.png`, `revision-requested.png`); `results/` and `report/` are generated and not committed |
| `docs/evidence/screenshots/` | Passing runs of each suite for the testing report: `dotnet test`, `pytest`, `npm test`, `flutter test`, and the four GitHub Actions runs |

## CI

`.github/workflows/backend-ci.yml`, `web-ci.yml`, `mobile-ci.yml`, `agents-ci.yml` run on push and pull
request to `main`, with `concurrency: cancel-in-progress`. The repository has no GitHub remote yet, so they
have not run on GitHub; every step of each workflow was run locally with the same commands and passed
(backend in Release against PostgreSQL 16; agents in a fresh Python 3.11 virtualenv with `LLM_PROVIDER=fake`).
