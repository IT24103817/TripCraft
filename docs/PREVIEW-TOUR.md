# Preview tour

A 15-step click-through of the complete system (branch `feat/quotations-c`: Students A, B and C together) on
the local stack. Follow the steps in order; each says where to go and what you should see.

**Before you start** — the stack is running and the database is freshly seeded:

| What | Where |
|------|-------|
| React app | <http://localhost:5199/login> |
| Swagger | <http://localhost:5080/swagger> |
| API health | <http://localhost:5080/health> — `db: "ok"` |
| Agent service health | <http://127.0.0.1:8001/health> |
| Mobile app | Android emulator `tripcraft`, started with `flutter run --dart-define=API_URL=http://10.0.2.2:5080` |

Every seeded account uses the password **`Passw0rd!`**: `tourist1…3`, `guide1…3`, `manager1…3`, `admin1…3`
`@tripcraft.test`. Guide logins: guide1 = Nimal Perera, guide2 = Kumari Silva, guide3 = Ruwan Fernando
(Anjali Jayasinghe has no app login).

The login endpoint allows 5 attempts per minute from one machine; if you see "Too many attempts", wait a minute.

## React — Operations Manager

**1. Log in** — <http://localhost:5199/login>, `manager1@tripcraft.test` / `Passw0rd!`, **Sign in**.
You land on the **Dashboard** (`/`). The sidebar shows Dashboard, Approvals, Trip requests, Attractions, Guides,
Vehicles, Hotels, Availability, Agent workflows, Quotations and Reports — no Users (that is Admin's).

**2. Dashboard** — `/`. KPI cards: **Pending approvals 1**, **Trips this month**, **Revenue this month** (USD, from
approved quotations this month — 0 until you approve something) and **Active workflows**. Below, **Latest
workflows** lists the pending trip's workflow (status Pending approval).

**3. Approvals inbox and the review page** — sidebar **Approvals** (`/approvals`). The *Pending approval* tab has one
row: the November Kandy + Ella trip for 2 people. Click **Open proposal …** → `/approvals/{workflowId}`. You should see:
the trip and budget (USD 1,200); **Deterministic validation** — "All deterministic checks passed" with every rule
ticked; the proposed **Itinerary** (4 days, Kandy then Ella); **Proposed guide, vehicle and rooms**; **Quotation
v1 (Pending)** with named lines in LKR and USD, the exchange rate and when it was fetched; the **Approve**,
**Reject** and **Request revision** buttons; and **Re-price with today's rates**. Leave it pending for now (or
approve it — both are fine).

**4. Workflow monitor** — sidebar **Agent workflows** (`/workflows`) → click the row → `/workflows/{id}`. Status,
started/finished, elapsed and agent time, then the **Agent steps** timeline: 1 Planner / Coordinator, 2 Itinerary
Analysis, 3 Resource & Action, 4 Validation & Safety — each with its tool calls (e.g. `get_weather · … ms`),
duration, retries and status. Open **Summaries and validation result** on a step to see its JSON summary.

**5. Guides CRUD** — sidebar **Guides** (`/resources/guides`). Four seeded guides. Try: type `ru` in the search
(only Ruwan); **Language** = `de` (only Kumari); click the **Day rate** header twice to sort descending; change the
page size and page. **Add guide**: enter phone `12` and languages `english` → field errors appear before anything
is sent; fix them (`+94 77 000 1111`, `en, it`) → toast "Added …". Click the row to edit; **Delete** asks for
confirmation (a guide held for an upcoming trip is refused with a 409 message).

**6. Vehicles and hotels CRUD** — **Vehicles** (`/resources/vehicles`): three vehicles; filter **Type** = Van or
**Seats at least** = 10; add one (registration must be unique — a duplicate shows the API's 409 message).
**Hotels** (`/resources/hotels`): four hotels; filter **City** = Ella; click **2 types · … rooms** to open the
room-types dialog and **Add room type**; coordinates outside Sri Lanka are refused.

**7. Availability calendar** — sidebar **Availability** (`/availability`). **Find available resources**: Type =
Vehicle, pick dates, Seats = 4, **Search** → free vehicles with their km rates; **Block** one to create a manual
hold. **Hold calendar** below: one row per held resource, indigo cells for trips and amber for manual blocks; the
list under it has **Release** for each hold. (Holds from approved trips appear here after step 12.)

**8. Reports** — sidebar **Reports** (`/reports`). Period filter at the top (defaults to this year). **Trip requests
by status**, **Revenue by month** (August shows the seeded completed trip) and **Guide and vehicle utilisation**
(held days as a percentage). **Quotations** (`/quotations`) lists every quotation version with status and FX
filters, search by the trip's objective, sorting and paging.

**9. Admin: users and audit log** — sign out (profile menu, top right) and sign in as `admin1@tripcraft.test`.
The sidebar is now Dashboard, Agent workflows, Users and Audit log. **Users** (`/admin/users`): search, filter by
role/status, create a user, deactivate one. **Audit log** (`/admin/audit-logs`): every business change with who,
action, entity and before → after; filter **Entity** = TripRequest, search `approved`, sort by date. Opening
`/approvals` as Admin shows the 403 page (separation of duties). Sign out and sign back in as `manager1` for step 12.

## Emulator — Tourist, then Guide

**10. Register a tourist** — on the emulator's **Sign in** screen tap **New here? Create an account**. Full name,
a new email (e.g. `you@example.com`), a password with upper case, lower case and a digit (8+ characters), and
nationality → **Create account**. You are signed in on **My trips** (empty state "No trip requests yet").

**11. Submit the section 6 demo request with a photo** — tab **New trip**. Objective: *5 days for 4 people,
Kandy and Ella, prefer the hill-country train, English-speaking guide.* Travel dates: tap the field, then the pencil
(**Switch to input**) and type 10/10/2026 and 10/14/2026, **OK**. Travellers: tap **+** twice → 4. Budget 1500.
Chips **Hill-country train** and **English-speaking guide**. Nationality and passport number (e.g. `N1234567`).
**Passport photo**: **Camera** (the emulator's virtual scene) or **Gallery**. **Submit trip request**.
You land on **Trip request** with the **Progress** timeline at *Planning* and "Our AI agents are planning your
trip. This page updates every 10 seconds."

**12. Watch the status, then approve in React** — in React, **Agent workflows** shows the new workflow in
Planning; open it and watch the four steps appear (about 1½–2 minutes with the local model). When it reaches
*Pending approval* the phone shows the orange **Awaiting operator approval** card. In React: **Approvals** →
**Open proposal …** for the 4-person trip → check the checklist, itinerary, resources and quotation (USD, under the
USD 1,500 budget) → **Approve** → confirm **Approve** in the dialog. Toast: "Approved. Trip is now confirmed;
6 holds created." Note the **guide's name** on the review page — you log in as that guide in step 15.

**13. Pull to refresh on the phone** — on the trip page, pull down. **Status: Confirmed**, the timeline's
*Confirmed* step is current, **Planning — Status: Completed**, the **Itinerary** shows the saved days, and
**History** lists every change (submitted → planning → pending approval → confirmed).

**14. Accept the quotation** — tap **View quotation**. Items with names (guide, vehicle, hotel rooms, entry
tickets), subtotal, service margin (15 %) and total in LKR and USD, the rate and its date. "Your operator approved
this price. Accept it to confirm." → **Accept quotation** → snackbar "Quotation accepted." and "You accepted this
price on …". The **Alerts** tab shows the status changes as notifications.

**15. Log in as the guide and check in** — profile icon (top right) → sign out. Sign in as the guide from step 12
(Nimal → `guide1`, Kumari → `guide2`, Ruwan → `guide3`). **My schedule** shows the trip (10–14 Oct, 4 travellers,
vehicle, days with hotel and stops). Tap **Day 1** → **Today's stops**. Put the emulator at the first stop:
emulator **…** (Extended controls) → **Location**, or in a terminal
`adb emu geo fix <longitude> <latitude>` (Temple of the Tooth: `80.6413 7.2936`; Royal Botanical Gardens:
`80.5966 7.2685`). Tap **Find my location** → "You are N m from …" → **Check in** (only enabled within 500 m) →
"Checked in … Trip is in progress." In React the trip is now *In progress* and the **Audit log** has
*StopCheckedIn*. The **Scan voucher** tab scans a hotel voucher QR (hotel id) and shows the hotel.

## If something goes wrong

- A workflow ends **Failed safely**: open it in the workflow monitor — the failing step and message are shown;
  the tourist can tap **Try again** on the trip page.
- The pending trip from the seed is for **3–6 November**; the demo request is **10–14 October**, so they never
  compete for the same guide or vehicle.
