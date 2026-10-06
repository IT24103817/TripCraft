# Groq vs Ollama — agent quality and latency (v1.1.2)

Measured on 6 October 2026 on `main` (the Resource agent's rooms-by-reference fix, commit `63c1477`), with the local
stack (API, agent service, React) on one Apple Silicon laptop and the seeded PostgreSQL database. The hosted
service (Render) was **not** used for any of these runs.

| Provider | Model | Where it ran |
|---|---|---|
| Ollama | `llama3.1:8b` (Q4_K_M) | locally, no key |
| Groq (free tier) | `qwen/qwen3.8-27b` | Groq's API; hosted settings: `NODE_TIMEOUT_SECONDS=90`, `RATE_LIMIT_MAX_WAIT_SECONDS=60` |

## Final table

| | Ollama `llama3.1:8b` | Groq `qwen/qwen3.8-27b` |
|---|---|---|
| Live golden suite (6 real-model cases) | **6 / 6** (6 Oct, this code) | **6 / 6** (5 Oct, before the rooms fix; see below) |
| Playwright end to end (6 specs) | 6 / 6 (5 Oct) | **6 / 6** (6 Oct, this code) |
| k6 `agent-latency.js`, 5 paced trips | **5 / 5** quotation sent | **5 / 5** quotation sent |
| **Time to quotation, median** (min – max) | **98.8 s** (96.5 – 104.6) | **32.1 s** (32.1 – 34.1) |
| Planner, median step | 9.0 s | 1.3 s |
| Itinerary Analysis, median step | 24.2 s | 2.2 s |
| Resource & Action, median step | 53.2 s | 6.3 s |
| Validation & Safety, median step | 9.1 s | 22.3 s ¹ |

¹ The Groq step times include waiting for Groq's free-tier limit of 8,000 tokens per minute. Groq's own answer
time is about 0.4–2 s per agent; one trip uses about 11,300 tokens (planner ≈ 1.1k, itinerary ≈ 1.9k,
resources ≈ 4.5k, validation ≈ 3.7k), more than one minute's budget, so in every run the Validation call waited
about 20 s for Groq's `retry-after` (and the Resource call waited in 3 of 5 runs). Each wait is recorded as a
warning on that agent step. Ollama's step times are the model itself on the laptop.

**Result: Groq reaches the quotation about 3× faster than Ollama (32 s vs 99 s), with every case passing.** On the
free tier the time is set by the per-minute token limit, not by the model.

## Method

- **k6, paced.** `k6 run -e PAUSE_SECONDS=120 tests/perf/agent-latency.js`: five demo trips (4 travellers, Kandy
  and Ella, USD 1,500, English guide), one after another, with a **120 s pause between trips that is not part of
  the measured time**. One trip uses about 11,300 tokens against Groq's 8,000 tokens a minute, so trips started
  back to back would mostly measure Groq's quota rather than the system; the pause measures one trip at a time, as
  in a demo. Ollama was run with the same pacing so the two are comparable. Time to quotation = from
  `start-planning` until the quotation is sent to the client automatically (workflow `Approved`).
- **Per-agent medians** come from the `agent_steps.duration_ms` rows of those five workflows.
- **Live golden suite** (`pytest -m live tests/live`): the golden case, over budget → re-plan at lowest cost →
  best-price flag, prompt injection, "the model cannot approve an over-budget trip", tool failure → failed safely,
  and a schema violation repaired by the real model. Same prompts, schemas and assertions for both providers.
- **Playwright** (`tests/e2e`): roles × 4, the over-budget request sent at the best price, and the demo trip sent
  automatically, accepted and confirmed in the browser.

## Groq accounts used

Groq's free tier allows 1,000 requests, 8,000 tokens a minute and 200,000 tokens a day **per account**, and a
quality gate uses most of a day's budget. Three team members' own Groq accounts were used, so that the hosted
service's budget stays free for the demo:

| Account | Used for |
|---|---|
| Account 1 — the original key, **also the hosted service's key** (Render `tripcraft-agents`) | the Groq live golden suite (5 Oct) and the first, unpaced k6 attempt (6 Oct, 03:03 UTC). Nothing else since. |
| Account 2 — a team member's key, local only | paced k6 attempts on 6 Oct; they exposed the room-copying problem fixed in `63c1477` (below) |
| Account 3 — the third team member's key, local only | Playwright 6/6 and the final five paced k6 trips in the table |

The local gate (accounts 2 and 3) and the hosted service (account 1) used **different team members' Groq
accounts**. No request was sent to the hosted service during this gate.

## What the gate found and fixed

1. **Invented validation concerns (5 Oct).** A real model sometimes reported concerns such as "3 stops is more than
   3". They are now advisory notes on the step; only code violations decide the status (commit `8de10a9`).
2. **Lowest-cost re-plan kept a paid stop (5 Oct).** A paid stop with no free alternative is now dropped while
   the day keeps at least one stop (commit `8de10a9`).
3. **The Resource agent miscopied the suggested room plan (6 Oct).** On the seeded data the model often put rooms
   on the wrong nights, forcing a 5,400-token repair that Groq's per-minute limit could not absorb inside the
   90 s node timeout (3 of 5 paced trips failed safely). The model can now choose code's plan by reference
   (`"use_suggested_rooms": true`); rooms it lists itself are still checked and repaired, so a real shortfall still
   fails safely. The Resource answer fell from about 780 to 115 output tokens, with no repair in any Groq run
   (commit `63c1477`). `llama3.1:8b` still lists rooms itself and needs one repair per run for its vehicle id,
   which is why Ollama's Resource step is unchanged (53 s).
4. **Token figures.** The earlier estimate of about 7,000 tokens a trip came from the small test fixtures; with the
   seeded database a trip uses about 11,300–12,000. The docs now say so (about 16 trips a day on one free account,
   trips about two minutes apart).

**Not re-run:** the Groq live golden suite on this exact code. It passed 6/6 on 5 October (account 1, before
fix 3); rerunning it would cost about 80,000 tokens, which account 3 no longer had after Playwright and k6. The
behaviour it covers is also exercised by the Groq Playwright and k6 runs above, and the Resource change is covered
by the FakeLLM suite (90 tests) and the Ollama live suite (6/6 on this code).

## Raw evidence

- k6 summaries and agent logs were kept outside the repository (they contain a login token); the numbers above are
  copied from them.
- The step rows can be re-read with
  `select agent_name, duration_ms, validation_result from agent_steps …` on the local `tripcraft_main` database.
- Earlier runs: [v1.1-e2e.md](v1.1-e2e.md); provider decision: [ADR-006](../adr/ADR-006-llm-provider.md).
