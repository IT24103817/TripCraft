# TripCraft v1.1 — lifecycle API contract

This is the contract that React and Flutter use for the v1.1 trip lifecycle. Both clients call only the ASP.NET Core
API. JSON is camelCase, except the agent-shaped proposal objects (`days`, `resources` in a proposal or a snapshot).
Those keep the agents' snake_case: `attraction_id`, `entry_fee_lkr`, `transfer_km`, `guide_id`, `vehicle_id`,
`room_type_id`, `hotel_id` and `night`. Errors are RFC 7807 ProblemDetails: 400 adds `errors`, and 409 means the
move is not allowed now.

## Trip statuses (`TripStatusMachine`)

Quotations go straight to the client. **The human approval gate is Confirm**: an Operations Manager confirms a
quotation the client accepted, and only then are guide, vehicle and rooms held. No manager action sits between the
agents and the client.

```
Submitted ─► Planning ─► QuotationSent ─► ClientAccepted ─► Confirmed ─► InProgress ─► Completed
               │  ▲          │   ▲              │
               ▼  │          ▼   └──────────────┘ (manager: Edit & resend)
          NeedsOperator   ClientDeclined ─► Planning (manager: Replan with note)
Cancelled: from Submitted, NeedsOperator, QuotationSent, ClientAccepted, ClientDeclined, Confirmed.
```

| From | Allowed next |
|------|--------------|
| Submitted | Planning, Cancelled |
| Planning | QuotationSent (proposal passed validation, auto-sent), NeedsOperator (agents failed safely or a Hard rule failed) |
| NeedsOperator | Planning (Retry planning), QuotationSent (Edit & send manually), Cancelled |
| QuotationSent | ClientAccepted, ClientDeclined, Cancelled |
| ClientAccepted | Confirmed (manager Confirm), QuotationSent (manager Edit & resend), Cancelled |
| ClientDeclined | Planning (manager Replan with note), Cancelled |
| Confirmed | InProgress, Cancelled |
| InProgress | Completed |
| Completed, Cancelled | — |

`PendingReview`, `RevisionRequested` and `FailedSafely` are no longer trip statuses. The migration maps old rows:
PendingReview → ClientDeclined (if the newest quotation was declined) or NeedsOperator; RevisionRequested → Planning;
FailedSafely → NeedsOperator.

Every change is an audit row. `GET /api/trip-requests/{id}/history` returns
`[{at, action, entity, actor, fromStatus, toStatus, reason}]`. `actor` is a role or `System`; the auto-send is
`System`.

The agent workflow status (`GET /api/trip-requests/{id}/workflow`, `.status`) is unchanged. It is Approved once
the quotation was sent and Completed after Confirm. `finalOutcome.editedSinceQuotation = true` means the proposal
was edited after it was priced.

## Cities

- `GET /api/attractions/cities` → `["Colombo","Ella","Galle","Kandy","Nuwara Eliya","Sigiriya"]`. Any signed-in user.
- `POST /api/trip-requests` and `PUT /api/trip-requests/{id}` take `cities: string[]`. It is required, holds 1–10
  unique cities, at most one per trip day, and each city must come from that list. Otherwise the call returns 400 with
  the message "'X' is not a city we cover. Supported cities: …".
- `TripRequestDto.cities: string[]`.
- `GET /api/trip-requests?cities=Kandy&cities=Ella` returns only trips that visit every listed city.

## Auto-send and the budget rule

When the agents' proposal arrives, the API runs `ProposalValidator`:
- **Valid:** a quotation version is created, marked sent, and the trip moves Planning → QuotationSent. The tourist
  gets a notification ("Your quotation is ready") and an email.
- **Only over budget (Soft rule) after the agents' re-plans:** the agents re-plan with a lowest-cost strategy
  (cheapest rooms, cheapest eligible guide and vehicle, fewer paid entries) up to `MAX_REPLANS`. If the total is
  still over the budget, the quotation is **sent anyway** with `bestAvailablePrice = true` and
  `budgetNote = "Best price we can offer — USD X above your budget"`.
- **Any Hard rule, or the agents failed safely:** nothing is sent. The trip becomes NeedsOperator, with the error
  summary on the workflow.

`QuotationDto` adds `bestAvailablePrice` (bool), `overBudgetUsd` (decimal, null when within budget) and `budgetNote`
(string, null when within budget).

## Client (Tourist) at QuotationSent

- Find the quotation id with `GET /api/trip-requests/{id}/workflow` → `finalOutcome.proposal.quotationId`, or the
  newest version.
- `POST /api/quotations/{id}/accept` → ClientAccepted, and the managers are notified.
- `POST /api/quotations/{id}/decline` `{reason}` (required) → ClientDeclined. The reason is shown to the manager.
- After the manager edits and resends, the tourist gets "Your quote was updated, please review" and must accept the
  new version again.

## Operations Manager actions

| Trip status | Action | Call |
|-------------|--------|------|
| ClientAccepted | **Confirm** (the approval gate: holds, itinerary, vouchers, email in one transaction) | `POST /api/trip-requests/{id}/confirm`. Returns 409 unless the newest version is the accepted one and nothing was edited since it was priced. |
| ClientAccepted, NeedsOperator | **Edit** a day / swap resources | `PUT /api/trip-requests/{id}/proposal/days/{n}`, `PUT /api/trip-requests/{id}/proposal/resources` |
| ClientAccepted, NeedsOperator | **Re-price** (new version, not sent yet) | `POST /api/quotations/{id}/calculate` → `RepriceResponse`. A Hard rule returns 409. |
| ClientAccepted, NeedsOperator | **Send** the re-priced version (Edit & resend / Edit & send manually) | `POST /api/quotations/{id}/send` `{comment?}` → QuotationSent; the tourist is notified "Your quote was updated, please review". |
| ClientDeclined | **Replan with note** | `POST /api/trip-requests/{id}/replan` `{note}` (required) → Planning. The note and the client's reason go to the Planner, and the new version is auto-sent. |
| NeedsOperator | **Retry planning** | `POST /api/trip-requests/{id}/start-planning` (managers may call it) → Planning |
| any cancellable | **Cancel** with reason | `POST /api/trip-requests/{id}/cancel` `{reason}` (the tourist is notified) |

These endpoints are removed: `POST /api/quotations/{id}/approve`, `/reject`, `/request-revision` and
`POST /api/trip-requests/{id}/reopen-review`.

**Versions side by side** work as before: `GET /api/quotations?tripRequestId={id}&sort=version`, then
`GET /api/quotations/{id}`, which includes `proposalSnapshot` and `decisions` (Declined with the client's reason,
Accepted, Approved = sent by a manager, Confirmed).

**Dashboard (manager):**
- `GET /api/dashboard/actions` →
  `{acceptedToConfirm, declinedNeedsDecision, needsOperator, guideChangeRequests, recentCancellations}`.
- `GET /api/dashboard/attention?status=ClientAccepted|ClientDeclined|NeedsOperator` (optional; all three when
  omitted) →
  `[{tripRequestId, objective, status, startDate, endDate, pax, touristName, detail, since, totalUsd}]`.
  `detail` is the client's decline reason, the error summary, or "Version N accepted".

## Cancellation

- `GET /api/trip-requests/{id}/cancellation` → `{canCancel, cancelUntil (date), cutoffDays, closedReason?,
  operatorContact}`.
- `POST /api/trip-requests/{id}/cancel` `{reason}` (required). The tourist can cancel until `cancelUntil`
  (CANCELLATION_CUTOFF_DAYS, default 3); a manager can cancel at any time. Holds are released in the same
  transaction, and the other side is notified. After the cut-off the response is 409 with a message that names
  the operator contact.

## Guides

- `POST /api/guides` `{name, phone, languages, dayRateLkr, maxPax, isActive, email}` (manager) → 201
  `GuideAccountDto {guide, email, temporaryPassword}`. The password is shown **once**.
- `POST /api/guides/{id}/reset-password` (manager) → `GuideAccountDto` with a new temporary password.
- `PUT /api/guides/{id}` takes `{name, phone, languages, dayRateLkr, maxPax, isActive}`; `userId` is gone.
- Login `user.mustChangePassword: true` → the app must force `POST /api/auth/change-password`
  `{currentPassword, newPassword}` (new password: 8+ characters with upper, lower and digit) → `UserDto`.
- There is no public guide registration: `/api/auth/register` always creates a Tourist.

## Guide change requests

- `POST /api/trip-requests/{id}/guide-change-requests` `{reason}` (the trip's guide, trip Confirmed) → 201.
- `GET /api/guide-change-requests` (manager) → open requests:
  `[{id, tripRequestId, tripObjective, startDate, endDate, pax, language, guideId, guideName, reason, status,
  candidates:[{id, name, languages, maxPax}]}]`.
- `POST /api/guide-change-requests/{id}/resolve` `{replacementGuideId}` (manager). The guide holds are swapped,
  and both guides and the tourist are notified.

## Vouchers and check-in

- `GET /api/trips/{id}/vouchers` (owner tourist or manager) →
  `[{id, type: "Trip"|"HotelNight", hotelId, hotelName, night, rooms, code, qrPayload}]`. The QR code shows
  `qrPayload` (`TRIPCRAFT-VOUCHER:…`).
- `GET /api/trips/{id}/vouchers.pdf` returns a printable PDF, one page per voucher.
- `POST /api/vouchers/verify` `{code}` (guide or manager) → `{valid, message, tripRequestId, type, night}`.
- `POST /api/check-ins` (guide) takes either `{voucherCode, tripRequestId?}` (a scanned trip voucher: it checks
  in the next stop of today's day) or `{itineraryStopId, latitude, longitude}` (GPS within 500 m). It returns
  `{stopId, distanceMeters?, checkedInAt, tripStatus, method: "Voucher"|"Gps", stopName}`. The first check-in →
  InProgress; the last stop → Completed.

## Notifications

- `GET /api/notifications/mine` → `{unreadCount, items:[{id, type, title, body, tripRequestId, isRead, createdAt}]}`
  (the newest 50).
- `POST /api/notifications/{id}/read` and `POST /api/notifications/read-all` → 204.
- Types: QuotationSent, QuotationUpdated, ClientAccepted, ClientDeclined, NeedsOperator, TripConfirmed, TripAssigned,
  TripCancelled, GuideChangeRequested, GuideReplaced, GuideChanged.
