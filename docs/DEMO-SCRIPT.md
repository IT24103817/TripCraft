# Demo script (10 minutes)

PLAN.md section 15, with the exact screens, accounts and URLs of this system. All accounts use the password
`Passw0rd!`. Replace `<api>` and `<web>` with the live URLs (TODO in the README) or use the local ones:
API `http://localhost:5080`, web `http://localhost:5173`.

## Pre-demo checklist (start 15 minutes before)

- [ ] **Wake Render and Neon:** open `https://<api>/health` → `{"status":"ok","db":"ok"}` (first call may take ~50 s). Mode B: also `https://tripcraft-agents.onrender.com/health`.
- [ ] **Ollama running:** `curl localhost:11434/api/version`; model present: `ollama list` shows `llama3.1:8b`.
- [ ] **Agent service running:** `curl localhost:8001/health` → `{"status":"ok"}`; its `INTERNAL_AGENT_KEY` equals the API's; if the API is on Render, the tunnel URL is in the API's `AGENT_SERVICE_URL` (docs/DEPLOYMENT.md section 4).
- [ ] **Seed check:** `POST <api>/api/auth/login` as `manager1@tripcraft.test` returns 200; Swagger `GET /api/trip-requests` shows the seeded completed trip.
- [ ] **Phone connected:** APK installed (docs/APK-INSTALL.md), Wi-Fi on, camera/location/notification permissions granted, logged out; screen mirroring (e.g. scrcpy) ready.
- [ ] **Browser tabs:** `<web>/login`, `<api>/swagger`, the GitHub repo (Actions tab, Insights → Contributors), `docs/diagrams/architecture.md` on GitHub.
- [ ] **Database view:** Neon SQL editor (or `psql`) open with the queries below.
- [ ] **Terminal:** `backend`, `agents`, `web` test commands ready (docs/TEST-EVIDENCE.md).
- [ ] **Fallback recording:** a pre-recorded run of minutes 1–6 in case the network or model fails.

```sql
select status, count(*) from trip_requests group by status;
select step_no, agent_name, status, duration_ms, retries, tool_name from agent_steps
  where workflow_id = '<workflow id>' order by step_no;
select action, entity, at from audit_logs order by at desc limit 10;
select * from resource_holds where trip_request_id = '<trip id>';   -- once Student B's table exists
```

## Script

| Time | Who | Do exactly this | Point out |
|------|-----|-----------------|-----------|
| 0:00–1:00 | A | README top (CI badges) → `docs/diagrams/architecture.md` → Actions tab | Problem, four roles, clients only talk to the API, internal agent service, CI green |
| 1:00–2:00 | A | Phone: **Create an account** (or `tourist1@tripcraft.test`) → **New trip** → objective "5 days for 4 people, 10–14 October, Kandy and Ella, budget USD 1,500, prefer the hill-country train, English-speaking guide" → **Travel dates** 10–14 Oct → Travellers 4 → Budget 1500 → chips *Hill-country train*, *English-speaking guide* → nationality, passport → **Camera** → **Submit trip request** | Date-range picker, camera, validation messages, 201 then start-planning; trip detail opens with the timeline at *Planning* |
| 2:00–4:00 | C | Web as `manager1@tripcraft.test` → **Agent workflows** → open the newest → watch the timeline refresh (every 5 s) | Planner → Itinerary → Resources → Validation, tool calls with durations, retries, expandable summaries; status *Pending approval* |
| 4:00–5:00 | C | **Approvals** → *Pending approval* tab → **Open** → review itinerary, guide/vehicle/rooms, quotation in LKR and USD with FX rate and as-of → validation checklist all green → **Approve** → confirm | Toast "Approved. Trip is now confirmed; N holds created." Then SQL: `audit_logs` newest rows, `resource_holds` for the trip † |
| 5:00–6:00 | A / B | Phone: pull to refresh → trip shows **Confirmed**. Log out → `guide1@tripcraft.test` → **Schedule** | Status timeline complete; guide schedule † |
| 6:00–7:00 | C | Web or phone: new request with budget **400** → workflow ends *Revision requested* → review page shows "Total is within the tourist's budget — failed", Approve disabled → **Request revision** with a comment. Swagger: authorize as `tourist1@tripcraft.test`, `POST /api/quotations/{id}/approve` | Soft violation and re-plan; **403** for a tourist (separation of duties) |
| 7:00–8:00 | A, B, C | Each: own list with search, sort (click a header), page size, next page — `/trips`, `/attractions`, resources pages, `/workflows`; Swagger endpoint groups | Server-side search/sort/paging; four page states |
| 8:00–9:00 | all | Terminal: `dotnet test`, `pytest -q`, `npm test`, `flutter test`; GitHub Actions history; Insights → Contributors; live URLs | 208 / 43 / 25 / 45 tests; PRs per member |
| 9:00–10:00 | all | `docs/adr/README.md` | One sentence per ADR from its author; close |

† Needs Students B and C merged: until then the workflow ends *Failed safely* at the Resource agent
(Resource Management answers 503), so minutes 2–6 cannot run end to end — use this script once they are merged.

## If something fails live

| Symptom | Say / do |
|---------|----------|
| Workflow ends *Failed safely* | Open it: the error summary names the step and cause — this is the designed safe failure. Show the pre-recorded run for the rest. |
| Planning slow | Local 8B model takes ~40 s for Planner + Itinerary; keep talking over the timeline. |
| API 50 s cold start | Expected on Render's free tier; the checklist above prevents it. |
| Login 429 | Login is limited to 5 attempts per minute per IP; wait a minute. |
