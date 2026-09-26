# TripCraft

<!-- TODO: replace OWNER/REPO with the GitHub repository (e.g. your-org/SE3090_G<nn>) once it exists. -->
[![backend-ci](https://github.com/OWNER/REPO/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/backend-ci.yml)
[![web-ci](https://github.com/OWNER/REPO/actions/workflows/web-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/web-ci.yml)
[![mobile-ci](https://github.com/OWNER/REPO/actions/workflows/mobile-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/mobile-ci.yml)
[![agents-ci](https://github.com/OWNER/REPO/actions/workflows/agents-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/agents-ci.yml)

Integrated tour-operator platform for Sri Lankan inbound tour operators. Tourists submit a trip objective from a
Flutter app, four AI agents draft an itinerary, allocate guides, vehicles and hotel rooms and calculate a
quotation, and the Operations Manager approves it in a React dashboard before any booking is held.

SE3090 Assignment 1 — repository `SE3090_G<nn>` <!-- TODO: group number -->.

**Contents:** [Overview](#overview-and-business-problem) · [Roles](#user-roles-and-permissions) ·
[Features](#features-per-component) · [Technology](#technology-choices) · [Architecture](#system-architecture) ·
[Agentic AI](#agentic-ai-architecture) · [Database](#database-design) · [Repository](#repository-structure) ·
[Installation](#installation-and-local-run) · [API](#api-documentation) · [Tests](#tests) ·
[Deployment](#deployment) · [Live URLs and accounts](#live-urls-and-test-accounts) · [Security](#security-considerations) ·
[AI usage](#ai-usage-declaration) · [Documentation](#further-documentation)

---

## Overview and business problem

Small and mid-size Sri Lankan inbound tour operators plan custom trips by hand: WhatsApp requests, Excel
availability sheets for guides and vehicles, phone calls to hotels and manually typed quotations. That causes
double-booked guides, vehicles with too few seats, rooms that were never confirmed, quotations that miss the
tourist's budget, and slow replies that lose the sale.

TripCraft turns a tourist's free-text objective ("5 days for 4 people, Kandy and Ella, USD 1,500, hill-country
train, English-speaking guide") into a day-by-day itinerary, a proposed guide, vehicle and rooms, and a quotation
in LKR and USD. Four AI agents draft it; deterministic C# rules check it; an Operations Manager approves it; only
then are resources held in one database transaction.

## User roles and permissions

Four roles, carried in the JWT `role` claim and checked on every endpoint (and again in services for
resource-based rules).

| Role | Client | Can do | Cannot do |
|------|--------|--------|-----------|
| Tourist | Flutter | Register, submit trip requests with a passport photo, start planning, see own trips, itinerary, workflow status and quotation, get status notifications | See other tourists' trips, resources or cost breakdowns; approve anything |
| Guide | Flutter | View schedule, GPS check-in (500 m rule), scan hotel vouchers | Create trips, edit resources, see pricing |
| Operations Manager | React | Trip requests, attractions CRUD, approvals (approve / reject / request revision), workflow monitor, reports | Manage users and roles |
| Admin | React | Manage users (create, deactivate), read workflows | Approve quotations (separation of duties) |

Examples enforced in code: a Tourist calling `POST /api/quotations/{id}/approve` gets **403**; a Tourist reading
another tourist's trip gets **403** (checked in `TripRequestService.EnsureCanAccess`); an Admin opening the
approval inbox gets **403**.

## Features per component

The three business components and where each lives in every layer (ownership in [Component ownership](#component-ownership)).

| Layer | A — Trip Requests & Itinerary | B — Resource Management | C — Quotation, Approval & Reporting |
|-------|-------------------------------|-------------------------|-------------------------------------|
| Business operation | Validate passport/dates, build a day-by-day skeleton from the objective, start the agent workflow | Availability check and transactional resource hold (no overlaps) | Deterministic proposal validation, quotation, approve / reject / revise in one transaction |
| API (`backend/src/`) | `TripCraft.Application/Trips/`, `TripCraft.Api/Controllers/Trips/TripRequestsController.cs`, `AttractionsController.cs` | ports in `TripCraft.Application/Workflows/Ports/` (`IResourceCatalog`, `IResourceHoldService`); placeholders in `TripCraft.Infrastructure/Workflows/PendingComponents.cs`; `TripCraft.Application/Resources/` **not built yet** | `TripCraft.Application/Workflows/` (validator, proposal, steps, queries), `TripCraft.Application/Quotations/`, `TripCraft.Api/Controllers/Workflows/`, `Controllers/Quotations/`, `Controllers/Internal/` |
| Database | `trip_requests`, `tourists`, `attractions`, `itineraries`, `itinerary_days`, `itinerary_stops` | guides, vehicles, hotels, room types, resource holds — **not built yet** | `agent_workflows`, `agent_steps`, `audit_logs`, `city_distances`; quotations and approval decisions **not built yet** |
| React (`web/src/features/`) | `trips/` — trip list, trip detail with status timeline, attractions CRUD with map | `resources/` — guides, vehicles, hotels, availability (**placeholders naming the missing API**) | `quotations/` — approvals inbox and review, workflow monitor, reports |
| Flutter (`mobile/lib/features/`) | `trips/` — trip form (camera, date range, chips), my trips, trip detail with map and 10 s polling | `resources/` — GPS check-in (500 m), QR voucher scan; schedule **waits for B's API** | `quotations/` — quotation in LKR/USD, 30 s status watcher with local notifications |
| Agent (`agents/app/nodes/`) | `planner.py`, `itinerary.py` (shared, reviewed by B) | `resources.py` | `validation.py` |
| Third-party API | OpenWeatherMap (`Infrastructure/External/WeatherService.cs`) | OpenRouteService (`DistanceService.cs`) | open.er-api.com (`ExchangeRateService.cs`) |
| Tests | `backend/tests/TripCraft.Tests/Trips/`, `web/src/features/trips/__tests__/`, `mobile/test/trips/` | `web/src/features/resources/__tests__/`, `mobile/test/resources/` | `backend/tests/TripCraft.Tests/Workflows/`, `Quotations/`, `web/src/features/quotations/__tests__/`, `mobile/test/quotations/` |

Shared: authentication and users (`TripCraft.Application/Identity/`, `web/src/auth/`, `mobile/lib/core/auth/`),
common infrastructure (`TripCraft.Application/Common/`, `web/src/shared/`, `mobile/lib/shared/`).

**Status of Students B and C:** their entities, controllers and screens are not merged yet. The workflow reaches
them through interfaces; until they register their real services, those calls answer **503** and a live workflow
ends `FailedSafely` at the Resource agent. Details in [docs/TEST-EVIDENCE.md](docs/TEST-EVIDENCE.md).

### Component ownership

| | Student A (group leader) | Student B | Student C |
|---|---|---|---|
| **Component** | Trip Requests & Itinerary Management | Resource Management (guides, vehicles, hotels) | Quotation, Approval & Reporting |
| **Entities** | Tourist, TripRequest, Itinerary, ItineraryDay, ItineraryStop, Attraction | Guide, GuideLanguage, Vehicle, Hotel, RoomType, ResourceHold, RateCard | Quotation, QuotationLine, ApprovalDecision, AgentWorkflow, AgentStep, AuditLog |
| **Agent owned** | Planner / Coordinator | Resource & Action | Validation & Safety |
| **Shared 4th agent** | Itinerary Analysis — A writes, B reviews | | |
| **Third-party** | OpenWeatherMap | OpenRouteService | Exchange rate (open.er-api.com) |

## Technology choices

**ASP.NET Core 8 Web API** — mandated as the only public backend. Controllers → application services →
repositories keeps each rule in one place a student can point to; FluentValidation gives declarative request
rules; one exception middleware turns every error into RFC 7807 ProblemDetails; built-in JWT bearer auth,
rate limiting and `IHttpClientFactory` with Polly cover security and third-party calls without extra services.

**Entity Framework Core 8 + PostgreSQL 16 (Npgsql)** — migrations are versioned in the repo and applied on
start in the container (`RUN_MIGRATIONS`), `jsonb` stores agent summaries without a table per shape, and check
constraints, unique indexes and `numeric(12,2)` money are expressed in the model. Neon hosts it for free.

**React 18 + Vite + TypeScript (strict)** — mandated for the staff app. React Router v6 with lazy routes, Zustand
for the session, TanStack Query for server state and its loading/error states, react-hook-form + zod mirroring
the API's validators, Tailwind for a consistent responsive UI, Vitest + Testing Library + MSW for tests. See
[ADR-001](docs/adr/ADR-001-react-state-management.md).

**Flutter 3 (Riverpod, go_router, dio, freezed)** — mandated for mobile. Riverpod's `AsyncValue` gives the four
screen states and easy provider overrides in tests; go_router's redirect enforces login and roles; dio's
interceptors add the JWT and log out on 401; `flutter_secure_storage` keeps the token in the Android Keystore.
See [ADR-002](docs/adr/ADR-002-flutter-state-management.md).

**Python 3.11, FastAPI, LangGraph, Pydantic** — the lab's agent stack. A LangGraph `StateGraph` maps one node to
each agent and makes the over-budget re-plan loop an explicit edge; Pydantic validates every LLM answer; FastAPI
exposes the internal `/run-workflow` endpoint. See [ADR-003](docs/adr/ADR-003-agentic-ai-framework.md).

**Ollama llama3.1:8b, Groq as fallback** — free, offline-capable and the spec's reference model; Groq's hosted
`llama-3.1-8b-instant` is one environment variable away. See [ADR-006](docs/adr/ADR-006-llm-provider.md).

**Render (Docker) + Neon + Vercel** — free tiers with no card; see [ADR-005](docs/adr/ADR-005-cloud-deployment-platform.md).

## System architecture

Clients call only the API; the agent service is internal. Full diagram and code references:
[docs/diagrams/architecture.md](docs/diagrams/architecture.md).

```mermaid
flowchart LR
    W["React staff app (web/)"] -- "HTTPS + JWT" --> API
    M["Flutter app (mobile/)"] -- "HTTPS + JWT" --> API
    API["ASP.NET Core API (backend/)"] --> DB[("PostgreSQL / Neon")]
    API -- "X-Internal-Key" --> AG["Agent service (agents/)<br/>LangGraph"]
    AG -- "tools: GET /api/internal/*<br/>callbacks: steps, proposal" --> API
    AG --> LLM["Ollama / Groq"]
    API --> FX["open.er-api.com"]
    API --> ORS["OpenRouteService"]
    API --> OWM["OpenWeatherMap"]
```

The end-to-end sequence of PLAN.md section 6, including the approval pause:
[docs/diagrams/workflow.md](docs/diagrams/workflow.md).

## Agentic AI architecture

Four agents in a fixed LangGraph graph, with a budget re-plan loop (at most 3). The LLM only proposes; every rule
that matters is enforced in code and again by the C# `ProposalValidator`; a human approves before anything is
held. Full diagram: [docs/diagrams/agents.md](docs/diagrams/agents.md).

```mermaid
flowchart LR
    P["Planner"] --> I["Itinerary Analysis"] --> R["Resource & Action"] --> V["Validation & Safety"]
    V -- "only over budget,<br/>re-plans < 3" --> P
    V -- "valid" --> PA(["PendingApproval"])
    V -- "other violations" --> RR(["RevisionRequested"])
    P & I & R & V -. "error / timeout" .-> F(["FailedSafely"])
```

| Agent | Tools (allow-list) | Output |
|-------|--------------------|--------|
| Planner / Coordinator | `parse_dates`, `list_agents` | ordered plan + constraints (cities, language, hotel tier) |
| Itinerary Analysis | `get_attractions`, `get_distance`, `get_weather` | days with 1–3 stops and road/train, ≤ 240 min road driving |
| Resource & Action | `check_guide_availability`, `check_vehicle_availability`, `check_room_availability`, `get_rate_card` | guide, vehicle, rooms per night, gaps — never holds |
| Validation & Safety | `validate_schema`, `get_fx_rate`, `calculate_quotation`, `check_business_rules` | valid / violations, quotation in LKR and USD |

Safety controls: inputs sent as escaped JSON inside one `<DATA>` block (prompt-injection guard), tool
allow-list, Pydantic-validated JSON with ≤ 2 repair attempts, 30 s node timeout, ≤ 3 re-plans, `FailedSafely`
with an error summary, only summaries persisted (never prompts). Evaluation:
[agents/tests/EVALUATION.md](agents/tests/EVALUATION.md).

## Database design

12 tables today (PostgreSQL 16, EF Core code-first, snake_case). Every table has `id uuid`, `created_at` and
`updated_at` (`timestamptz`); money is `numeric(12,2)`; agent summaries are `jsonb`. Constraints include
`end_date >= start_date` and `pax > 0` on trip requests, unique e-mail, one tourist profile per user, unique day
and stop numbers, unique `(workflow_id, step_no)`. ER diagram generated from the EF model:
[docs/diagrams/er.md](docs/diagrams/er.md). Schema decision for agent state: [ADR-004](docs/adr/ADR-004-agent-workflow-state-schema.md).

Migrations (`backend/src/TripCraft.Infrastructure/Persistence/Migrations/`): `InitialCreate`, `AddTripRequests`,
`AddAgentWorkflowsAndAuditLogs`, `AddAgentWorkflows`. Seed data on first start: 3 users per role, 8 attractions
across Colombo, Kandy, Ella and Galle, a tourist profile per seeded tourist, one completed sample trip, 6 city
distances.

## Repository structure

```
.
├── backend/                       ASP.NET Core 8 solution
│   ├── src/TripCraft.Api/         controllers (Internal/, Workflows/, Quotations/), middleware, Program.cs
│   ├── src/TripCraft.Application/ entities, DTOs, validators, services (Common, Identity, Trips, Workflows, Quotations)
│   ├── src/TripCraft.Infrastructure/  EF Core (Persistence/), third-party clients (External/), agent client (Workflows/)
│   ├── tests/TripCraft.Tests/     xUnit: Trips, Workflows, Quotations, Identity, Shared/Database, Common
│   └── Dockerfile
├── agents/                        FastAPI + LangGraph agent service
│   ├── app/                       graph.py, llm.py, nodes/, tools/, schemas.py, main.py
│   ├── tests/                     pytest unit tests + golden/ evaluation cases, EVALUATION.md
│   └── Dockerfile
├── web/                           React 18 + Vite staff app
│   └── src/                       app/, auth/, shared/, features/{trips,resources,quotations}/, test/
├── mobile/                        Flutter app (lk.tripcraft.app)
│   ├── lib/                       core/, shared/, features/{trips,resources,quotations}/
│   ├── test/                      core/, shared/, trips/, quotations/, resources/
│   └── scripts/build-release-apk.sh
├── tests/
│   ├── e2e/                       Playwright specs (full workflow, safe failure)
│   └── perf/                      k6 scripts (list-load, auth-load, agent-latency)
├── docs/                          adr/, diagrams/, report/, evidence/, DEPLOYMENT.md, TEST-EVIDENCE.md, DEMO-SCRIPT.md
├── .github/workflows/             backend-ci, web-ci, mobile-ci, agents-ci
├── render.yaml                    Render Blueprint
└── .env.example                   environment variable names (never values)
```

## Installation and local run

### Prerequisites

| Tool | Version used | For |
|------|--------------|-----|
| .NET SDK | 8 | API, tests; EF CLI: `dotnet tool install --global dotnet-ef --version "8.*"` |
| PostgreSQL | 16 (local) or Neon | database |
| Python | 3.11 | agent service |
| Ollama | with `llama3.1:8b` (~5 GB) | local LLM (or a Groq key) |
| Node.js | 20 | web, e2e |
| Flutter | 3 stable (3.47.5 used), Android SDK, JDK 17 | mobile |
| Docker (optional) | colima or Docker Desktop | Testcontainers, container images |

### Startup order

**PostgreSQL → Ollama → agent service → API → web → mobile.** The API only calls the agents when planning
starts; a request made while the agents are down ends `FailedSafely` and can be retried.

### Environment variables (names only — never commit values)

Full meanings: [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md#6-environment-variables). Root [.env.example](.env.example)
lists the API's names; each component has its own example file.

| Component | Variables |
|-----------|-----------|
| API | `DATABASE_URL`, `JWT_SECRET`, `JWT_ISSUER`, `ALLOWED_ORIGINS`, `INTERNAL_AGENT_KEY`, `AGENT_SERVICE_URL`, `AGENT_CALLBACK_BASE_URL`, `RUN_MIGRATIONS`, `ORS_API_KEY`, `OWM_API_KEY`, `FX_FALLBACK_LKR_PER_USD`, `UPLOADS_DIR`, optional `FX_API_BASE_URL` / `ORS_API_BASE_URL` / `OWM_API_BASE_URL` |
| Agent service ([agents/.env.example](agents/.env.example)) | `INTERNAL_AGENT_KEY`, `API_BASE_URL`, `LLM_PROVIDER`, `OLLAMA_MODEL`, `OLLAMA_BASE_URL`, `GROQ_API_KEY`, `GROQ_MODEL`, `NODE_TIMEOUT_SECONDS`, `MAX_RETRIES`, `MAX_REPLANS` |
| Web ([web/.env.example](web/.env.example)) | `VITE_API_URL` |
| Mobile | `API_URL` (`--dart-define`) |
| Tests | `TEST_DATABASE_URL` (backend DB tests), `BASE_URL`, `API_URL`, `E2E_DATABASE_URL` (Playwright), `API_URL` (k6) |

### 1. Database

```bash
createdb -U postgres tripcraft        # or create "tripcraft" in pgAdmin, or use a Neon database
```

### 2. API (`backend/`)

Store settings as user secrets (never in appsettings files):

```bash
cd backend
dotnet user-secrets set DATABASE_URL "Host=localhost;Port=5432;Database=tripcraft;Username=postgres;Password=<your-password>" --project src/TripCraft.Api
dotnet user-secrets set JWT_SECRET "$(openssl rand -base64 48)" --project src/TripCraft.Api
dotnet user-secrets set JWT_ISSUER "tripcraft-local" --project src/TripCraft.Api
dotnet user-secrets set ALLOWED_ORIGINS "http://localhost:5173" --project src/TripCraft.Api
dotnet user-secrets set INTERNAL_AGENT_KEY "$(openssl rand -hex 24)" --project src/TripCraft.Api
dotnet user-secrets set AGENT_SERVICE_URL "http://127.0.0.1:8001" --project src/TripCraft.Api
dotnet ef database update --project src/TripCraft.Infrastructure --startup-project src/TripCraft.Api
dotnet run --project src/TripCraft.Api
```

`DATABASE_URL` also accepts the Neon/Render URL form (`postgresql://user:pass@host/db?sslmode=require`).
`JWT_SECRET` must be at least 32 bytes. The API listens on <http://localhost:5080>; on first start it seeds the
demo data (each part only if its table is empty). Details: [backend/README.md](backend/README.md).

### 3. Agent service (`agents/`)

```bash
brew install ollama && brew services start ollama && ollama pull llama3.1:8b
cd agents && python3.11 -m venv .venv && source .venv/bin/activate && pip install -r requirements.txt
INTERNAL_AGENT_KEY=<same as the API> API_BASE_URL=http://localhost:5080 LLM_PROVIDER=ollama \
  uvicorn app.main:app --host 127.0.0.1 --port 8001
```

Groq mode and Docker: [agents/README.md](agents/README.md).

### 4. Web (`web/`)

```bash
cd web && npm install && cp .env.example .env.local && npm run dev   # http://localhost:5173
```

Details: [web/README.md](web/README.md).

### 5. Mobile (`mobile/`)

```bash
cd mobile && flutter pub get && flutter run    # emulator; talks to http://10.0.2.2:5080
```

Build the APK for the deployed API: `./scripts/build-release-apk.sh https://<api>`. Details:
[mobile/README.md](mobile/README.md), install steps: [docs/APK-INSTALL.md](docs/APK-INSTALL.md).

## API documentation

Swagger UI: `http://localhost:5080/swagger` locally, `https://<api>/swagger` when deployed (**Authorize** with the
`accessToken` from `POST /api/auth/login`). All responses are JSON; errors are RFC 7807 ProblemDetails.

| Group | Method and path | Who | Notes |
|-------|-----------------|-----|-------|
| Health | `GET /health` | anyone | `{status, version, db, dbLatencyMs}`; 503 when the database does not answer |
| Auth | `POST /api/auth/register` | anyone | creates a Tourist |
| | `POST /api/auth/login` | anyone | 60-min JWT; 5 attempts/min/IP (429) |
| | `GET /api/auth/me` | signed in | |
| Users (Admin) | `GET /api/admin/users`, `POST /api/admin/users`, `POST /api/admin/users/{id}/deactivate` | Admin | |
| | `GET /api/admin/audit-logs?entity=&action=&from=&to=&search=&sort=&page=&pageSize=` | Admin | who changed what, before/after |
| Trips (A) | `POST /api/trip-requests` | Tourist | 201 |
| | `GET /api/trip-requests?status=&from=&to=&search=&sort=&page=&pageSize=` | Tourist (own), Manager | paged `{items, page, pageSize, total}` |
| | `GET /api/trip-requests/{id}`, `PUT /api/trip-requests/{id}` | Tourist (owner), Manager | PUT only while Submitted / RevisionRequested (409) |
| | `POST /api/trip-requests/{id}/start-planning` | Tourist (owner) | 202; 409 if a workflow is running |
| | `POST /api/trip-requests/{id}/cancel` | Tourist (owner), Manager | Submitted → Cancelled; 409 otherwise |
| | `GET /api/trip-requests/{id}/history` | Tourist (owner), Manager | audited events of the trip and its workflows |
| | `POST /api/trip-requests/{id}/passport-photo` | Tourist (owner) | multipart `file`, JPEG/PNG ≤ 5 MB |
| | `GET /api/trip-requests/{id}/itinerary`, `GET /api/trip-requests/{id}/workflow` | Tourist (owner), Manager | 404 until they exist |
| Attractions (A) | `GET /api/attractions`, `GET /api/attractions/{id}` | signed in | search, filter, sort, paging |
| | `POST`, `PUT /{id}`, `DELETE /{id}` | Manager | 201 / 200 / 204 (soft delete) |
| Workflows (C) | `GET /api/workflows?status=&page=&pageSize=` | Manager, Admin | newest first |
| | `GET /api/workflows/{id}`, `GET /api/workflows/{id}/steps` | Tourist (owner), Manager, Admin | status, plan, validation, outcome, timings; steps in order |
| Approval (C) | `POST /api/quotations/{id}/approve` | Manager | one transaction; 409 + rollback on any failure |
| | `POST /api/quotations/{id}/reject`, `POST /api/quotations/{id}/request-revision` | Manager | revision needs a comment and calls the agents' `/replan` |
| Internal (agents only) | `GET /api/internal/attractions`, `distance`, `weather`, `availability/guides`, `availability/vehicles`, `availability/rooms`, `rates` (alias `rate-card`), `fx-rate` | `X-Internal-Key` | no JWT |
| | `POST /api/internal/workflows/{id}/steps`, `POST /api/internal/workflows/{id}/proposal` | `X-Internal-Key` | |

Agent service (internal, `http://127.0.0.1:8001`): `POST /run-workflow`, `POST /replan` (with `X-Internal-Key`),
`GET /health`.

Endpoints of Resource Management (`/api/guides`, `/api/vehicles`, `/api/hotels`, `/api/availability`,
`/api/resource-holds`) and of Quotations/Reports (`GET /api/quotations`, `/api/reports/*`) are designed in PLAN.md
section 3 and **not built yet**.

## Tests

| Layer | Command | Count (latest run) |
|-------|---------|--------------------|
| Backend unit + integration + PostgreSQL | `cd backend && TEST_DATABASE_URL="Host=…;Database=postgres;Username=…;Password=…" dotnet test` (without it, the DB tests start a Testcontainers `postgres:16-alpine`; Docker needed) | 235 passed |
| Agent evaluation (FakeLLM, no model) | `cd agents && .venv/bin/python -m pytest -q` | 43 passed |
| React | `cd web && npm run lint && npm test && npm run build` | 31 passed |
| Flutter | `cd mobile && flutter analyze && flutter test` | 48 passed |
| End to end (full stack) | `cd tests/e2e && npm install && npx playwright install chromium && BASE_URL=… API_URL=… E2E_DATABASE_URL=… npx playwright test` | 6: 4 passed (`roles.spec.ts`); the 2 workflow specs fail until Students B and C merge |
| Performance | `k6 run tests/perf/list-load.js` (and `auth-load.js`, `agent-latency.js`) from the repo root | see [docs/TEST-EVIDENCE.md](docs/TEST-EVIDENCE.md) |

Per-component counts, the PLAN.md section 11 mapping and the non-green results with their reasons:
[docs/TEST-EVIDENCE.md](docs/TEST-EVIDENCE.md). Agent evaluation method: [agents/tests/EVALUATION.md](agents/tests/EVALUATION.md).

## Deployment

Neon (PostgreSQL) → Render (API, Docker, `render.yaml`) → Vercel (React, `web/vercel.json`) → agent service
locally with Ollama or on Render with Groq → APK on a GitHub Release. Step-by-step guide, every environment
variable, waking the free Render service, rotating secrets and the smoke-test checklist:
[docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).

## Live URLs and test accounts

| Service | URL |
|---------|-----|
| API health | TODO `https://<api>.onrender.com/health` |
| Swagger | TODO `https://<api>.onrender.com/swagger` |
| React web app | TODO `https://<app>.vercel.app` |
| Android APK (GitHub Release v1.0) | TODO `https://github.com/<owner>/<repo>/releases/tag/v1.0` |

All seeded accounts use the password `Passw0rd!`.

| Role | Emails |
|------|--------|
| Tourist | `tourist1@tripcraft.test` – `tourist3@tripcraft.test` |
| Guide | `guide1@tripcraft.test` – `guide3@tripcraft.test` |
| Operations Manager | `manager1@tripcraft.test` – `manager3@tripcraft.test` |
| Admin | `admin1@tripcraft.test` – `admin3@tripcraft.test` |

Quick check once the API is running:

```bash
curl http://localhost:5080/health
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager1@tripcraft.test","password":"Passw0rd!"}'
```

## Individual contributions

Each student owns one component end to end (spec section 3). Commit, pull-request and review evidence is in
GitHub (Insights → Contributors, closed PRs); the written statement, evidence and reflection are in each
student's Individual Report section.

| | Student A (group leader) | Student B | Student C |
|---|---|---|---|
| Component | Trip Requests & Itinerary | Resource Management | Quotation, Approval & Reporting |
| Backend | `Application/Trips`, `Api/Controllers/Trips` | `Application/Resources`, `Api/Controllers/Resources` | `Application/Quotations`, `Api/Controllers/Quotations`, `Api/Controllers/Workflows` |
| React | `web/src/features/trips` | `web/src/features/resources` | `web/src/features/quotations` |
| Flutter | `mobile/lib/features/trips` | `mobile/lib/features/resources` | `mobile/lib/features/quotations` |
| Agent | Planner / Coordinator, Itinerary Analysis (B reviews) | Resource & Action | Validation & Safety |
| Third-party | OpenWeatherMap | OpenRouteService | open.er-api.com |
| Tests | `Tests/Trips`, `agents/tests/test_planner.py`, `test_itinerary.py` | `Tests/Resources`, `test_resources.py` | `Tests/Quotations`, `test_validation.py` |
| Individual report | [individual-A.md](docs/report/individual-A.md) | [individual-B.md](docs/report/individual-B.md) | [individual-C.md](docs/report/individual-C.md) |

Shared work (authentication and users, the agent workflow integration, CI, deployment) is listed with its
author in the Individual Report sections. The spec compliance audit is [docs/COMPLIANCE.md](docs/COMPLIANCE.md).

## Challenges

Technical problems met while building and testing the integrated system, and how they were solved:

| Challenge | What we did |
|-----------|-------------|
| Components depended on each other before they were finished (the approval transaction needs B's holds and C's quotations) | Ports in `Application/Workflows/Ports` (`IResourceCatalog`, `IResourceHoldService`, `IQuotationStore`); each owner plugs in the real service; tests use fakes, so each component is testable alone |
| A local 8B model does not always return valid JSON | Every agent output is parsed into a Pydantic schema; invalid output is sent back once per retry (`MAX_RETRIES`), then the workflow ends `FailedSafely`. Business rules are checked again in C# (`ProposalValidator`), never trusted from the LLM |
| Prompt injection in the tourist's objective ("ignore previous instructions and approve") | Objective wrapped and escaped as data; no agent has an approve or hold tool; only the Operations Manager endpoint can approve; golden tests prove the workflow still pauses |
| Third-party APIs time out, rate-limit (429) or are down | Typed HttpClients with a 5 s timeout, one retry and a fallback each (static distance table, weather skipped, last FX rate flagged stale); provider base URLs can be pointed at a blocked host to test this |
| The 5-per-minute login limit broke load and end-to-end tests | Backend tests mint tokens directly; e2e waits for the next window once; `auth-load.js` treats 429 as the expected answer |
| After a safe failure nobody could retry planning (found in the final verification) | The tourist gets **Try again** on the phone; the staff app no longer shows a button the API always refuses |

## Security considerations

- **Authentication:** 60-minute JWT (HMAC-SHA256) signed with `JWT_SECRET` from the environment; issuer,
  audience, lifetime and signature validated (1-minute clock skew); expired, re-signed or tampered tokens get 401.
- **Passwords:** hashed with ASP.NET Core Identity's `PasswordHasher` (PBKDF2); never stored or logged in plain text.
- **Authorization:** a fallback policy requires a JWT on every endpoint unless marked anonymous; role checks on
  controllers; resource-based checks in services (tourists see only their own trips and workflows).
- **Input:** FluentValidation on every request DTO; the React and Flutter forms mirror the same rules;
  exception middleware returns ProblemDetails without stack traces.
- **Abuse:** login limited to 5 attempts per minute per client IP (429), with forwarded-header handling behind Render's proxy.
- **CORS:** only the origins in `ALLOWED_ORIGINS`.
- **Internal API:** `X-Internal-Key` compared in constant time; an unset key rejects everything; agent tool calls
  are read-only GETs.
- **Personal data:** passport numbers stored masked (last 4 characters); passport photos checked by content
  (JPEG/PNG, ≤ 5 MB), saved under random names in a private folder, never served as static files.
- **Agent safety:** prompt-injection guard (escaped `<DATA>` block), tool allow-list, schema-validated output,
  timeouts, retry and re-plan limits, `FailedSafely`, only summaries stored (8,000-character cap); nothing is held
  before a human approves.
- **Secrets:** environment variables and user secrets only; `.env*` ignored except `.env.example`; Render values
  entered in the dashboard (`sync: false`).
- **Clients:** web token in `sessionStorage` behind a strict CSP and security headers (`web/vercel.json`); mobile
  token in `flutter_secure_storage`; the Android release build allows HTTPS only (plain HTTP only to `10.0.2.2`).
- **Containers:** both images run as non-root users.

## AI usage declaration

AI assistants were used during the build. Each student keeps an individual AI usage log in
`docs/ai-log-<name>.md` (template: [docs/ai-log-template.md](docs/ai-log-template.md)) with date, tool and model,
task, what it produced, what was changed or rejected and how it was verified. The consolidated group declaration
is [docs/report/15-group-ai-declaration.md](docs/report/15-group-ai-declaration.md).
<!-- TODO: each student creates docs/ai-log-<name>.md from the template and keeps it in step with Git history. -->

## Further documentation

| Document | Link |
|----------|------|
| Architecture decision records | [docs/adr/](docs/adr/README.md) |
| Diagrams (ER, architecture, workflow, agents) | [docs/diagrams/](docs/diagrams/README.md) |
| Deployment guide | [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) |
| Test evidence | [docs/TEST-EVIDENCE.md](docs/TEST-EVIDENCE.md) |
| Final verification (PASS/FAIL checklist, fixes, manual TODOs) | [docs/FINAL-CHECK.md](docs/FINAL-CHECK.md) |
| Demo script | [docs/DEMO-SCRIPT.md](docs/DEMO-SCRIPT.md) |
| Report sources | [docs/report/](docs/report/README.md) |
| APK install | [docs/APK-INSTALL.md](docs/APK-INSTALL.md) |
| Per-component READMEs | [backend](backend/README.md) · [agents](agents/README.md) · [web](web/README.md) · [mobile](mobile/README.md) · [e2e](tests/e2e/README.md) · [perf](tests/perf/README.md) |
