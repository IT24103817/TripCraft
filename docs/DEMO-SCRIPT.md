# Demo script (10 minutes, v1.1 lifecycle)

This is PLAN.md section 15, with the exact screens, accounts and URLs of this system. All seeded accounts use the
password `Passw0rd!`. Replace `<api>` and `<web>` with the live URLs, or use the local ones: API
`http://localhost:5080` and web `http://localhost:5199`. The statuses and buttons follow
[diagrams/workflow.md](diagrams/workflow.md).

## Pre-demo checklist (start 15 minutes before)

- [ ] **Wake Render and Neon:** open `https://<api>/health`. It should return `{"status":"ok","db":"ok"}`; the first call may take about 50 s.
- [ ] **Ollama is running:** `curl localhost:11434/api/version`, and `ollama list` shows `llama3.1:8b`.
- [ ] **The agent service is running:** `curl localhost:8001/health` returns `{"status":"ok"}`. The `INTERNAL_AGENT_KEY` must be the same as the API's.
- [ ] **`VOUCHER_SIGNING_KEY` is set** on the API (32+ random bytes). Without it, Confirm and voucher scans fail.
- [ ] **Seed check:** login as `manager1@tripcraft.test` works, and `GET /api/attractions/cities` lists 6 cities.
- [ ] **Phone:** the app is installed, location, camera and notification permissions are granted, and you are logged out.
- [ ] **Browser tabs:** `<web>/login`, `<api>/swagger` and [docs/evidence/sample-voucher.pdf](evidence/sample-voucher.pdf).
- [ ] **Database view:** keep the SQL below open.
- [ ] **Fallback recording:** have a recording of minutes 1–7 ready.

```sql
select status, count(*) from trip_requests group by status;
select action, before, after, at from audit_logs where entity_id = '<trip id>' order by at;
select resource_type, status, from_date, to_date from resource_holds where trip_request_id = '<trip id>';
select type, night, rooms from vouchers where trip_request_id = '<trip id>';
```

## Script

| Time | Who | Do exactly this | Point out |
|------|-----|-----------------|-----------|
| 0:00–1:00 | A | README → `docs/diagrams/workflow.md` (state diagram) | One status machine, 11 statuses, two human gates, nothing booked before Confirm |
| 1:00–2:00 | A | Phone as `tourist1@tripcraft.test` → **Home**: greeting, "Trips picked for your mood" → **Slow train journey** → itinerary and map → **Customize with the planner** (form prefilled: objective, cities Kandy, Nuwara Eliya, Ella) → start **today**, 4 travellers → camera → **Submit** (or **Book as is** for a package without changes) | Packages come from `trip_templates`, priced from today's rate cards; cities come from a list; the trip card says "Our planner agents are building your trip" |
| 2:00–3:00 | C | Web as `manager1@tripcraft.test` → **Dashboard**: "Needs your action" → **Proposals to review** tile → **Agent runs** for the trip | Four navigation groups; Planner → Itinerary → Resources → Validation; the bell shows "Trip ready for review" |
| 3:00–4:30 | C | **Review queue** → open the trip: read **Why this plan** (guide, vehicle, hotels, driving, budget) and the banner → **Edit directly**: change day 1's stop, swap the vehicle → **Re-price** → v1 and v2 side by side → **Send to client** | "Why this plan" is built from the agent step summaries; Send is disabled until re-priced; v2 is a new version |
| 4:30–5:30 | A | Phone: a local notification "Your quotation is ready" → open the trip → **Accept** | Trip *Client accepted*; the manager's bell counts it |
| 5:30–6:30 | C | Web: the dashboard tile **Client-accepted to confirm** → trip → **Confirm** → **Quotation** tab: deposit 30 % → **Mark deposit paid** → **Availability**: the guide's and van's cells are *Confirmed* (hover shows the tourist); click a free cell to add a maintenance block | One transaction: holds, saved itinerary, vouchers, *Confirmed*, audit, emails (Mailtrap inbox or pickup folder); itinerary PDF from the trip page |
| 6:30–7:30 | A / B | Phone: tourist **Vouchers** (QR codes). Log in as `guide1@tripcraft.test` (on first login with a new guide account the app forces a password change) → trip → **Scan voucher** with the tourist's phone screen | HMAC-signed code checked for signature, trip, day and guide; trip *In progress*; GPS check-in is the fallback |
| 7:30–8:30 | B | Guide: **Request replacement** with a reason → web dashboard **Guide change requests** → pick a same-language guide → **Swap** | Holds swapped in one transaction; both guides and the tourist are notified |
| 8:30–9:15 | C | Safe-failure path: a new trip with budget **400** → review page shows the over-budget warning, **Send** disabled → **Request revision** with a comment → v2 arrives | Soft rule, re-plan with the comment; Swagger as tourist `POST /api/quotations/{id}/approve` → **403** |
| 9:15–10:00 | all | Admin **Settings**: switch Ollama ↔ Groq and the cancellation notice; tourist **Cancel** on a trip starting within the notice → "cancellation closed, contact operator"; dark mode toggle; terminal test runs | Settings are stored in the database and read per request; `dotnet test`, `pytest -q`, `npm test`, `flutter test` |

**Rehearsing:** Confirm holds the guide and vehicle on those dates, so the next run on the same dates gets other
resources. Cancel the rehearsal trip (as manager) to release its holds, or use fresh dates. The voucher scan
only works on a day of the trip, so the main demo trip starts **today**.

## If something fails live

| Symptom | Say / do |
|---------|----------|
| Trip ends *Failed safely* | This is the designed safe failure: the history shows the reason. Press **Try again** on the phone, or show the recording. |
| Confirm returns 409 | A resource was taken meanwhile; nothing was saved. Use **Reopen review**, swap the resource, re-price and send again. |
| Voucher scan returns "This voucher is for …" | The trip does not run today; use the GPS check-in or a trip that starts today. |
| Planning is slow | The local 8B model takes about 40 s for Planner + Itinerary; keep talking over the timeline. |
| Login returns 429 | Login is limited to 5 attempts per minute per IP; wait one minute. |
