# 4. Database design

PostgreSQL 16 on Neon, EF Core 8 code-first with the snake_case naming convention. ER diagram: section 13
(`docs/diagrams/er.md`), generated from the EF model snapshot.

## Tables (12 today)

| Table | Purpose | Notable constraints / indexes |
|-------|---------|-------------------------------|
| `users` | accounts and role | unique `email` |
| `tourists` | tourist profile | unique `user_id`; `passport_number_masked` (last 4) |
| `trip_requests` | the tourist's request | check `end_date >= start_date`, check `pax > 0`; index `(status, start_date)`; `preferences jsonb` |
| `attractions` | catalogue | index `city`; soft delete `is_deleted` |
| `itineraries`, `itinerary_days`, `itinerary_stops` | the day-by-day plan | unique `trip_request_id`; unique `(itinerary_id, day_number)`; unique `(itinerary_day_id, sequence)` |
| `agent_workflows` | one agent run | index `trip_request_id`, `(status, started_at)`; `plan`, `final_outcome`, `validation_result` jsonb |
| `agent_steps` | one row per agent step | unique `(workflow_id, step_no)`; summaries jsonb |
| `audit_logs` | before/after of every change | no FKs on purpose |
| `city_distances` | fallback distance table | unique `(from_city, to_city)`; check distance and duration > 0 |

All ids are `uuid`; all tables have `created_at`/`updated_at timestamptz` (set in `AppDbContext.SaveChangesAsync`);
money is `numeric(12,2)`. Tables of Students B and C (guides, vehicles, hotels, room types, resource holds, rate
card, quotations, quotation lines, approval decisions) are designed in PLAN.md section 4 — TODO when merged.

## Migrations and seed

`InitialCreate` → `AddTripRequests` → `AddAgentWorkflowsAndAuditLogs` → `AddAgentWorkflows`. The seeder adds
3 users per role, 8 attractions (Colombo, Kandy, Ella, Galle), one tourist profile per seeded tourist, one
completed sample trip with its itinerary, and 6 city distances — each only if its table is empty.

## Transactions

- Start planning: workflow row + trip status + audit in one `SaveChanges`.
- Proposal: validation result, status, quotation version and audit in one `SaveChanges`.
- Approve: explicit transaction — holds → quotation → trip → workflow → decision → audit → commit; any failure
  rolls back (proved on real PostgreSQL in `Tests/Shared/Database/ApprovalTransactionPostgresTests.cs`).

## Schema decision for agent state

Hybrid relational + jsonb (ADR-004).

TODO: screenshot of the Neon Tables view after migration.
