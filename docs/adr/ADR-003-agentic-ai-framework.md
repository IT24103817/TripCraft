# ADR-003: Agentic AI framework and orchestration

- **Status:** Accepted
- **Date:** 2026-09-26
- **Author:** Student A

## Context

Four agents (Planner, Itinerary Analysis, Resource & Action, Validation & Safety) must run in a fixed order,
share state, loop back when over budget (at most 3 re-plans), time out, fail safely, and report every step.
The assignment requires ASP.NET Core as the only public backend and allows a Python agent service called by it.
Business rules that decide money and bookings must be deterministic and testable, not left to an 8B model.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **LangGraph (Python)** | The lab stack; a `StateGraph` with nodes and conditional edges maps 1:1 to our four agents and the re-plan loop. The Python LLM ecosystem (langchain-ollama, langchain-groq, Pydantic) is mature. | A second runtime and service to deploy. Its API moves quickly (we pinned versions). |
| **Microsoft Agent Framework (.NET)** | One language with the API; could run in-process. | Newer, with fewer examples for local Ollama and graph loops; less lab support if we got stuck under a 4-day deadline. |
| **Custom orchestrator** | No framework to learn; full control. | We would write state passing, loops, retries and step reporting ourselves — more code to test and defend, with no benefit over a graph library. |

## Decision

**LangGraph in a FastAPI service** (`agents/`), called only by the API with `X-Internal-Key`. Graph nodes are
the agents. The LLM only proposes; **deterministic rules run in code**: in each node (e.g. ≤ 3 stops, ≤ 240 min
driving, only offered ids) and again in C# (`ProposalValidator`) when the proposal reaches the API. A human
approves before anything is held.

## Consequences

- Tools are plain functions on an allow-list; the LLM never calls a tool itself.
- Every LLM answer is validated by a Pydantic schema; invalid JSON gets up to 2 repair messages, then the
  workflow ends `FailedSafely`.
- Two validation layers can disagree; the API's C# validator decides the status (PendingApproval,
  RevisionRequested or FailedSafely).
- Tests run without a model (FakeLLM + respx), so CI is fast and free; the real model is exercised in the
  e2e and agent-latency runs.

## Where this shows in the code

- `agents/app/graph.py` — `StateGraph`, `guarded()` timeout wrapper, `after_validation` re-plan routing
- `agents/app/nodes/planner.py`, `itinerary.py`, `resources.py`, `validation.py` — the four agents
- `agents/app/tools/registry.py` — `ALLOWED_TOOLS`, `run_tool`
- `agents/app/llm.py` — JSON mode and the repair loop
- `backend/src/TripCraft.Infrastructure/Workflows/AgentServiceClient.cs` — API → agents
- `backend/src/TripCraft.Application/Workflows/ProposalValidator.cs` — deterministic C# rules
