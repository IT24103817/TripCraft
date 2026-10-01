# TripCraft v1.1 — lifecycle API contract

This is the contract that React and Flutter use for the v1.1 trip lifecycle. Both clients call only the ASP.NET Core
API. JSON is camelCase, except the agent-shaped proposal objects (`days`, `resources` in a proposal or a snapshot).
Those keep the agents' snake_case: `attraction_id`, `entry_fee_lkr`, `transfer_km`, `guide_id`, `vehicle_id`,
`room_type_id`, `hotel_id` and `night`. Errors are RFC 7807 ProblemDetails: 400 adds `errors`, and 409 means the
move is not allowed now.

## Trip statuses (`TripStatusMachine`)

```
Submitted ─► Planning ─► PendingReview ─► QuotationSent ─► ClientAccepted ─► Confirmed ─► InProgress ─► Completed
              │   ▲          │   ▲  ▲          │                 │
              ▼   │          ▼   │  └──────────┘ (declined)      └─► PendingReview (reopen)
        FailedSafely    RevisionRequested
Cancelled: from Submitted, FailedSafely, PendingReview, RevisionRequested, QuotationSent, ClientAccepted, Confirmed.
```

| From | Allowed next |
|------|--------------|
| Submitted | Planning, Cancelled |
| Planning | PendingReview, FailedSafely |
| FailedSafely | Planning ("Try again"), Cancelled |
| PendingReview | QuotationSent, RevisionRequested, Cancelled |
| RevisionRequested | PendingReview, FailedSafely, Cancelled |
| QuotationSent | ClientAccepted, PendingReview (declined), Cancelled |
| ClientAccepted | Confirmed, PendingReview (reopen), Cancelled |
| Confirmed | InProgress, Cancelled |
| InProgress | Completed |
| Completed, Cancelled | — |

Every change is an audit row. `GET /api/trip-requests/{id}/history` returns
`[{at, action, entity, actor, fromStatus, toStatus, reason}]`. A status change has `action = "TripRequestStatusChanged"`
and a human-readable `reason`. `actor` is a role (`Tourist`, `OperationsManager`, `Guide`) or `System`.

The agent workflow status (`GET /api/trip-requests/{id}/workflow`, `.status`) is separate. Its values are Planning,
PendingApproval (valid, can be sent), RevisionRequested (over budget: only a warning, so "Send" is disabled until it
is revised or edited and re-priced), Approved (sent), Completed (confirmed), Rejected and FailedSafely.
`workflow.finalOutcome.editedSinceQuotation = true` means the proposal was edited after it was priced, so Re-price
must run first.

## Cities

- `GET /api/attractions/cities` → `["Colombo","Ella","Galle","Kandy","Nuwara Eliya","Sigiriya"]`. Any signed-in user.
- `POST /api/trip-requests` and `PUT /api/trip-requests/{id}` take `cities: string[]`. It is required, holds 1–10
  unique cities, at most one per trip day, and each city must come from that list. Otherwise the call returns 400 with
  the message "'X' is not a city we cover. Supported cities: …".
- `TripRequestDto.cities: string[]`.
- `GET /api/trip-requests?cities=Kandy&cities=Ella` returns only trips that visit every listed city.

## Manager review (PendingReview) — Operations Manager

| Button | Call | Result |
|--------|------|--------|
| Send to client | `POST /api/quotations/{quotationId}/approve` `{comment?}` | trip → QuotationSent, tourist notified. 409 if over budget (workflow RevisionRequested), edited and not re-priced, or not the newest version |
| Request revision | `POST /api/quotations/{id}/request-revision` `{comment}` (required) | trip → RevisionRequested; Planner re-plans with the comment; the new proposal → PendingReview with quotation version n+1 |
| Reject | `POST /api/quotations/{id}/reject` `{comment?}` | trip → Cancelled |
| Edit a day | `PUT /api/trip-requests/{id}/proposal/days/{dayNumber}` `{attractionIds:[guid], notes?}` | 1–3 attractions in that day's city. Returns `EditableProposalDto {workflowId, tripRequestId, editedSinceQuotation, days, resources}` |
| Swap resources | `PUT /api/trip-requests/{id}/proposal/resources` `{guideId?, vehicleId?, rooms?:[{city, roomTypeId}]}` | each must be free (use `GET /api/availability`). 409 otherwise |
| Re-price | `POST /api/quotations/{quotationId}/calculate` | new version: `RepriceResponse {quotationId, version, totalLkr, totalUsd, previousTotalLkr, previousTotalUsd, workflowStatus, validation}`. The old version → Superseded. 409 on a Hard rule |
| Confirm (at ClientAccepted) | `POST /api/trip-requests/{id}/confirm` | one transaction: holds, itinerary, vouchers, Confirmed, audit, email. 409 and no change on conflict |
| Reopen review (at ClientAccepted) | `POST /api/trip-requests/{id}/reopen-review` `{reason}` | → PendingReview |

These calls return `QuotationDecisionResponse {quotationId, tripRequestId, workflowId, decision, tripStatus,
workflowStatus, holdsCreated}`.

**Versions side by side:** use `GET /api/quotations?tripRequestId={id}&sort=version` for the list, then
`GET /api/quotations/{id}` for each version. A `QuotationDto` has `version`, `status` (Pending, Approved = sent,
Declined, RevisionRequested, Superseded, Rejected), `acceptedAt`, `lines` and `proposalSnapshot`
(`{days, resources}`, snake_case inside). `decisions: [{decision, comment, decidedAt}]` carries the manager's
revision comment (`RevisionRequested`) and the client's decline reason (`Declined`).

## Client (Tourist) at QuotationSent

- Find the quotation id with `GET /api/trip-requests/{id}/workflow` → `finalOutcome.proposal.quotationId`, or the
  `quotationId` of the newest version.
- `POST /api/quotations/{id}/accept` → ClientAccepted, and the managers are notified.
- `POST /api/quotations/{id}/decline` `{reason}` (required) → PendingReview. The reason is shown to the manager.

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
- Types: ReviewNeeded, QuotationSent, ClientAccepted, ClientDeclined, TripConfirmed, TripAssigned, TripCancelled,
  TripRejected, GuideChangeRequested, GuideReplaced, GuideChanged, ReviewReopened.
