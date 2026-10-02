# TripCraft v1.1 — staff web additions (API contract)

This adds to [API-V11.md](API-V11.md). All JSON is camelCase, errors are ProblemDetails, and dates are `yyyy-MM-dd`.
"Manager" means OperationsManager.

## Dashboard (manager)

- `GET /api/dashboard/actions` →
  `{acceptedToConfirm, declinedNeedsDecision, needsOperator, guideChangeRequests, recentCancellations}`.
  Each value is an integer:
  - `acceptedToConfirm`: trips in ClientAccepted (tile "Accepted — confirm").
  - `declinedNeedsDecision`: trips in ClientDeclined (tile "Declined — needs a decision").
  - `needsOperator`: trips in NeedsOperator (tile "Needs operator").
  - `guideChangeRequests`: open requests.
  - `recentCancellations`: trips cancelled in the last 7 days.
- `GET /api/dashboard/attention?status=` → the trips behind the first three tiles, with `detail` (the client's
  decline reason, the error summary, or "Version N accepted"). See [API-V11.md](API-V11.md).
- `GET /api/dashboard/upcoming` → trips running today or tomorrow (Confirmed or InProgress), in the operator's time
  zone. Each item is
  `{tripRequestId, objective, startDate, endDate, pax, status, touristName, guideName, vehicleRegistrationNo,
  vehicleType, day: "today"|"tomorrow", dayNumber}`.

Link targets for the web: trips list filtered by status (`/trips?status=ClientAccepted`), the review queue (Accepted / Declined / Needs operator tabs), and the
dashboard's guide-change panel.

## Why this plan (review page)

`GET /api/trip-requests/{id}/plan-explanation` (manager) →
`{items: [{topic: "guide"|"vehicle"|"hotels"|"driving"|"budget", title, text}]}`.

The text is plain language, built from the proposal, the database and the agent step summaries. For example:
"Nimal Perera was chosen: the cheapest free English-speaking guide for 4 people (LKR 6,000 a day)."; "Van CAB-1234
has 6 seats for 4 travellers."; "Day 3: 140 km, about 3 h 10 min of driving."; "Total USD 624 against a budget of
USD 1,500 — 58% under budget." Before any proposal exists it returns 404.

## Availability grid (manager)

`GET /api/availability/grid?from=&to=&type=&language=&seats=&city=`
- The range is at most 62 days. `type` is optional (Guide, Vehicle or Room). `language` filters guides, `seats`
  filters vehicles (minimum seats), and `city` filters hotel room types.
- Response:

```
{ days: ["2026-10-02", …],
  rows: [{ resourceType, resourceId, name, detail, capacity,
           cells: [{ date, state: "Free"|"Held"|"Confirmed"|"Blocked", holdId?, tripRequestId?, touristName?,
                     tripStatus?, note?, heldQuantity, freeQuantity }] }] }
```

- Room types use `capacity = totalRooms` and give `freeQuantity` per night. Guides and vehicles have capacity 1.
- Cell states:
  - `Blocked`: a manual hold (no trip), such as leave or maintenance.
  - `Confirmed`: a hold for a trip that is Confirmed, InProgress or Completed.
  - `Held`: a hold for a trip in any other status.
  - `Free`: nothing held. For rooms, the state is that of the largest hold that night.

Manual blocks:
- `GET /api/resource-holds/{id}` returns a `HoldDto`.
- `POST /api/resource-holds` creates one (this already exists). The body is
  `{resourceType, resourceId, fromDate, toDate, quantity, note}`; `note` describes the block, e.g.
  "Annual leave" or "Maintenance".
- `PUT /api/resource-holds/{id}` with `{fromDate, toDate, quantity, note}` works for manual blocks only; a trip hold
  returns 409.
- `POST /api/resource-holds/{id}/release` releases a hold (this already exists).

## Hotels with room types

- `POST /api/hotels` and `PUT /api/hotels/{id}` take
  `{name, city, starRating, latitude, longitude, isActive, roomTypes: [{id?, name, capacity, ratePerNightLkr, totalRooms}]}`.
  - `roomTypes` needs at least one row; names must be unique within the hotel; capacity is 1–8; rate > 0; totalRooms is 1–500.
  - On PUT, a row with `id` updates that room type, a row without `id` adds one, and a missing id is deleted. Deleting
    a room type that has holds returns 409.
- `HotelDto.roomTypes` is unchanged. The per-room-type endpoints still exist.

## Guides

These are unchanged from API-V11: the response holds `{guide, email, temporaryPassword}`. For the "Share with guide"
text, the web builds it itself:
"Your TripCraft guide login: {email} / temporary password {password}. Open the TripCraft app and sign in; you will
be asked to choose a new password."

## Settings (Admin)

- `GET /api/admin/settings` →
  `{llmProvider: "ollama"|"groq", cancellationCutoffDays, marginPct, depositPct, operatorContact, updatedAt}`.
- `PUT /api/admin/settings` takes the same body without `updatedAt`.
- Validation:
  - `llmProvider` must be `ollama` or `groq`.
  - `cancellationCutoffDays` is 0–30.
  - `marginPct` is 0–100; saving it writes today's rate card margin.
  - `depositPct` is 0–100.
  - `operatorContact` is required, at most 200 characters.
- The values are read on every request. `llmProvider` is sent to the agent service with each new workflow.
- Other roles get 403.

## Audit log filters (Admin)

`GET /api/admin/audit-logs?actor=&entity=&action=&from=&to=&page=&pageSize=`. `actor` matches part of the user's
email (case-insensitive); `actor=system` returns rows without a user. The response already includes the actor email
and role.

## Deposit and payment (quotations)

- `QuotationDto` adds `depositPct`, `depositLkr`, `depositUsd`, `depositPaid` (bool) and `depositPaidAt`.
  `depositPct` is the setting at the time the version was made.
- `POST /api/quotations/{id}/payment` with `{paid: true|false}` (manager). It is allowed only on the newest version
  once the client has accepted it: the trip is ClientAccepted, Confirmed, InProgress or Completed. Otherwise it
  returns 409. The change is audited.

## PDF export

`GET /api/trips/{id}/itinerary.pdf` (owner tourist or manager) returns `application/pdf` with:
- the trip details;
- the day-by-day itinerary: the saved itinerary when confirmed, otherwise the proposal;
- the newest sent quotation's lines, totals in LKR and USD, the deposit and its paid state.

It returns 409 when no quotation has been sent yet.

## Email (fourth third-party integration)

Emails go to the tourist on QuotationSent and on Confirmed. The service sends them with the Mailtrap sandbox HTTP
API (`MAILTRAP_API_TOKEN`, `MAILTRAP_INBOX_ID`). It uses the same typed-HttpClient and resilience wrapper as FX,
distance and weather. With no settings, or when Mailtrap fails, the email falls back to the pickup folder, so a
booking never fails because of email.
