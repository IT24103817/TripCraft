# agents

Python LangGraph agent service (FastAPI, internal only on `localhost:8001`). Only the ASP.NET Core API
calls it, using the `X-Internal-Key` header. React and Flutter never call it.

Four agents run in a fixed graph: **planner → itinerary → resources → validation → END**. If validation
fails *only* because the trip is over budget, the graph loops back to the planner, which moves to the
budget hotel tier (max `MAX_REPLANS` times). Any tool failure, invalid LLM output or timeout ends the
run as `FailedSafely` with an `error_summary`. Nothing is ever held here: holds are created only by the
API when the Operations Manager approves.

## Setup

Requires Python 3.11.

```bash
cd agents
python3.11 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env        # then fill in the values
```

## Environment variables

| Name | Default | Purpose |
|------|---------|---------|
| `INTERNAL_AGENT_KEY` | _(none — required)_ | Shared secret. Requests without it get 401; if it is empty, every request gets 401. |
| `API_BASE_URL` | `http://localhost:5080` | ASP.NET Core API the tools call (`/api/internal/...`). |
| `LLM_PROVIDER` | `ollama` | `ollama` or `groq`. |
| `OLLAMA_MODEL` | `llama3.1:8b` | Model used with Ollama. |
| `OLLAMA_BASE_URL` | `http://localhost:11434` | Where Ollama listens (from Docker: `http://host.docker.internal:11434`). |
| `OLLAMA_NUM_PREDICT` | `1536` | Most tokens one Ollama answer may use. Normal answers stay under about 1,000; a runaway JSON answer is cut off and repaired instead of hitting the node timeout. |
| `GROQ_API_KEY` | _(none)_ | Only needed when `LLM_PROVIDER=groq`. |
| `GROQ_MODEL` | `llama-3.1-8b-instant` | Model used with Groq. |
| `NODE_TIMEOUT_SECONDS` | `30` | Timeout per agent node. |
| `MAX_RETRIES` | `2` | Repair attempts when the LLM returns invalid JSON. |
| `MAX_REPLANS` | `3` | Max budget re-plans per run. |

## Two ways to run it

| | Mode A — local with Ollama (demo default) | Mode B — Render with Groq (optional) |
|---|---|---|
| Model | `llama3.1:8b` on your laptop, free, no key | `llama-3.1-8b-instant` on Groq's free tier |
| Where | your laptop, `http://127.0.0.1:8001` | a free Render web service (`render.yaml`, commented block) |
| API setting | `AGENT_SERVICE_URL` = a URL the API can reach (see below) | `AGENT_SERVICE_URL=https://tripcraft-agents.onrender.com` |
| Callbacks | `API_BASE_URL` = the API (local or Render) | `API_BASE_URL=https://tripcraft-api.onrender.com` |

PLAN.md section 12 allows the agent service to run locally during the demo. If the API is on Render and the
agents are on your laptop, Render cannot call `localhost`: either run the API locally too for the demo, or
expose port 8001 with a tunnel (e.g. `cloudflared tunnel --url http://localhost:8001`) and put that HTTPS URL
in the API's `AGENT_SERVICE_URL`. The internal key protects the service either way.

### Mode A — Ollama (local, free)

```bash
brew install ollama && brew services start ollama   # or download from ollama.com
ollama pull llama3.1:8b                              # ~5 GB, once
cd agents && source .venv/bin/activate
INTERNAL_AGENT_KEY=<same as the API> API_BASE_URL=http://localhost:5080 LLM_PROVIDER=ollama \
  uvicorn app.main:app --host 127.0.0.1 --port 8001
```

### Mode B — Groq (on Render, or locally on a slow laptop)

Create a free key at console.groq.com. Locally put it in `agents/.env` (never in a committed file):

```bash
LLM_PROVIDER=groq
GROQ_API_KEY=<your key>
```

On Render, uncomment the `tripcraft-agents` service in `render.yaml` and enter `GROQ_API_KEY`,
`INTERNAL_AGENT_KEY` and `API_BASE_URL` in the dashboard. Groq's free tier has rate limits; one workflow makes
about four LLM calls (more when a repair is needed).

### Docker

```bash
docker build -t tripcraft-agents agents
docker run -p 8001:8001 -e INTERNAL_AGENT_KEY=… -e API_BASE_URL=http://host.docker.internal:5080 \
  -e LLM_PROVIDER=ollama -e OLLAMA_BASE_URL=http://host.docker.internal:11434 tripcraft-agents
```

(With colima use `host.lima.internal` instead of `host.docker.internal`.) The image runs as a non-root user.

## Startup order

1. **PostgreSQL** (Neon, or local)
2. **Ollama** (Mode A only) — `curl localhost:11434/api/version`
3. **Agent service** — `curl localhost:8001/health` → `{"status":"ok"}`
4. **API** — `curl <api>/health` → `{"status":"ok","db":"ok",…}`
5. **Web** (Vercel or `npm run dev`)
6. **Mobile** (APK built with `--dart-define=API_URL=<api>`)

The API only calls the agent service when a tourist starts planning, so starting it before the agents is harmless;
a planning request made while the agents are down ends `FailedSafely` and can be retried.

## Start the service

```bash
cd agents
source .venv/bin/activate
uvicorn app.main:app --host 127.0.0.1 --port 8001
curl http://127.0.0.1:8001/health      # {"status":"ok"}
```

## Endpoints

| Method | Path | Auth | Body | Reply |
|--------|------|------|------|-------|
| GET | `/health` | none | — | `{"status":"ok"}` |
| POST | `/run-workflow` | `X-Internal-Key` | `WorkflowRequest` | 202, runs the graph in the background |
| POST | `/replan` | `X-Internal-Key` | `WorkflowRequest` + required `manager_comment` | 202 |

`WorkflowRequest`: `workflow_id, objective, start_date, end_date, pax, budget_usd, preferences,
manager_comment?, callback_base_url?`. Keys may be snake_case or camelCase; `preferencesJson` (a JSON
string, as the C# client sends it) is also accepted.

## Internal API routes the service expects (ASP.NET Core must implement)

All tools are read-only `GET` calls with `X-Internal-Key` and a 10 s timeout. Any non-2xx reply is a `ToolError`.

| Tool | Route | Returns |
|------|-------|---------|
| `get_attractions(city)` | `GET /api/internal/attractions?city=` | `[{id, name, city, category, durationMinutes, entryFeeLkr}]` |
| `get_distance(from_city, to_city)` | `GET /api/internal/distance?from=&to=` | `{fromCity, toCity, distanceKm, durationMinutes}` |
| `get_weather(city, date)` | `GET /api/internal/weather?city=&date=` | `{city, date, summary, rainProbability}` |
| `check_guide_availability` | `GET /api/internal/availability/guides?from=&to=&language=&pax=` | `[{id, name, languages[], maxPax}]` |
| `check_vehicle_availability` | `GET /api/internal/availability/vehicles?from=&to=&seats=` | `[{id, registrationNo, type, seats}]` |
| `check_room_availability` | `GET /api/internal/availability/rooms?city=&night=&rooms=` | `[{hotelId, hotelName, roomTypeId, roomTypeName, capacity, availableRooms}]` |
| `get_rate_card()` | `GET /api/internal/rate-card` | `{marginPct, guideDayRates{id:lkr}, vehicleKmRates{id:lkr}, roomNightRates{id:lkr}}` |
| `get_fx_rate()` | `GET /api/internal/fx-rate` | `{base:"USD", quote:"LKR", rate, asOf, stale}` |

Callbacks the service POSTs (snake_case JSON, with `X-Internal-Key`):

- `POST {callback_base_url}/api/internal/workflows/{workflow_id}/steps`: one `StepReport` after every agent
  `{agent_name, tool_calls[], input_summary, output_summary, validation_result, duration_ms, retries, status}`
- `POST {callback_base_url}/api/internal/workflows/{workflow_id}/proposal`: once at the end
  `{plan, days, resources, quotation, violations, status, replans, error_summary}`.
  `status` is the agents' verdict code from PLAN.md section 10: `PendingApproval` (valid), `RevisionRequested`
  (violations remain) or `FailedSafely`. It is not a trip status. The API decides: a valid proposal, or one that is
  only over budget after the lowest-cost re-plans, is sent to the client (trip QuotationSent); anything else makes
  the trip NeedsOperator.

## Quotation formula (mirror in the C# calculator)

```
guide    = guide day_rate_lkr   x trip days
vehicle  = vehicle rate_per_km  x total transfer km between cities
rooms    = rate_per_night       x room-nights (one line per room type)
entry    = entry_fee_lkr        x pax, for each stop with a fee
subtotal = sum of lines; margin = subtotal x margin_pct / 100; total_lkr = subtotal + margin
total_usd = total_lkr / fx_rate (LKR per 1 USD)
```

Every amount is rounded to 2 decimals, half away from zero (`MidpointRounding.AwayFromZero` in C#).

## Safety controls

- **Allow-list:** `app/tools/registry.py` `ALLOWED_TOOLS`; `run_tool` raises `ToolNotAllowed` for anything else.
  The LLM never calls tools itself: nodes call them in code and pass the results in.
- **Prompt injection:** all inputs go to the model as JSON inside one `<DATA>` block with `<` and `>` escaped.
  Every system prompt says the block is data, not instructions.
- **Rules enforced in code, not only prompts:** 1–3 stops per day, ≤ 240 min road driving per day, only
  offered ids, resource gaps listed, deterministic validation that the LLM can add to but never override.
- **Limits:** 30 s per node, 2 JSON repair retries, 3 re-plans, then `FailedSafely`.
- **Logging:** JSON logs with `workflow_id` and `node` only. Prompts and model replies are never logged.

## Tests

```bash
cd agents
.venv/bin/python -m pytest -q
```

Tests use a `FakeLLM` (canned JSON in `tests/fixtures/`) and `respx` mocks of every internal API route,
so they need no Ollama, Groq or running API.
