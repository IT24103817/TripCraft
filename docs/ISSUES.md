# GitHub issues — four-day execution plan

One issue per row, taken from PLAN.md section 14 (26–30 September). Owner: A / B / C = Student A / B / C; "All" = whole group.
Rows that say "each student" in the plan are split into one issue per owner so that every issue has one assignee.

## Sat 26 Sep — setup (by midnight)

| # | Issue title | Owner |
|---|-------------|-------|
| 1 | Email lecturer for written approval of 3 members / 3 components / 4 agents | A |
| 2 | Create repo, protect main, set up project board, .gitignore, .env.example, README ownership table | A |
| 3 | Add backend-ci.yml so CI history starts today | A |
| 4 | Create docs/ai-log-a.md and log today's AI use | A |
| 5 | Create docs/ai-log-b.md and log today's AI use | B |
| 6 | Create docs/ai-log-c.md and log today's AI use | C |
| 7 | Scaffold ASP.NET Core solution (API, Application, Infrastructure, Tests) with Identity + JWT login on Neon | A |
| 8 | Scaffold React app (Vite, router, Zustand, Axios interceptor, login page) | B |
| 9 | Scaffold Flutter app (go_router, Riverpod, dio, secure storage, login screen) | B |
| 10 | Scaffold LangGraph service with hello-world graph, Ollama model pulled, Pydantic schemas for all four agent contracts | C |

## Sun 27 Sep — backend and data (all APIs in Swagger by 10 PM)

| # | Issue title | Owner |
|---|-------------|-------|
| 11 | Trip Requests & Itinerary: entities, migration, seed, DTOs, validators, controller + service, endpoints + business op | A |
| 12 | Resource Management: entities, migration, seed, DTOs, validators, controller + service, endpoints + business op | B |
| 13 | Quotation & Approval: entities, migration, seed, DTOs, validators, controller + service, endpoints + business op | C |
| 14 | Availability query + transactional resource hold with overlap check (409 on conflict) | B |
| 15 | Quotation calculator (pure C#) + FX HttpClient + approve/reject/revise endpoints in one transaction | C |
| 16 | start-planning endpoint calling the agent service + agent_workflows / agent_steps tables | A |
| 17 | Deploy API to Render and database to Neon (/health live) | A |
| 18 | Trip Requests: 3 unit tests + 1 integration test | A |
| 19 | Resources: 3 unit tests + 1 integration test | B |
| 20 | Quotations: 3 unit tests + 1 integration test | C |

## Mon 28 Sep — agents and web (Section 6 workflow end to end by 11 PM)

| # | Issue title | Owner |
|---|-------------|-------|
| 21 | Planner / Coordinator agent + Itinerary Analysis agent with tools; OpenWeatherMap HttpClient | A |
| 22 | Resource & Action agent with availability tools; OpenRouteService distance HttpClient | B |
| 23 | Validation & Safety agent, C# deterministic validators, safe-failure path, step persistence | C |
| 24 | React: trip request list, itinerary editor, attraction CRUD (search, sort, pagination, four page states) | A |
| 25 | React: guide/vehicle/hotel CRUD + availability calendar (search, sort, pagination, four page states) | B |
| 26 | React: approval inbox + agent workflow monitor + reports (four page states) | C |
| 27 | Run the golden-case workflow end to end on one machine and fix until it passes | All |
| 28 | Deploy React app to Vercel | All |

## Tue 29 Sep — mobile, tests, evidence (everything demoable by 8 PM)

| # | Issue title | Owner |
|---|-------------|-------|
| 29 | Flutter: trip request form with camera + date-range picker, itinerary view, status timeline | A |
| 30 | Flutter: guide schedule + GPS check-in | B |
| 31 | Flutter: quotation view, accept quotation, notifications | C |
| 32 | Build release APK, install on a real phone, run workflow against deployed API | All |
| 33 | React tests, Flutter widget tests, agent pytest golden cases — capture screenshots | All |
| 34 | k6 performance run — capture screenshots | A |
| 35 | Get all four CI workflows green | All |
| 36 | Write ADRs 001–006, ER diagram, architecture diagram | All |
| 37 | Record the 10-minute demo video (Section 15 script) | All |
| 38 | Draft consolidated report; each student writes Individual section, AI log and reflection | All |

## Wed 30 Sep — submit by 6 PM

| # | Issue title | Owner |
|---|-------------|-------|
| 39 | Assemble and proof-read final report PDF; check all links in an incognito window | All |
| 40 | Complete README: setup, env variable names, startup order, test accounts, live URLs | All |
| 41 | Tag release v1.0 with the APK attached | All |
| 42 | Submit on Course Web and screenshot the confirmation | A |
| 43 | Rehearse viva questions for one hour | All |
