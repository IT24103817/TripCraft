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
| `LLM_PROVIDER` | `groq` when hosted on Render (`RENDER=true`), else `ollama` | `ollama` (local), `groq` (hosted) or `gemini` (optional). The Admin Settings page can override it per run. |
| `OLLAMA_MODEL` | `llama3.1:8b` | Model used with Ollama. |
| `OLLAMA_BASE_URL` | `http://localhost:11434` | Where Ollama listens (from Docker: `http://host.docker.internal:11434`). |
| `OLLAMA_NUM_PREDICT` | `1536` | Most tokens one Ollama answer may use. Normal answers stay under about 1,000; a runaway JSON answer is cut off and repaired instead of hitting the node timeout. |
| `GROQ_API_KEY` | _(none)_ | console.groq.com key; needed when the provider is `groq`. **Secret.** |
| `GROQ_MODEL` | `qwen/qwen3.8-27b` | Model used with Groq (see "Groq free tier" below). |
| `RATE_LIMIT_RETRIES`, `RATE_LIMIT_BACKOFF_SECONDS` | `3`, `2` | On 429 (rate limit) or 503 (overloaded) the call waits the provider's `retry-after`, or 2, 4, 8 s, and retries; each retry is a step warning; then the node fails safely. |
| `RATE_LIMIT_MAX_WAIT_SECONDS` | `20` | Longest single wait. A longer `retry-after` (a daily limit) fails at once. Hosted Groq: `60`, with `NODE_TIMEOUT_SECONDS=90`. |
| `GEMINI_API_KEY`, `GEMINI_MODEL` | _(none)_, `gemini-3.8-flash` | Optional provider `gemini` (Google AI Studio). Free tier: 20 requests per day per model, so not for hosting. |
| `NODE_TIMEOUT_SECONDS` | `30` | Timeout per agent node. |
| `MAX_RETRIES` | `2` | Repair attempts when the LLM returns invalid JSON. |
| `MAX_REPLANS` | `3` | Max budget re-plans per run. |

## Two ways to run it

| | Mode A — local with Ollama | Mode B — hosted on Render with Groq |
|---|---|---|
| Model | `llama3.1:8b` on your laptop, free, no key | `qwen/qwen3.8-27b` on Groq's free tier (`GROQ_API_KEY`) |
| Where | your laptop, `http://127.0.0.1:8001` | the `tripcraft-agents` Render web service (`render.yaml`) |
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

### Mode B — Groq (hosted on Render, or locally without Ollama)

Create a free key at console.groq.com. Locally put it in `agents/.env` (never in a committed file):

```bash
LLM_PROVIDER=groq
GROQ_API_KEY=<your key>
```

On Render, the `tripcraft-agents` service in `render.yaml` already sets `LLM_PROVIDER=groq`, `GROQ_MODEL`,
`NODE_TIMEOUT_SECONDS=90` and `RATE_LIMIT_MAX_WAIT_SECONDS=60`; enter `GROQ_API_KEY`, `INTERNAL_AGENT_KEY` and
`API_BASE_URL` in the dashboard. On Render `LLM_PROVIDER` may also be left unset: the service then picks Groq.

#### Groq free tier (checked 5 Oct 2026)

- **Model.** Groq's docs list `llama-3.1-8b-instant` as a production model, but our key gets
  `404 model_not_found` for it: `GET /openai/v1/models` offers this account `openai/gpt-oss-20b`,
  `openai/gpt-oss-120b`, `qwen/qwen3.8-27b` and `allam-2-7b` (4k context, too small) as chat models. All three
  larger ones answer in JSON mode; `qwen/qwen3.8-27b` was the fastest (0.27 s) and used the fewest tokens (the
  gpt-oss models spend extra tokens on hidden reasoning), so it is the default. Override with `GROQ_MODEL`.
- **Limits.** Groq's rate-limit page does not list this model's numbers. The API returns two of them on every
  response (`x-ratelimit-limit-requests`, `x-ratelimit-limit-tokens`) and the third in its 429 message: **1,000
  requests per day, 8,000 tokens per minute and 200,000 tokens per day** per model on the free tier. The daily
  token budget refills gradually (about 2.3 tokens a second). A 429 carries `retry-after` in seconds.
- **What that means.** One planning run uses about 7,000 tokens (planner ≈ 1.1k, itinerary ≈ 1.5k, resources
  ≈ 2.5k, validation ≈ 2.0k), just under one minute's budget. A second run within the same minute, or an
  over-budget trip (up to four passes, ≈ 28k tokens), waits for Groq's `retry-after`; that is why hosted Groq allows
  waits of up to 60 s and a 90 s node timeout. The daily token budget is the real ceiling: about 28 normal planning
  runs a day (or 7 over-budget trips with their four passes). When it is used up, Groq asks for a wait longer than
  60 s and the run fails safely with "quota used up … retry in N s"; **Retry planning** later.

Gemini (`LLM_PROVIDER=gemini`, `GEMINI_API_KEY`) also works, but its free tier allows only 20 requests per day
per model (`gemini-3.8-flash`), about four trips, so it is not used for hosting.

### Live suite (real model)

The golden cases also run against a real model; this is never part of CI:

```bash
LIVE_LLM_PROVIDER=groq LIVE_NODE_TIMEOUT_SECONDS=90 RATE_LIMIT_MAX_WAIT_SECONDS=60 \
  .venv/bin/python -m pytest -m live tests/live   # GROQ_API_KEY from agents/.env or the environment
LIVE_LLM_PROVIDER=ollama LIVE_NODE_TIMEOUT_SECONDS=120 .venv/bin/python -m pytest -m live tests/live
```

Each test writes the raw model replies to `tests/live/output/` (git-ignored) to compare providers.

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
