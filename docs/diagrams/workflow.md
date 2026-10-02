# The assessed workflow (PLAN.md section 6, v1.1 lifecycle)

The journey from the tourist's request to the finished tour. Quotations go straight to the client: no operator sits
between the agents and the tourist. **The human approval gate is Confirm.** An Operations Manager confirms a
quotation the client accepted, and only then are guide, vehicle and rooms held. Confirm is a manager-only action in
React.

## Trip statuses

One C# class, `TripStatusMachine` (backend/src/TripCraft.Application/Trips/TripStatusMachine.cs), lists every
allowed move. Every endpoint changes a status through it, and an illegal move returns 409. Each change is written to
the trip history (audit_logs) with the actor and a reason. The automatic send is recorded with the actor `System`.

```mermaid
stateDiagram-v2
    [*] --> Submitted
    Submitted --> Planning: start planning
    Planning --> QuotationSent: proposal passed validation (auto-send)
    Planning --> NeedsOperator: agents failed safely / Hard rule
    NeedsOperator --> Planning: manager Retry planning
    NeedsOperator --> QuotationSent: manager Edit & send manually
    QuotationSent --> ClientAccepted: tourist Accept
    QuotationSent --> ClientDeclined: tourist Decline (reason)
    ClientDeclined --> Planning: manager Replan with note
    ClientAccepted --> Confirmed: manager Confirm (holds, vouchers, email)
    ClientAccepted --> QuotationSent: manager Edit & resend (new version)
    Confirmed --> InProgress: first check-in (voucher scan or GPS)
    InProgress --> Completed: last stop checked in
    Submitted --> Cancelled
    NeedsOperator --> Cancelled
    QuotationSent --> Cancelled
    ClientAccepted --> Cancelled
    ClientDeclined --> Cancelled
    Confirmed --> Cancelled: holds released
    Completed --> [*]
    Cancelled --> [*]
```

The tourist's timeline in the app shows the main path: Submitted, Planning, Quotation sent, Accepted, Confirmed,
In progress, Completed. ClientDeclined, NeedsOperator and Cancelled are shown as side states with a "what's next"
line.

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
    alt valid, or only over budget after the lowest-cost re-plans
        API->>DB: quotation v1 (sent, bestAvailablePrice if still over budget), trip QuotationSent (actor System)
        API->>T: notification "Your quotation is ready" + email
    else Hard rule or agent error
        API->>DB: workflow FailedSafely, trip NeedsOperator, notify managers
        Note over OM: Needs operator tile: Retry planning · Edit & send manually · Cancel
    end
    T->>API: GET /api/notifications/mine (polling) → local notification
    alt Accept
        T->>API: POST /api/quotations/{id}/accept → trip ClientAccepted, notify managers
    else Decline (reason)
        T->>API: POST /api/quotations/{id}/decline → trip ClientDeclined, reason on the dashboard
        OM->>API: POST /api/trip-requests/{id}/replan {note} → Planning
        API->>AG: POST /replan (note + the client's reason)
        AG->>API: proposal → quotation v2, auto-sent, trip QuotationSent
    end
    Note over OM: Accepted — confirm tile: Confirm · Edit & resend
    opt Edit & resend
        OM->>API: PUT …/proposal/days/{n}, PUT …/proposal/resources (from GET /api/availability)
        OM->>API: POST /api/quotations/{id}/calculate (Re-price → new version, not sent)
        OM->>API: POST /api/quotations/{id}/send → trip QuotationSent, tourist "Your quote was updated, please review"
        T->>API: POST /api/quotations/{v2}/accept (Confirm stays disabled until the newest version is accepted)
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

**Budget rule:** with budget USD 400, Validation finds `OVER_BUDGET` (a Soft rule). The Planner re-plans with the
`lowest` cost strategy (budget rooms, the cheapest eligible guide and vehicle, fewer paid entries) up to
`MAX_REPLANS`. If the total is still over the budget, the quotation is sent anyway with
`bestAvailablePrice = true` and the sentence "Best price we can offer — USD X above your budget".

**Safe-failure path:** a Hard rule or an agent error never sends a quote. The trip becomes `NeedsOperator`, the
error summary is shown on the dashboard, and a manager chooses Retry planning, Edit & send manually, or Cancel.
