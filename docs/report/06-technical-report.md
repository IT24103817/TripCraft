# 6. Technical report

TODO: 10–15 pages in total. The outline below is pre-filled from the code; expand each part in your own words.

## 6.1 Request pipeline

`Program.cs`: forwarded headers → exception middleware → status-code pages → Serilog request logging → Swagger →
CORS → authentication → authorization → rate limiter → controllers. Then the startup block runs migrations when
`RUN_MIGRATIONS=true` and the idempotent seeder.

## 6.2 Component A — trip requests and the skeleton

`TripPlanningRules` (pure): masked passport, start not in the past, ≤ 30 days, cities found in the objective in
order, days split across cities (extra days to earlier cities), stops per day by pace (relaxed = 2, otherwise 3).
`TripPlanningService`: load and authorise → status must be Submitted or RevisionRequested (409) → rules (400) →
workflow row + trip status + audit in one save → call the agent service; if it fails, the workflow ends
`FailedSafely` and the trip returns to its previous status.

## 6.3 Workflow integration

- Agent client (`AgentServiceClient`): 10 s per try, one Polly retry, never throws; sets `FailedSafely` itself.
- Internal API: tool endpoints reuse existing services (attractions from Trips; distance, weather, FX from the
  typed clients; availability and rates from `IResourceCatalog`).
- Steps (`WorkflowStepService`): one `agent_steps` row per report, numbered per workflow, summaries ≤ 8,000 characters.
- Proposal (`WorkflowProposalService`): loads database facts → `ProposalValidator` → PendingApproval /
  RevisionRequested (only Soft) / FailedSafely (any Hard) → quotation version → audit.

## 6.4 Deterministic validation (`ProposalValidator`)

Hard: incomplete JSON, unknown attraction/guide/vehicle/hotel/room type, overlapping guide or vehicle hold, rooms
< pax on a night, seats < pax, guide language, 0 or > 3 stops in a day, quotation total more than 1 LKR off the
server recomputation. Soft: over budget. The server recomputes the quotation with the same formula as the
agent's `calculate_quotation` (half-away-from-zero rounding).

## 6.5 Approval transaction (`QuotationApprovalService`)

Holds (guide and vehicle for the whole trip; one per room type and night) → quotation Approved → trip Confirmed →
workflow Completed with the decision in `final_outcome` → approval decision → audit → commit. Any exception:
`DiscardChanges`, rollback, 409. Reject and request-revision (with a comment that goes to the Planner via `/replan`).

## 6.6 Third-party integrations

| Service | Client | Fallback |
|---------|--------|----------|
| open.er-api.com | `ExchangeRateService` (1 h cache) | last known rate flagged `stale`; `FX_FALLBACK_LKR_PER_USD` before any success |
| OpenRouteService | `DistanceService` (city centre = average of attraction coordinates) | seeded `city_distances` table |
| OpenWeatherMap | `WeatherService` | `null` + warning (weather is advisory) |

All: typed `HttpClient`, 5 s per try, one retry; keys from the environment, never logged (the OWM client has
HTTP logging removed because the key is in the URL).

## 6.7 Agent service

See report section 8 and `agents/README.md`: graph, nodes, tools, prompts with the `<DATA>` block, repair loop,
code-enforced rules, callbacks.

## 6.8 Clients

See report section 5.

TODO: code excerpts (with file paths) for one controller → service → repository chain per component, and one
migration.
