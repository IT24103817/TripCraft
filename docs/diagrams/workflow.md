# The assessed workflow (PLAN.md section 6, v1.1 lifecycle)

The journey from the tourist's request to the finished tour. There are two human gates: the manager reviews the
proposal, and the tourist accepts the price. Nothing is held until the manager confirms an accepted quotation.

## Trip statuses

One C# class, `TripStatusMachine` (backend/src/TripCraft.Application/Trips/TripStatusMachine.cs), lists every
allowed move. Every endpoint changes a status through it, and an illegal move returns 409. Each change is written to
the trip history (audit_logs) with the actor and a reason.

```mermaid
stateDiagram-v2
    [*] --> Submitted
    Submitted --> Planning: start planning
    Planning --> PendingReview: proposal valid (or only over budget)
    Planning --> FailedSafely: agent error / Hard rule
    FailedSafely --> Planning: Try again
    PendingReview --> QuotationSent: manager Send to client
    PendingReview --> RevisionRequested: manager Request revision (comment)
    RevisionRequested --> PendingReview: re-planned, new version
    RevisionRequested --> FailedSafely: re-plan failed
    QuotationSent --> ClientAccepted: tourist Accept
    QuotationSent --> PendingReview: tourist Decline (reason)
    ClientAccepted --> Confirmed: manager Confirm (holds, vouchers, email)
    ClientAccepted --> PendingReview: manager reopens review
    Confirmed --> InProgress: first check-in (voucher scan or GPS)
    InProgress --> Completed: last stop checked in
    Submitted --> Cancelled
    FailedSafely --> Cancelled
    PendingReview --> Cancelled: reject / cancel
    RevisionRequested --> Cancelled
    QuotationSent --> Cancelled
    ClientAccepted --> Cancelled
    Confirmed --> Cancelled: holds released
    Completed --> [*]
    Cancelled --> [*]
```

A tourist may cancel until `CANCELLATION_CUTOFF_DAYS` (default 3) days before the start. After that, the app says
why cancellation is closed and offers the operator contact. A manager can cancel at any time.

## Sequence

```mermaid
sequenceDiagram
    autonumber
    actor T as Tourist (Flutter)
    participant API as ASP.NET Core API
    participant DB as PostgreSQL
    participant AG as Agent service (LangGraph)
    actor OM as Operations Manager (React)
    actor G as Guide (Flutter)

    T->>API: GET /api/attractions/cities
    T->>API: POST /api/trip-requests {…, cities:[Kandy, Ella]}
    API->>DB: trip Submitted + audit
    T->>API: POST /api/trip-requests/{id}/start-planning
    API->>DB: workflow Planning, trip Planning (TripStatusMachine), audit
    API->>AG: POST /run-workflow (X-Internal-Key, cities)
    loop Planner, Itinerary, Resources, Validation
        AG->>API: GET /api/internal/* tools, POST …/steps
    end
    AG->>API: POST /api/internal/workflows/{id}/proposal
    API->>API: ProposalValidator (deterministic rules)
    API->>DB: quotation v1 + snapshot, trip PendingReview, notify managers

    Note over OM: Review page: Send · Request revision · Edit directly · Reject
    alt Request revision (comment)
        OM->>API: POST /api/quotations/{id}/request-revision
        API->>AG: POST /replan (comment, violations)
        AG->>API: proposal → quotation v2, trip PendingReview (v1 and v2 side by side)
    else Edit directly
        OM->>API: PUT …/proposal/days/{n}, PUT …/proposal/resources (from GET /api/availability)
        OM->>API: POST /api/quotations/{id}/calculate (Re-price → new version)
    end
    OM->>API: POST /api/quotations/{id}/approve (Send to client)
    API->>DB: quotation Approved, trip QuotationSent, notify tourist
    T->>API: GET /api/notifications/mine (polling) → local notification
    alt Accept
        T->>API: POST /api/quotations/{id}/accept → trip ClientAccepted, notify managers
    else Decline (reason)
        T->>API: POST /api/quotations/{id}/decline → trip PendingReview, reason shown to the manager
    end
    OM->>API: POST /api/trip-requests/{id}/confirm
    rect rgba(120, 160, 255, 0.15)
        API->>DB: BEGIN
        API->>DB: holds (overlap check) → saved itinerary → vouchers (HMAC-signed) →<br/>decision → workflow Completed → trip Confirmed → notifications → email outbox → audit
        API->>DB: COMMIT (any failure: ROLLBACK, 409, trip stays ClientAccepted)
    end
    API->>API: send the outbox email (SMTP or pickup folder), after the commit
    T->>API: GET /api/trips/{id}/vouchers → QR codes in the app (PDF: …/vouchers.pdf)
    G->>API: POST /api/check-ins {voucherCode} (mobile_scanner) — or GPS
    API->>DB: check-in, trip InProgress … Completed after the last stop
```

**Safe-failure path:** with budget USD 400, Validation finds `OVER_BUDGET`. That is a Soft rule, so the trip is in
PendingReview with a warning, and "Send to client" stays disabled. The manager then requests a revision (the
Planner re-plans with the budget hotel tier) or edits and re-prices. A Hard rule or an agent error ends the trip in
`FailedSafely`, and the tourist can press "Try again".
