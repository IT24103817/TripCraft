# ADR-006: LLM provider

- **Status:** Accepted (revised 2026-10-05: Groq is the hosted provider, Ollama the local one, Gemini optional)
- **Date:** 2026-09-26
- **Author:** Student B (drafted during the build; to be reviewed and defended by the author)

## Context

The four agents need a chat model that returns JSON. The assignment requires no-cost services; the demo must
not fail because a hosted quota ran out or the venue Wi-Fi is poor. The spec's reference diagram uses Ollama.
All secrets must stay out of the repo.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **Ollama, llama3.1:8b, local** | Free, no key, works offline; the spec's reference stack; JSON output mode (`format="json"`). | Needs ~5 GB of disk and a capable laptop; slower than hosted models (about 40 s for Planner + Itinerary on our Apple Silicon laptop); cannot run on Render's free tier. |
| **Groq free tier** (`qwen/qwen3.8-27b`; planned `llama-3.1-8b-instant`) | Very fast hosted inference (≈ 1 s per agent call); JSON mode; 1,000 requests a day. | Needs an API key and internet; free tier 8,000 tokens a minute (a second run within a minute waits) and 200,000 tokens a day (about 28 trips). |
| **Gemini Flash free tier, `gemini-3.8-flash`** (tried 2026-10-05) | Google's current stable Flash model; native JSON output mode; fast. | Free tier allows only **20 requests per day per model** (about four trips), and it often answered 503 "high demand". |
| **OpenAI** | Strongest models and tooling. | Needs a paid account and card — breaks the no-cost rule. |

## Decision

**Ollama `llama3.1:8b`** locally and **Groq** when hosted, switched with `LLM_PROVIDER=ollama|groq|gemini` (or per
run from the Admin Settings page). When `LLM_PROVIDER` is unset the agent service picks Groq on Render
(`RENDER=true`) and Ollama elsewhere. Gemini stays in the code as an optional provider. No OpenAI.

**Why Groq for hosting, and which model (2026-10-05).** Ollama cannot run on Render's free tier, so the hosted agent
service needs an API provider. Gemini was tried first: its free tier allows 20 requests per day per model, which
a single quality-gate run used up, so it cannot carry a demo. Groq's free tier allows 1,000 requests and 200,000
tokens a day, about 28 planning runs. The
planned model `llama-3.1-8b-instant` is listed in Groq's docs, but our key gets `404 model_not_found` for it; of
the models the key can use, `qwen/qwen3.8-27b` answered fastest and with the fewest tokens, which matters under
the 8,000-tokens-per-minute limit, so it is the default (`GROQ_MODEL` overrides it). The same prompts, Pydantic
schemas and tests are used for every provider; no provider-specific prompt was needed. Evidence:
[docs/evidence/groq-vs-ollama.md](../evidence/groq-vs-ollama.md).

## Consequences

- One factory builds either model in JSON mode at temperature 0; nothing else in the code knows the provider.
- The 8B model sometimes returns wrong JSON: in the local end-to-end runs the Itinerary agent needed 2 repair
  messages before its answer passed the rules. The repair loop (≤ 2) and the code-enforced rules make that safe.
- `NODE_TIMEOUT_SECONDS` must be raised (60–120 s) on a slow laptop.
- CI never calls a model (`LLM_PROVIDER=fake`, FakeLLM in tests). The live suite (`pytest -m live`) runs the same
  golden cases against a real model on purpose.
- A hosted model can answer 429 (rate limit) or 503 (overloaded): the call waits the provider's `retry-after` (or
  2, 4, 8 s) and retries, each retry shown as a warning on that agent's step; after three retries, or when the
  provider asks for a longer wait than `RATE_LIMIT_MAX_WAIT_SECONDS` (a daily limit), the node fails safely and the
  trip goes to Needs operator. One planning run uses about 7,000 tokens, so hosted Groq allows 60 s waits and a 90 s
  node timeout to ride out its per-minute token limit.
- The live suite showed that a real model sometimes invents a "concern" that restates a code-checked rule wrongly
  ("3 stops is more than 3"). Such concerns are now advisory notes on the step; only code violations set the status.

## Where this shows in the code

- `agents/app/llm.py` — `get_chat_model()` (Ollama / Groq / Gemini / fake), `call_json()` repair loop and the
  429 / 503 backoff
- `agents/app/config.py` — `LLM_PROVIDER` and its hosted default, `OLLAMA_*`, `GROQ_*`, `RATE_LIMIT_*`, optional
  `GEMINI_*`
- `render.yaml` — the `tripcraft-agents` service with `LLM_PROVIDER=groq` (key entered in the dashboard)
- `agents/tests/test_groq.py`, `agents/tests/test_gemini.py`, `agents/tests/live/` — provider tests and the
  real-model golden suite
- `docs/evidence/perf/agent-latency-summary.json` — measured run times with Ollama
