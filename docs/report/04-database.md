# 4. Database design

PostgreSQL 16 on Neon, EF Core 8 code-first with the snake_case naming convention. ER diagram: section 13
(`docs/diagrams/er.md`), generated from the EF model snapshot.

## Tables (22)

Owner: A = Trip Requests & Itinerary, B = Resource Management, C = Quotation, Approval & Reporting, Shared = used by
all components (see the README ownership table).

| Table | Owner | Purpose | Notable constraints / indexes |
|-------|-------|---------|-------------------------------|
| `users` | Shared | accounts and role | unique `email` |
| `tourists` | A | tourist profile | unique `user_id`; `passport_number_masked` (last 4) |
| `trip_requests` | A | the tourist's request | check `end_date >= start_date`, check `pax > 0`; index `(status, start_date)`; `preferences jsonb` |
| `attractions` | A | catalogue of places to visit | index `city`; soft delete `is_deleted` |
| `itineraries` | A | the plan of one trip | unique `trip_request_id` |
| `itinerary_days` | A | one day of the plan, with city and hotel | unique `(itinerary_id, day_number)`; `hotel_id` FK to `hotels` |
| `itinerary_stops` | A | one attraction visit in a day | unique `(itinerary_day_id, sequence)` |
| `guides` | B | tour guides and their day rate | unique `user_id` (optional login); check `day_rate_lkr > 0`, `max_pax > 0` |
| `guide_languages` | B | languages a guide speaks | unique `(guide_id, language_code)`; index `language_code` |
| `vehicles` | B | vans, cars and coaches | unique `registration_no`; check `seats > 0`, `rate_per_km_lkr > 0` |
| `hotels` | B | hotels with city, stars and location | index `city`; check `star_rating` 1 to 5 |
| `room_types` | B | room types of a hotel with rate and room count | unique `(hotel_id, name)`; check capacity, total rooms and rate > 0 |
| `rate_cards` | B | operator margin, effective from a date | unique `effective_from`; check margin 0 to 100 |
| `resource_holds` | B | a guide, vehicle or room reserved for dates | index `(resource_type, resource_id, from_date, to_date)`; `btree_gist` exclusion constraint: no overlapping `Held` holds of one guide or vehicle |
| `stop_check_ins` | B | a guide's GPS check-in at a stop | unique `itinerary_stop_id`; check `distance_meters >= 0` |
| `quotations` | C | a priced version of a trip, in LKR and USD | unique `(trip_request_id, version)`; index `(status, created_at)`; checks on version, totals and FX rate |
| `quotation_lines` | C | one priced line (guide, vehicle, room or entry) | check `line_type`; check qty > 0 and amounts >= 0 |
| `approval_decisions` | C | a manager's approve / reject / revise decision | index `quotation_id`; FK `decided_by` to `users` |
| `agent_workflows` | Shared | one agent run | index `trip_request_id`, `(status, started_at)`; `plan`, `final_outcome`, `validation_result` jsonb |
| `agent_steps` | Shared | one row per agent step | unique `(workflow_id, step_no)`; summaries jsonb |
| `audit_logs` | Shared | before/after of every change | no FKs on purpose |
| `city_distances` | Shared | fallback distance table | unique `(from_city, to_city)`; check distance and duration > 0 |

All ids are `uuid`; all tables have `created_at`/`updated_at timestamptz` (set in `AppDbContext.SaveChangesAsync`);
money is `numeric(12,2)`.

## Migrations and seed

`InitialCreate` → `AddTripRequests` → `AddAgentWorkflowsAndAuditLogs` → `AddAgentWorkflows` →
`AddResourceManagement` → `AddQuotations`. The seeders add:
- 3 users per role;
- 21 attractions in six cities (Colombo 2, Kandy 4, Ella 4, Galle 5, Nuwara Eliya 3, Sigiriya 3);
- 4 guides, 3 vehicles, a 15 % rate card, and 6 hotels (one per city) with 2 room types each;
- one tourist profile per seeded tourist;
- one completed sample trip with its itinerary and quotation;
- 15 city distances.

Users are seeded only when the `users` table is empty. Guides, vehicles and the rate card are seeded only on
the first run (when the `guides` table is empty). Attractions (by name), hotels (by id) and city distances (by city
pair) are topped up on every start, so an older database picks up new seed rows without duplicates. The full list
is in `docs/diagrams/er.md` (Seed data).

## Transactions

- Start planning: workflow row + trip status + audit in one `SaveChanges`.
- Proposal: validation result, status, quotation version and audit in one `SaveChanges`.
- Approve: explicit transaction — holds → quotation → trip → workflow → decision → audit → commit; any failure
  rolls back (proved on real PostgreSQL in `Tests/Shared/Database/ApprovalTransactionPostgresTests.cs`).

## Schema decision for agent state

Hybrid relational + jsonb (ADR-004).

TODO: screenshot of the Neon Tables view after migration.
