# backend

ASP.NET Core 8 Web API (C#) — the only public backend. Entity Framework Core + PostgreSQL (Npgsql).
Setup, user secrets, migrations and test accounts are in the [root README](../README.md#run-the-backend-locally).

## Configuration (user secrets or environment variables — never in appsettings)

| Name | Required | Purpose |
|------|----------|---------|
| `DATABASE_URL` | yes | PostgreSQL connection (key=value or `postgresql://` URL) |
| `JWT_SECRET`, `JWT_ISSUER` | yes | Token signing (secret ≥ 32 bytes) |
| `ALLOWED_ORIGINS` | yes | CORS, comma-separated |
| `INTERNAL_AGENT_KEY` | yes | Shared secret with the agent service (`X-Internal-Key`), both directions |
| `AGENT_SERVICE_URL` | yes | e.g. `http://127.0.0.1:8001` — if missing, planning ends `FailedSafely` |
| `AGENT_CALLBACK_BASE_URL` | no | Base URL the agent service posts steps/proposals to; defaults to the agent's own `API_BASE_URL` |
| `ORS_API_KEY` | no | OpenRouteService; without it the seeded `city_distances` table is used |
| `OWM_API_KEY` | no | OpenWeatherMap; without it weather is skipped (advisory only) |
| `RUN_MIGRATIONS` | no | `true` applies EF migrations on start (Docker/Render); the seeder always runs and only fills empty tables |
| `UPLOADS_DIR` | no | Private folder for passport photos (default `uploads/` next to the app); never served as static files |
| `FX_FALLBACK_LKR_PER_USD` | no | Rate used (flagged stale) if open.er-api.com fails before any success; default 300 |

## Docker and deployment

`docker build -t tripcraft-api backend` builds the production image (non-root, port 8080). `GET /health` returns
`{status, version, db}` with a real database ping (`503` when the database does not answer). Render, Neon and
Vercel setup: [docs/DEPLOYMENT.md](../docs/DEPLOYMENT.md).

## Startup order

1. PostgreSQL (local or Neon), then `dotnet ef database update --project src/TripCraft.Infrastructure --startup-project src/TripCraft.Api`
2. Agent service: `cd agents && .venv/bin/uvicorn app.main:app --host 127.0.0.1 --port 8001` with the same `INTERNAL_AGENT_KEY` and `API_BASE_URL=http://localhost:5080` (see [agents/README.md](../agents/README.md))
3. API: `dotnet run --project src/TripCraft.Api` (seeds users, attractions and city distances on first start)
4. React / Flutter clients

## Agent workflow (PLAN.md sections 5–6)

```
Tourist  POST /api/trip-requests/{id}/start-planning ──► API creates agent_workflows (Planning) ──► agent POST /run-workflow
agent    GET  /api/internal/... tools                 ◄── internal API (X-Internal-Key)
agent    POST /api/internal/workflows/{id}/steps       ──► agent_steps row per agent
agent    POST /api/internal/workflows/{id}/proposal    ──► ProposalValidator ──► PendingApproval | RevisionRequested | FailedSafely
Manager  POST /api/quotations/{id}/approve             ──► one transaction: holds, quotation, trip Confirmed, workflow Completed, decision, audit
```

`ProposalValidator` (Application/Workflows) is a pure class. **Hard** violations (unknown attraction/guide/
vehicle/hotel/room type, overlapping guide or vehicle hold, rooms < pax on a night, seats < pax, guide language,
day with 0 or > 3 stops, quotation total off by more than 1 LKR from the server recomputation, incomplete JSON)
end the workflow `FailedSafely`. The only **Soft** violation, `OVER_BUDGET`, gives `RevisionRequested`.

## Public workflow endpoints (JWT)

| Method | Path | Roles |
|--------|------|-------|
| POST | `/api/trip-requests/{id}/start-planning` | Tourist (owner). 202; 409 if a workflow is already running |
| POST | `/api/trip-requests/{id}/passport-photo` | Tourist (owner). Multipart `file`, JPEG/PNG by content, ≤ 5 MB; stored privately in `UPLOADS_DIR` under a random name |
| GET | `/api/trip-requests/{id}/workflow` | Tourist (owner), OperationsManager — the trip's newest workflow; 404 before planning |
| GET | `/api/workflows/{id}` | Tourist (owner), OperationsManager, Admin — status, plan, validation result, current step, outcome, timings |
| GET | `/api/workflows/{id}/steps` | same — ordered by `step_no` |
| GET | `/api/workflows?status=&page=&pageSize=` | OperationsManager, Admin |
| POST | `/api/quotations/{id}/approve` | OperationsManager — 409 ProblemDetails and full rollback on any failure |
| POST | `/api/quotations/{id}/reject` | OperationsManager |
| POST | `/api/quotations/{id}/request-revision` | OperationsManager — `{"comment": "..."}` required; calls the agent `/replan` |

## Internal API (agent service only)

`[AllowAnonymous]` for JWT, protected by `InternalKeyAuthFilter`: header `X-Internal-Key` must equal
`INTERNAL_AGENT_KEY`, otherwise 401 (also 401 for everyone if the key is not configured).

| Method | Path | Backed by |
|--------|------|-----------|
| GET | `/api/internal/attractions?city=` | Trips `IAttractionService` |
| GET | `/api/internal/distance?from=&to=` | `IDistanceService` (OpenRouteService → `city_distances`); 404 if unknown |
| GET | `/api/internal/weather?city=&date=` | `IWeatherService` (OpenWeatherMap); 404 when no forecast |
| GET | `/api/internal/availability/guides?from=&to=&language=&pax=` | Resources `IResourceCatalog` |
| GET | `/api/internal/availability/vehicles?from=&to=&seats=` | Resources `IResourceCatalog` |
| GET | `/api/internal/availability/rooms?city=&night=&rooms=` | Resources `IResourceCatalog` |
| GET | `/api/internal/rates` (alias `/rate-card`, used by the agent tool) | Resources `IResourceCatalog` |
| GET | `/api/internal/fx-rate` | `IExchangeRateService` (open.er-api.com, 1 h cache, stale fallback) |
| POST | `/api/internal/workflows/{id}/steps` | creates an `agent_steps` row (snake_case body) |
| POST | `/api/internal/workflows/{id}/proposal` | runs `ProposalValidator`, sets status, creates a quotation version |

All third-party clients: typed `HttpClient`, 5 s per try, one Polly retry, then the fallback above. Keys are
never logged (ORS key in a header; the OWM client has HTTP logging removed because OWM needs the key in the URL).

## Components not merged yet (Students B and C)

The workflow talks to Resource Management and Quotations only through the ports in
`Application/Workflows/Ports`: `IResourceCatalog`, `IResourceHoldService` (overlap check + staged hold, throws
`ConflictException`) and `IQuotationStore`. Until B and C register their real services in
`Infrastructure/Workflows/WorkflowsSetup.cs`, placeholders answer **503** and a proposal ends `FailedSafely`
with "… is not available yet". Tests use in-memory fakes that commit only when the `DbContext` saves.

## Tests

```bash
dotnet test
```

EF Core InMemory (Docker/Testcontainers not required). The agent service, third-party APIs and the B/C ports
are faked; `Workflows/Fixtures/*.json` are real bodies captured from the Python agent service (contract test).
