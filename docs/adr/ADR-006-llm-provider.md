# ADR-006: LLM provider

- **Status:** Accepted
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
| **Groq free tier, llama-3.1-8b-instant** | Very fast hosted inference; JSON mode; the same model family, so prompts carry over. | Needs an API key (a secret to manage) and internet; free-tier rate limits. |
| **OpenAI** | Strongest models and tooling. | Needs a paid account and card — breaks the no-cost rule. |

## Decision

**Ollama `llama3.1:8b`** by default, **Groq `llama-3.1-8b-instant`** as the documented fallback, switched with
`LLM_PROVIDER=ollama|groq`. No OpenAI.

## Consequences

- One factory builds either model in JSON mode at temperature 0; nothing else in the code knows the provider.
- The 8B model sometimes returns wrong JSON: in the local end-to-end runs the Itinerary agent needed 2 repair
  messages before its answer passed the rules. The repair loop (≤ 2) and the code-enforced rules make that safe.
- `NODE_TIMEOUT_SECONDS` must be raised (60–120 s) on a slow laptop.
- CI never calls a model (`LLM_PROVIDER=fake`, FakeLLM in tests).

## Where this shows in the code

- `agents/app/llm.py` — `get_chat_model()` (Ollama / Groq / fake) and `call_json()` repair loop
- `agents/app/config.py` — `LLM_PROVIDER`, `OLLAMA_MODEL`, `OLLAMA_BASE_URL`, `GROQ_API_KEY`, `GROQ_MODEL`
- `render.yaml` — optional `tripcraft-agents` service with `LLM_PROVIDER=groq`
- `docs/evidence/perf/agent-latency-summary.json` — measured run times with Ollama
