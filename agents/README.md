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
| `GROQ_API_KEY` | _(none)_ | Only needed when `LLM_PROVIDER=groq`. |
| `GROQ_MODEL` | `llama-3.1-8b-instant` | Model used with Groq. |
| `NODE_TIMEOUT_SECONDS` | `30` | Timeout per agent node. |
| `MAX_RETRIES` | `2` | Repair attempts when the LLM returns invalid JSON. |
| `MAX_REPLANS` | `3` | Max budget re-plans per run. |

## Run with Ollama (local, free)

```bash
brew install ollama          # or download from ollama.com
ollama serve &               # starts on localhost:11434
ollama pull llama3.1:8b
export LLM_PROVIDER=ollama
```

## Run with Groq (faster on a slow laptop)

Create a free key at console.groq.com, then put it in `agents/.env` (never in a committed file):

```bash
LLM_PROVIDER=groq
GROQ_API_KEY=<your key>
```

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
  `status` is `PendingApproval` (valid), `RevisionRequested` (violations remain) or `FailedSafely`.

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
