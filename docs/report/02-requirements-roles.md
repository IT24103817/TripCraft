# 2. Requirements and roles

## Roles and permissions

| Role | Client | Can do | Cannot do |
|------|--------|--------|-----------|
| Tourist | Flutter | Register, submit trip requests with a passport photo, start planning, see own trips, itinerary, workflow status and quotation, receive status notifications | See other tourists, resources, cost breakdowns; approve |
| Guide | Flutter | Schedule, GPS check-in within 500 m, scan hotel vouchers | Create trips, edit resources, see pricing |
| Operations Manager | React | Trip requests, attractions CRUD, approvals (approve / reject / request revision), workflow monitor, reports | Manage users and roles |
| Admin | React | Create and deactivate users, read workflows | Approve quotations (separation of duties) |

Enforced by the JWT `role` claim, `[Authorize(Roles = …)]` on controllers, a fallback policy requiring
authentication everywhere, and resource-based checks in services (`TripRequestService.EnsureCanAccess`,
`WorkflowQueryService`).

## Functional requirements (by component)

| ID | Requirement | Where |
|----|-------------|-------|
| A1 | Tourist submits a trip request (objective, dates, pax, budget, preferences, nationality, passport) | `POST /api/trip-requests` |
| A2 | Validate dates and passport; extract cities; build a day-by-day skeleton; start the agent workflow | `TripPlanningRules`, `TripPlanningService` |
| A3 | Attractions CRUD with search, filter, sort, paging, soft delete | `AttractionsController`, `AttractionService` |
| A4 | Passport photo upload (JPEG/PNG ≤ 5 MB, private storage) | `PassportPhotoService` |
| B1 | Availability of guides (language, pax), vehicles (seats), rooms (city, night) | TODO Student B (`IResourceCatalog`) |
| B2 | Transactional resource hold with overlap check (409) | TODO Student B (`IResourceHoldService`) |
| C1 | Deterministic validation of every agent proposal | `ProposalValidator` |
| C2 | Approve (one transaction), reject, request revision (re-plan) | `QuotationApprovalService` |
| C3 | Workflow monitor: status, plan, steps, timings | `WorkflowQueryService`, `GET /api/workflows/*` |
| C4 | Quotation list, reports (revenue, utilisation) | TODO Student C |
| X1 | Four agents with contracts, allow-listed tools, safe failure | `agents/app/` |

## Non-functional requirements

| Requirement | How it is met |
|-------------|---------------|
| Security | JWT, roles, PBKDF2 passwords, rate-limited login, CORS, internal key, masked passport numbers (report section 12) |
| Reliability | Timeouts, one retry and fallbacks for third-party APIs; agent timeouts, retries and `FailedSafely` |
| Performance | p95 < 800 ms at 50 concurrent users on the trip list (measured 7.7 ms locally, report section 9) |
| Usability | Four page states on every screen; responsive at 360, 768 and 1280 px (web) and 360×640 / 412×915 (mobile) |
| Cost | Free tiers only (Neon, Render, Vercel, Ollama/Groq) |
