# Per-student ownership summary

Compiled on 6 October 2026 from the master plan (`TripCraft — SE3090 Assignment 1 Master Plan.md`, sections 3, 5 and
13), the README's *Component ownership* table, `docs/COMPLIANCE.md`, the code on `main` (commit `14878cc`, tag
`v1.1.1`) and the three student branches of the group repository `https://github.com/ilhamhilmy63/TripCraft`
(fetched the same day). Test counts were measured on `main` with each tool's own test listing, not estimated.

## 1. Who owns what

| Student ID | GitHub username | Letter | Component | Agent(s) owned | Third-party integration | ADRs |
|---|---|---|---|---|---|---|
| IT24103652 | `ilhamhilmy63` (Ilham Hilmy; group leader) | A | Trip Requests & Itinerary Management | Planner / Coordinator; writes the shared Itinerary Analysis agent | OpenWeatherMap (weather per itinerary day) | ADR-002 Flutter state management, ADR-003 Agentic AI framework |
| IT24103817 | `IT24103817` (Ibrahim M A) | B | Resource Management (guides, vehicles, hotels) | Resource & Action; reviews Itinerary Analysis | OpenRouteService (city distances; `api.heigit.org/openrouteservice`) | ADR-005 Cloud deployment platform, ADR-006 LLM provider |
| IT24103079 | `inzam2659-pixel` | C | Quotation, Approval & Reporting | Validation & Safety | open.er-api.com (USD→LKR exchange rate) | ADR-001 React state management, ADR-004 Agent workflow state schema |

**How this was confirmed.** The README table gives letters, components, agents and third parties, but no student
IDs or usernames, and the report cover's member table is still a TODO. The ID-to-letter mapping comes from the
branches themselves: each branch's README names its letter ("TripCraft — Student A Component" on `IT24103652`,
"Student B Component" on `IT24103817`, "Student C contribution" on `IT24103079`), and each branch tip contains that
component's files. GitHub usernames are the accounts GitHub links to the commits on each branch (via the API).
ADR authors are from plan section 13 and the `Author:` line of each ADR on `main`; they agree.

| Branch | Commits (GitHub account) | Tip content | Compared with `main` |
|---|---|---|---|
| `IT24103652` | 8, all `ilhamhilmy63` (28 Sep) | Student A only: backend `Trips`, agents `planner.py` + `itinerary.py`, web `features/trips`, mobile `features/trips`, their tests, ADR-002/003 | 346 files: 220 identical, 112 differ (older v1.0 versions), 0 only on the branch apart from 14 pre-refactor web component files |
| `IT24103817` | 36, all `IT24103817` (28 Sep – 1 Oct) | Student B only: backend `Resources`, agents `resources.py`, web `features/resources`, mobile `features/resources`, their tests, ADR-005/006 | 332 files: 216 identical, 96 differ, 6 only on the branch (v1.0 screens later replaced on `main`) |
| `IT24103079` | 72 `inzam2659-pixel`, 8 `ilhamhilmy63`, 5 `IT24103817` | Student C only (`docs/student-c-files.txt`, 193 paths): backend `Quotations` + `Workflows` + `Controllers/Internal`, agents `validation.py` + golden tests, web `features/quotations`, mobile `features/quotations`, ADR-001/004 | 196 files: 104 identical, 67 differ, 25 only on the branch (see discrepancy 5) |

### Where the branches and the README disagree

These are stated as found; nothing here is a judgement about who did the work.

1. **Authorship on `main` does not follow the ownership table.** Of the 43 commits on `main`, 39 are by
   `IT24103817`, including the full Student A component, the "Resource Management component (draft for Student B to
   review and adopt)" and the "Quotation, Approval & Reporting component (draft for Student C to review and adopt)";
   2 are by `ilhamhilmy63` (the initial commit and the project rename) and 2 by `inzam2659-pixel` (a scheduled CI
   workflow, since removed). `docs/COMPLIANCE.md` already says B's and C's components were first written as drafts
   for those students to review and adopt. Each student's own branch is the record of their contribution.
2. **Student A's component also appears in Student B's branch history.** `IT24103817` contains 16 commits of the
   Trip Requests & Itinerary component (8 on 28 Sep, 8 on 1 Oct) authored by `IT24103817`, later removed by
   "chore: remove Student A trip files; this branch now holds Student B". The branch tip holds only Student B's
   work. Student A's own branch `IT24103652` has 8 commits by `ilhamhilmy63` with the same scope.
3. **Resource Management code appears in Student C's branch history, authored by Student A's account.** The 8
   `ilhamhilmy63` commits on `IT24103079` (the repository's initial commit of 24 Sep and 7 on 26 Sep) include "feat(resources): add guides vehicles hotels and
   availability", "add availability validation rules" and "create transactional resource holds" (Student B's
   component), plus solution and auth scaffolding. They are history only; the branch tip holds no Resources files.
   The 5 `IT24103817` commits on that branch are the shared skeleton/auth commits it was started from, also on `main`.
4. **Shared files are claimed by Student C's file list.** `docs/student-c-files.txt` lists the audit log
   (`Common/Auditing/*`, the README table gives the `AuditLog` entity to C) and `IUnitOfWork`, the shared
   `validate_schema` tool, and the ports in `Application/Workflows` that the other components implement
   (`IResourceCatalog`, `IResourceHoldService`, `IDistanceService`, `IWeatherService`). The same files are on
   Student A's and Student B's branches. On `main` the audit log is used by every component, so this summary lists
   it under the shared foundation, and the ports as C's interfaces implemented by A (weather) and B (resources,
   distance).
5. **Student C's branch has work that is not on `main`.** 25 files: 10 quotation/report test files and 2 exchange-rate
   test files published on 2 October (for example `DecisionCommentBoundaryTests`, `ReportCalendarBoundaryTests`,
   `UtilisationOrderingTests`, `ExchangeRateRecoveryTests`), the `status_watcher` mobile service with its test, and
   the v1.0 approval flow (`QuotationApprovalService`, `QuotationApprovalsController`, `DecisionActions`,
   `WorkflowDetailPage`), which `main` replaced in v1.1. The new tests have not been merged into `main`.
6. **The approval gate moved in v1.1, and the README's "Features per component" row is out of date.** It still says
   Student C's business operation is "approve / reject / revise in one transaction". On `main` the human approval
   gate is **Confirm** (`TripConfirmationService` in C's `Quotations`), exposed as
   `POST /api/trip-requests/{id}/confirm` in `TripRequestsController`, which sits in Student A's `Controllers/Trips`
   folder. The same controller also serves C's proposal-edit and plan-explanation endpoints.
7. **v1.1 features are not in the README ownership table.** Mood packages, guide accounts, guide change requests,
   vouchers, the availability grid, the dashboard, notifications, settings and email were added after the table was
   written. This summary places each one by the folder and service it lives in; vouchers span B (scan at
   check-in) and C (issued by Confirm).

## 2. Each student's component on `main`

Paths are relative to the repository root. "(op)" marks the business operation beyond CRUD from plan section 3.

### Student A — Trip Requests & Itinerary (IT24103652, `ilhamhilmy63`)

| Layer | Folders and key files |
|---|---|
| Backend | `backend/src/TripCraft.Application/Trips/` (51 files: `TripRequest`, `Tourist`, `Attraction`, `Itinerary`, `ItineraryDay`, `ItineraryStop`, `TripTemplate`, `TripStatusMachine`, `Planning/TripPlanningRules`, `Services/TripRequestService`, `TripPlanningService`, `ItineraryEditService`, `PassportPhotoService`, `Templates/TripTemplateService`, 7 validators); `backend/src/TripCraft.Infrastructure/Trips/` (13: configurations, repositories, `TripsSeeder`, `TripTemplatesSeeder`, `LocalPassportPhotoStore`); `backend/src/TripCraft.Api/Controllers/Trips/`; `Infrastructure/External/WeatherService.cs` |
| Migrations | `AddTripRequests`; trip templates in `AddTripTemplatesAndGuideRatings` (shared with B) |
| Agents | `agents/app/nodes/planner.py`, `agents/app/nodes/itinerary.py` (B reviews); tools `parse_dates`, `list_agents`, `get_attractions`, `get_distance`, `get_weather` |
| Web | `web/src/features/trips/` (16 files) |
| Mobile | `mobile/lib/features/trips/` (41 files) |

**Endpoints** (Trips controllers):
- `POST /api/trip-requests`, `GET /api/trip-requests`, `GET /api/trip-requests/{id}`, `PUT /api/trip-requests/{id}`
- **(op)** `POST /api/trip-requests/{id}/start-planning`: validates passport and dates, builds the day-by-day
  itinerary skeleton and starts the agent workflow
- `POST /api/trip-requests/{id}/cancel`, `GET /api/trip-requests/{id}/cancellation`,
  `GET /api/trip-requests/{id}/history`, `GET /api/trip-requests/{id}/itinerary`,
  `PUT /api/trip-requests/{id}/itinerary/days/{n}`, `POST /api/trip-requests/{id}/passport-photo`,
  `GET /api/trip-requests/{id}/workflow`
- `GET /api/attractions`, `GET /api/attractions/cities`, `GET /api/attractions/{id}`, `POST /api/attractions`,
  `PUT /api/attractions/{id}`, `DELETE /api/attractions/{id}`
- `GET /api/tourists/me`, `POST /api/tourists/me/passport-photo`
- `GET /api/trip-templates`, `GET /api/trip-templates/{id}`, `POST /api/trip-templates/{id}/book`

**React screens:** `TripsListPage` (trip request list; its Quotations tab is C's `TripQuotationsList`),
`TripDetailPage` (status timeline, itinerary day editor, map, history), `AttractionsPage` (attraction CRUD).

**Flutter screens:** `tourist_home_screen` (mood packages), `package_detail_screen` with the Book sheet,
`new_trip_screen` (trip request form), `my_trips_screen`, `trip_detail_screen` (overview, itinerary with map and
weather, vouchers tab, itinerary PDF).

**Device features:** camera and gallery for the passport photo (`image_picker`), date-range picker, map
(`flutter_map`), PDF share sheet (`share_plus`), QR display of vouchers (`qr_flutter`).

**Tests (on `main`):**
- Backend `Tests/Trips`: **143** tests in 17 classes (largest: `TripStatusMachineTests` 35, `TripRequestsEndpointsTests` 21, `TripPlanningRulesTests` 13, `TripHistoryAndCancelTests` 11, `TripsModelConfigurationTests` 10, `TripsValidatorTests` 9); plus `WeatherServiceTests` 6 and `ItineraryEditPostgresTests` 1.
- Agents: `test_planner.py` 6, `test_itinerary.py` 4, and 2 of the 3 tests in `test_lowest_cost.py` (fewer paid entries).
- Web `features/trips/__tests__`: **45** (`TripDetailPage` 23, `ItineraryDayEditor` 14, `TripsListPage` 6, `AttractionForm` 2).
- Mobile `test/trips`: **89** in 17 files (`itinerary_pdf` 12, `trip_detail` 11, `whats_next` 7, `tourist_home` 7, `weather` 6, `trip_progress` 6, `trip_tabs` 6, …).

### Student B — Resource Management (IT24103817, `IT24103817`)

| Layer | Folders and key files |
|---|---|
| Backend | `backend/src/TripCraft.Application/Resources/` (35 files: `Guide`, `GuideLanguage`, `Vehicle`, `Hotel`, `RoomType`, `RateCardEntry`, `ResourceHold`, `StopCheckIn`, `GuideChangeRequest`, `GuideRating`, `AvailabilityRules`, `Services/AvailabilityService`, `ResourceHoldService`, `AvailabilityGridService`, `GuideScheduleService`, `GuideChangeService`, `ResourceCatalog`, 4 validator files); `backend/src/TripCraft.Infrastructure/Resources/` (12: configurations, `ResourceRepository`, `ResourcesSeeder`); `backend/src/TripCraft.Api/Controllers/Resources/`; `Infrastructure/External/DistanceService.cs` |
| Migrations | `AddResourceManagement` (with the btree_gist no-overlap constraint on `resource_holds`); guide ratings in `AddTripTemplatesAndGuideRatings` |
| Agents | `agents/app/nodes/resources.py` (`pick_guide`, `cheapest_vehicle`, `suggest_rooms`); tools `check_guide_availability`, `check_vehicle_availability`, `check_room_availability`, `get_rate_card` |
| Web | `web/src/features/resources/` (25 files) |
| Mobile | `mobile/lib/features/resources/` (17 files) |

**Endpoints** (Resources controllers):
- Guides: `GET /api/guides`, `GET /api/guides/{id}`, `POST /api/guides` (creates the guide's account with a
  temporary password), `PUT /api/guides/{id}`, `DELETE /api/guides/{id}`, `POST /api/guides/{id}/reset-password`,
  `GET /api/guides/{id}/schedule`, `GET /api/guides/me/schedule`
- Vehicles: `GET /api/vehicles`, `GET /api/vehicles/{id}`, `POST /api/vehicles`, `PUT /api/vehicles/{id}`,
  `DELETE /api/vehicles/{id}`
- Hotels: `GET /api/hotels`, `GET /api/hotels/{id}`, `POST /api/hotels`, `PUT /api/hotels/{id}`,
  `DELETE /api/hotels/{id}`, `GET|POST /api/hotels/{id}/room-types`, `PUT|DELETE /api/hotels/{id}/room-types/{roomTypeId}`
- **(op)** `GET /api/availability`: what is free for the dates, language, seats and rooms, with rates
- **(op)** `POST /api/resource-holds`: transactional hold; overlapping holds are rejected with 409
- `GET /api/availability/grid`, `GET /api/resource-holds`, `GET|PUT /api/resource-holds/{id}`,
  `POST /api/resource-holds/{id}/release`
- `POST /api/check-ins` (GPS within 500 m, or a scanned trip voucher)
- `POST /api/trip-requests/{id}/guide-change-requests`, `GET /api/guide-change-requests`,
  `POST /api/guide-change-requests/{id}/resolve`
- `GET /api/trip-requests/{id}/assignment`, `GET|POST /api/trip-requests/{id}/guide-rating`

**React screens:** `GuidesPage` (with the temporary-password dialog), `VehiclesPage`, `HotelsPage` (hotel form with
room types), `AvailabilityPage` (availability grid with manual blocks), `GuideChangeRequestsPanel` on the dashboard.

**Flutter screens:** `guide_home_screen` (today's trip and searchable schedule), `trip_day_screen` (vehicle card,
check-in panel, request replacement), `qr_scan_screen` (voucher scan).

**Device features:** GPS check-in (`geolocator`), QR code scanning with the camera (`mobile_scanner`).

**Tests (on `main`):**
- Backend `Tests/Resources`: **70** tests in 12 classes (`AvailabilityRulesTests` 12, `GuidesEndpointsTests` 10, `VehiclesAndHotelsEndpointsTests` 9, `ResourceValidatorsTests` 6, `GuideScheduleAndCheckInTests` 5, `ResourceHoldServiceTests` 5, `VoucherCheckInTests` 5, …); plus `ResourceHoldConstraintPostgresTests` 4, `DistanceServiceTests` 6 and the 5 OpenRouteService cases in `ExternalServicesSetupTests`.
- Agents: `test_resources.py` 12 and 1 of the 3 tests in `test_lowest_cost.py` (cheapest vehicle).
- Web `features/resources/__tests__`: **22** (`AvailabilityPage` 5, `GuidesPage` 5, `HotelForm` 4, `EmptyStates` 3, `GuideChangeRequestsPanel` 3, `VehiclesAndHotelsPages` 2).
- Mobile `test/resources`: **34** in 6 files (`guide_home` 13, `voucher_scan` 8, `check_in` 5, `request_replacement` 3, `voucher_lookup` 3, `vehicle_card` 2).

### Student C — Quotation, Approval & Reporting (IT24103079, `inzam2659-pixel`)

| Layer | Folders and key files |
|---|---|
| Backend | `backend/src/TripCraft.Application/Quotations/` (22 files: `Quotation`, `QuotationLine`, `ApprovalDecision`, `QuotationCalculator`, `TripConfirmationService`, `OperatorQuotationService`, `QuotationClientService`, `Services/QuotationService`, `QuotationStore`, `Reports/ReportService`, `Dashboard/DashboardService`, `Documents/TripDocumentService`); `backend/src/TripCraft.Application/Workflows/` (39: `AgentWorkflow`, `AgentStep`, **`ProposalValidator`**, `ProposalFactsLoader`, `Services/WorkflowProposalService`, `WorkflowStepService`, `WorkflowQueryService`, `ProposalEditService`, `PlanExplanationService`, `Ports/`, `External/` interfaces); `backend/src/TripCraft.Infrastructure/Quotations/` (5), `Infrastructure/Workflows/` (7: `AgentServiceClient`, configurations, `WorkflowsSeeder`); `Controllers/Quotations/`, `Controllers/Workflows/`, `Controllers/Internal/`; `Infrastructure/External/ExchangeRateService.cs` |
| Migrations | `AddAgentWorkflowsAndAuditLogs`, `AddAgentWorkflows`, `AddQuotations`, `QuotationsGoStraightToClient`, `RetireRevisionRequested` |
| Agents | `agents/app/nodes/validation.py`; tools `calculate_quotation`, `get_fx_rate`, `validate_schema`, `check_business_rules`; golden evaluation cases `agents/tests/golden/` |
| Web | `web/src/features/quotations/` (36 files) |
| Mobile | `mobile/lib/features/quotations/` (13 files) |

**Endpoints:**
- `GET /api/quotations`, `GET /api/quotations/{id}`
- **(op)** `POST /api/quotations/{id}/calculate`: full quotation calculation (vehicle km rate × distance + guide day
  rate × days + rooms × nights + margin), LKR→USD
- `POST /api/quotations/{id}/payment` (deposit), `POST /api/quotations/{id}/accept`, `POST /api/quotations/{id}/decline`
- `POST /api/quotations/{id}/send`, `POST /api/trip-requests/{id}/replan`, `POST /api/trip-requests/{id}/proposal/reprice`
- **(op, human approval gate)** `POST /api/trip-requests/{id}/confirm`: one transaction places the holds, saves the
  itinerary, issues the vouchers and queues the email (served by `TripConfirmationService`; the route is in
  `TripRequestsController`)
- `GET /api/trip-requests/{id}/plan-explanation`, `PUT /api/trip-requests/{id}/proposal/days/{n}`,
  `PUT /api/trip-requests/{id}/proposal/resources` (same controller, C's services)
- `GET /api/reports/revenue`, `GET /api/reports/utilisation`, `GET /api/reports/trips-by-status`
- `GET /api/dashboard/actions`, `GET /api/dashboard/attention`, `GET /api/dashboard/upcoming`
- `GET /api/workflows`, `GET /api/workflows/{id}`, `GET /api/workflows/{id}/steps`
- Internal API for the agents (`X-Internal-Key`): `GET /api/internal/attractions | distance | weather |
  availability/guides | availability/vehicles | availability/rooms | rates (alias rate-card) | fx-rate`,
  `POST /api/internal/workflows/{id}/steps`, `POST /api/internal/workflows/{id}/proposal`

**React screens:** `ApprovalsPage` (Accepted / Declined / Needs operator review queue), `ApprovalReviewPage` (Why this
plan, version comparison, validation checklist), `AgentRunsPage` and `AgentRunDetailPage` (agent workflow monitor
with step timeline), `ReportsPage` (reports dashboard), and on the trip page the `TripActions` panel (Confirm,
Edit & resend, Replan with note), `TripQuotationTab` and the Quotations tab list; the dashboard's action tiles.

**Flutter screens:** `quotation_screen` (quotation in LKR/USD, accept or decline with a reason),
`notifications_screen`; the quotation decision card on the trip screen.

**Device features:** local phone notifications (`flutter_local_notifications`) from 30-second polling.

**Tests (on `main`):**
- Backend `Tests/Quotations`: **63** tests in 8 classes (`QuotationApprovalTests` 12, `TripConfirmationServiceTests` 11, `QuotationCalculatorTests` 10, `OperatorQuotationServiceTests` 8, `QuotationStatusCodeTests` 7, `DashboardAndDocumentsTests` 6, …); `Tests/Workflows`: **67** in 11 classes (`ProposalValidatorTests` 15, `InternalEndpointsTests` 11, `WorkflowValidatorsTests` 9, …); plus `ExchangeRateServiceTests` 5, `AgentServiceClientTests` 5, and 8 PostgreSQL tests (`ApprovalTransaction` 2, `ApprovalWithRealResources` 1, `ProposalSave` 2, `QuotationConstraint` 2, `AuditLogReader` 1).
- Agents: `test_validation.py` 10 and the golden cases in `tests/golden/` (17 tests in 7 files).
- Web `features/quotations/__tests__`: **70** (`TripActions` 17, `ApprovalReviewPage` 13, `reviewRules` 12, `TripQuotationsList` 6, `TripQuotationTab` 5, …).
- Mobile `test/quotations`: **30** in 4 files (`quotation_screen` 10, `notifications_poller` 7, `notifications_screen` 7, `quotation_decision` 6).
- Not on `main` yet: the 12 extra test files on branch `IT24103079` (discrepancy 5).

## 3. The agentic subsystem (the fourth component)

A LangGraph service in Python (`agents/`), called only by the ASP.NET Core API with the `X-Internal-Key` header.
Every agent answers in JSON validated by a Pydantic schema (`agents/app/schemas.py`), may call only the tools on its
allow-list (`agents/app/tools/registry.py`), and never holds a resource. Locally the model is Ollama `llama3.1:8b`;
hosted it is Groq (`qwen/qwen3.8-27b`), see ADR-006.

| Agent | Owner | Responsibility | Input contract | Output contract | Allowed tools |
|---|---|---|---|---|---|
| Planner / Coordinator (`nodes/planner.py`) | **Student A** | Turns the objective into an ordered plan, delegates each step, re-plans on a budget violation (lowest-cost strategy) or an operator's note | `PlannerInput {objective, start_date, end_date, pax, budget_usd, preferences}` | `PlannerOutput {plan, constraints{cities, guide_language, transport_preference, hotel_tier, max_stops_per_day, cost_strategy}}` | `parse_dates`, `list_agents` |
| Itinerary Analysis (`nodes/itinerary.py`) | **Student A** writes, Student B reviews | Picks 1–3 attractions per day, travel order, road or train, ≤ 4 h driving a day | `ItineraryInput {plan, cities, dates, pax, preferences}` | `ItineraryOutput {days[{day, city, stops[], transport}]}` | `get_attractions`, `get_distance`, `get_weather` |
| Resource & Action (`nodes/resources.py`) | **Student B** | Proposes an available guide with the language, a vehicle with enough seats and rooms per night; proposes, never holds | `ResourceInput {days, pax, language, dates}` | `ResourceActionOutput {guide_id, vehicle_id, rooms[{hotel_id, room_type_id, night}], gaps[]}` | `check_guide_availability`, `check_vehicle_availability`, `check_room_availability`, `get_rate_card` |
| Validation & Safety (`nodes/validation.py`) | **Student C** | Deterministic checks, then a compliance verdict; the model's concerns are advisory only, the status comes from code | `ValidationInput {days, resources, quotation_draft, budget_usd}` | `ValidationSafetyOutput {valid, violations[], quotation_final}` | `calculate_quotation`, `get_fx_rate`, `validate_schema`, `check_business_rules` |

- **Shared foundation of the agent service (group work):** the graph and its budget re-plan loop (`app/graph.py`,
  on no student branch), the Pydantic schemas (`app/schemas.py`), the tool registry and allow-list
  (`app/tools/registry.py`), the LLM factory with the repair loop and 429/503 backoff (`app/llm.py`), the shared
  node helpers and prompt-injection guard (`app/nodes/common.py`), the FastAPI app, config and the internal-key
  check, and the live real-model suite (`tests/live/`).
- **Deterministic validation in C# — Student C:** `backend/src/TripCraft.Application/Workflows/ProposalValidator.cs`
  checks every plan section 5 rule (ids exist, no hold overlaps, rooms ≥ pax, seats ≥ pax, guide language, 1–3
  stops a day, ≤ 4 h driving, quotation matches the server calculation, over budget as the only Soft rule) and is
  what decides whether a proposal is sent to the client.
- **Human approval gate — Student C's Quotations component:** **Confirm** (`TripConfirmationService`), a
  manager-only action in React, offered only after the client accepted the newest quotation version. It is the only
  place guide, vehicle and rooms are held.
- **Tests:** agents total **87** in CI (planner 6, itinerary 4, resources 12, lowest cost 3, validation 10, golden
  17, shared: auth 6, llm 5, Gemini 15, Groq 5, registry 4), plus **6** live real-model tests run on purpose.

## 4. Shared foundation (group work)

| Area | Where |
|---|---|
| Identity: users, roles, JWT, login rate limit, guide accounts' password change | `Application/Identity/` (25 files), `Infrastructure/Identity/` (4), `Controllers/Identity/AuthController`, `Controllers/Admin/AdminUsersController`; tests `Tests/Identity` 22 |
| Common: base entity, exceptions, paging, auditing, settings, notifications, email outbox | `Application/Common/` (30 files); tests `Tests/Common` 32 |
| Persistence: `AppDbContext` (also the unit of work), migrations, seeding, connection-string parsing | `Infrastructure/Persistence/` (38 files); `InitialCreate`, `V11Lifecycle`, `AddSettingsAndDeposits`, `AllowGeminiProvider` migrations; PostgreSQL tests `Tests/Shared/Database` (constraints 8, migrations 3, startup 2, health 2) |
| API middleware and setup: ProblemDetails error handling, Swagger, CORS, rate limiting, authorization policies, health | `Api/Middleware/`, `Api/Setup/`, `Api/Authorization/`, `HealthController` |
| Audit log | `AuditLog` entity and `/api/admin/audit-logs` (React `AuditLogPage`) — see discrepancy 4 |
| Settings (v1.1) | `/api/admin/settings`, React `SettingsPage` (LLM provider, cancellation notice, margin, deposit) |
| Notifications (v1.1) | `/api/notifications/*`, web `NotificationBell`, Flutter polling and local notifications |
| Email (v1.1, fourth third-party integration) | Mailtrap sandbox with the `.eml` pickup-folder fallback (`MailtrapEmailSender`) |
| Vouchers (v1.1) | `Application/Vouchers/` and `/api/trips/{id}/vouchers`, `/vouchers.pdf`, `/itinerary.pdf`, `/api/vouchers/verify`; issued by C's Confirm, scanned by B's check-in; tests `Tests/Vouchers` 11 |
| Web shell | `web/src/app` (layout, dashboard page, routing), `web/src/auth` (login, guards, users, audit log, settings, mobile-app page), `web/src/shared` (Hallmark components, statuses, API client); landing page `web/src/features/landing`; web tests outside the three features: 67 |
| Mobile shell | `mobile/lib/core` (API client, auth, router, secure storage), `mobile/lib/shared` (theme, widgets, statuses), login, register, change password; mobile tests outside the three features: 33 |
| CI | `.github/workflows/backend-ci.yml`, `web-ci.yml`, `mobile-ci.yml`, `agents-ci.yml` |
| Deployment and docs | `render.yaml`, Dockerfiles, `web/vercel.json`, `docs/DEPLOYMENT.md`, `docs/DEPLOY-CHECKLIST.md`, diagrams, report sources, `docs/COMPLIANCE.md` |

Totals on `main`: backend 475, agents 87 (+6 live), web 204, mobile 186.

## 5. The four components in plain English

1. **Trip Requests & Itinerary (Student A).** A tourist opens the app, either books one of the ready-made mood
   packages or describes the trip they want: dates, cities, travellers, budget and a passport photo taken with the
   camera. The system checks the dates and passport details, lays out a day-by-day skeleton of the trip and hands
   it to the planning agents. The tourist then follows the trip on a timeline, sees the itinerary on a map with the
   weather for each day, and keeps the itinerary PDF and vouchers on the phone. Staff manage the attractions and
   can adjust any day of an itinerary.
2. **Resource Management (Student B).** The operator keeps the catalogue of guides, vehicles and hotels with their
   room types and prices, and gives each guide an account. Before anything is promised to a tourist, the system can
   say exactly which guide, vehicle and rooms are free on those dates, and when a booking is confirmed it reserves
   them in one step that refuses any double booking. Guides see their schedule on the phone, check in at each stop
   by scanning the tourist's voucher or by GPS, and can ask for a replacement.
3. **Quotation, Approval & Reporting (Student C).** Every proposal from the agents is checked by fixed rules in
   code and priced line by line in rupees and dollars. A proposal that passes is sent to the tourist straight away;
   the tourist accepts or declines it with a reason. The operations manager then confirms the accepted quotation,
   the single human approval step, which books the resources, issues the vouchers and emails the tourist. Managers
   can edit and resend a quote, re-plan a declined one, follow every agent run step by step, and read revenue and
   utilisation reports.
4. **Agentic AI subsystem (A, B and C, one agent each, on a shared foundation).** Four cooperating AI agents turn a
   tourist's request into a complete proposal: the Planner breaks the request into steps, the Itinerary agent picks
   the places to visit each day, the Resource agent proposes a guide, vehicle and rooms, and the Validation agent
   prices the trip and checks it against the rules. Each agent may only use its own approved tools, must answer in
   a fixed format, and can never book anything; if the trip is over budget it re-plans at the lowest cost, and if
   anything fails it stops safely and hands the trip to the operator.

*Note:* the request asked for these paragraphs "in the same style as the report's four numbered items". The
repository's report sources do not contain such a numbered list (the closest is the "Solution" paragraph in
`docs/report/01-overview-scope.md`), so the paragraphs follow plan section 3's framing of one component per student
plus the agentic subsystem.
