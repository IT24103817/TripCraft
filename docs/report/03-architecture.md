# 3. Architecture

## System architecture

React (staff) and Flutter (tourists, guides) call only the ASP.NET Core API. The API owns PostgreSQL, calls the
third-party APIs through typed clients, and calls the internal agent service, which calls back through an
internal API protected by `X-Internal-Key`. Diagram: section 13 (`docs/diagrams/architecture.md`).

TODO: insert the architecture PNG once exported (`docs/diagrams/png/architecture-1.png`, see `docs/diagrams/README.md`).

## Backend layering

`TripCraft.Api` (controllers, middleware, auth) → `TripCraft.Application` (entities, DTOs, validators, services,
interfaces) → `TripCraft.Infrastructure` (EF Core `AppDbContext`, repositories, migrations, seeders, HTTP clients).
Controllers return DTOs only; all endpoints are async; FluentValidation runs on every request DTO; one middleware
maps exceptions to RFC 7807 ProblemDetails (400/401/403/404/409/503).

## Agentic AI architecture

A FastAPI service runs a LangGraph `StateGraph`: Planner → Itinerary → Resources → Validation, with a re-plan
edge back to the Planner when the only violation is over budget (at most 3 times). Each node calls its
allow-listed tools through `run_tool`, calls the LLM through `call_json` (JSON mode, Pydantic schema, ≤ 2 repair
messages), enforces its rules in code, and reports a step to the API. The API validates the final proposal again
in C# and pauses for a human. Diagrams: `docs/diagrams/agents.md`, `docs/diagrams/workflow.md`.

TODO: insert the agent and workflow PNGs once exported (see `docs/diagrams/README.md`).

## Integration boundaries for Students B and C

Resource Management and Quotations are reached through ports in `TripCraft.Application/Workflows/Ports/`
(`IResourceCatalog`, `IResourceHoldService`, `IQuotationStore`). Placeholders answer 503 until the real services
are registered in `TripCraft.Infrastructure/Workflows/WorkflowsSetup.cs`.
