# System architecture

TripCraft's version of the assignment's reference architecture. Both clients talk **only** to the ASP.NET Core
API; the agent service is internal and reachable only with the `X-Internal-Key` header.

```mermaid
flowchart LR
    subgraph Clients
        W["React staff app<br/>web/ (Vite, Vercel)<br/>Operations Manager, Admin"]
        M["Flutter app<br/>mobile/ (Android APK)<br/>Tourist, Guide"]
    end

    subgraph API["ASP.NET Core 8 Web API — backend/ (Render, Docker)"]
        C["Controllers<br/>JWT + roles, FluentValidation,<br/>ProblemDetails"]
        S["Application services<br/>Trips, Workflows, Quotations,<br/>ProposalValidator"]
        R["Repositories / AppDbContext<br/>EF Core + Npgsql"]
        I["Internal API /api/internal/*<br/>InternalKeyAuthFilter"]
        X["Typed HttpClients<br/>5 s timeout, 1 Polly retry, fallback"]
        C --> S --> R
        I --> S
        S --> X
    end

    DB[("PostgreSQL 16<br/>Neon")]

    subgraph AG["Agent service — agents/ (FastAPI + LangGraph)"]
        G["Planner → Itinerary → Resources → Validation"]
        T["Allow-listed tools<br/>(read-only GET)"]
        G --> T
    end

    LLM["LLM<br/>Ollama llama3.1:8b (local)<br/>or Groq llama-3.1-8b-instant"]

    FX["open.er-api.com<br/>USD→LKR"]
    ORS["OpenRouteService<br/>distance matrix"]
    OWM["OpenWeatherMap<br/>5-day forecast"]

    W -- "HTTPS + JWT" --> C
    M -- "HTTPS + JWT" --> C
    R --> DB
    S -- "POST /run-workflow, /replan<br/>X-Internal-Key" --> AG
    T -- "GET /api/internal/*<br/>X-Internal-Key" --> I
    G -- "POST steps + proposal<br/>X-Internal-Key" --> I
    G --> LLM
    X --> FX
    X --> ORS
    X --> OWM
```

| Arrow | Code |
|-------|------|
| Clients → API | `web/src/shared/api/http.ts`, `mobile/lib/core/api/api_client.dart` |
| API → agent service | `backend/src/TripCraft.Infrastructure/Workflows/AgentServiceClient.cs` |
| Agent tools → internal API | `agents/app/tools/http_client.py`, `backend/src/TripCraft.Api/Controllers/Internal/` |
| Step reports and proposal → API | `agents/app/callbacks.py`, `backend/src/TripCraft.Api/Controllers/Internal/InternalWorkflowsController.cs` |
| API → third-party APIs | `backend/src/TripCraft.Infrastructure/External/` |
| Agents → LLM | `agents/app/llm.py` |
