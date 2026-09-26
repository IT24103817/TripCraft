# The assessed workflow (PLAN.md section 6)

From the tourist's request to the confirmed trip, including the pause for human approval.

```mermaid
sequenceDiagram
    autonumber
    actor T as Tourist (Flutter)
    participant API as ASP.NET Core API
    participant DB as PostgreSQL
    participant AG as Agent service (LangGraph)
    participant LLM as Ollama / Groq
    actor OM as Operations Manager (React)

    T->>API: POST /api/auth/login
    API-->>T: 200 JWT (role Tourist)
    T->>API: POST /api/trip-requests (demo request)
    API->>API: FluentValidation (dates, pax, budget, passport)
    API->>DB: insert trip_requests (Submitted) + audit
    API-->>T: 201 Created
    T->>API: POST /api/trip-requests/{id}/passport-photo (multipart)
    T->>API: POST /api/trip-requests/{id}/start-planning
    API->>DB: insert agent_workflows (Planning), trip → Planning, audit (one SaveChanges)
    API->>AG: POST /run-workflow (X-Internal-Key)
    AG-->>API: 202 Accepted
    API-->>T: 202 Accepted {workflowId}

    loop Planner, Itinerary, Resources, Validation (≤ 3 budget re-plans)
        AG->>LLM: system prompt + <DATA>inputs</DATA>, JSON mode
        LLM-->>AG: JSON (validated by Pydantic, repaired ≤ 2 times)
        AG->>API: GET /api/internal/* tools (attractions, distance, weather, availability, rates, fx-rate)
        AG->>API: POST /api/internal/workflows/{id}/steps
        API->>DB: insert agent_steps
    end
    AG->>API: POST /api/internal/workflows/{id}/proposal
    API->>API: ProposalValidator (deterministic C# rules)
    API->>DB: workflow → PendingApproval, validation_result, final_outcome, quotation version, audit

    Note over T,OM: PAUSE — nothing is held until a human approves
    loop every 10 s while Planning
        T->>API: GET /api/trip-requests/{id}/workflow
    end
    T-->>T: "Awaiting operator approval"

    OM->>API: GET /api/workflows?status=PendingApproval, GET /api/workflows/{id}
    OM->>API: POST /api/quotations/{id}/approve
    rect rgba(120, 160, 255, 0.15)
        API->>DB: BEGIN
        API->>DB: resource holds (overlap check) → quotation Approved → trip Confirmed →<br/>workflow Completed → approval decision → audit
        API->>DB: COMMIT (any failure: ROLLBACK, 409)
    end
    API-->>OM: 200 "Approved. Trip is now confirmed"
    T->>API: pull to refresh → GET /api/trip-requests/{id}
    API-->>T: status Confirmed
```

**Second path (safe failure):** budget USD 400 → Validation finds `OVER_BUDGET` → the graph re-plans with the budget
hotel tier; if still over budget the API sets `RevisionRequested` and the manager can request a revision
(`POST /api/quotations/{id}/request-revision`, which calls the agent's `/replan`).

**Current state:** the Resource Management (Student B) and Quotations (Student C) steps are served by placeholder
ports that answer 503 until those components are merged, so a live run ends `FailedSafely` at the Resources agent
(see `docs/TEST-EVIDENCE.md`).
