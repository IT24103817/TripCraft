# Final verification

Evaluator run on **28 Sep 2026** on merged `main`, after the demo-hardening and completeness-audit session.

The local stack was:
- PostgreSQL 16;
- the API with the final code;
- the agent service on Ollama `llama3.1:8b`;
- React on Vite (:5199);
- the Flutter release APK on an Android emulator.

Every item was run, not just read. The per-requirement evidence is in [docs/COMPLIANCE.md](COMPLIANCE.md).

## Summary

| Step | Check | Result | Evidence |
|------|-------|--------|----------|
| 4 | Components: entities, migration, seed, ≥ 4 endpoints + business op, FluentValidation, React (search/filter/sort/paging + 4 states), Flutter (loading/empty/error), tests in own folders | **PASS** (after fixes 4, 5, 10, 11, 14) | A 16, B 26, C 13 endpoints (74 in Swagger); [step 4 table](COMPLIANCE.md#completeness-audit-step-4-components) |
| 4 | Status workflow, history endpoints, reporting (plan section 3) | **PASS** | `TripRequestStatus` + services; `/history`, `/api/admin/audit-logs`; `/api/reports/*` + `ReportsPage` |
| 5 | Four separate agents: responsibility, Pydantic contract, `ALLOWED_TOOLS`, step report, timeout, retry limit, safe failure | **PASS** | [step 5 table](COMPLIANCE.md#completeness-audit-step-5-agents); golden tests |
| 5 | Replan loop works | **PASS** (after fixes 7, 8) | automatic (`test_over_budget.py`) and manager-triggered (live: planner `hotel_tier=budget`, quotation v2) |
| 5 | ProposalValidator implements every plan section 5 rule | **PASS** (after fix 6) | 15 `ProposalValidatorTests`, one or more per code incl. new `DRIVING_LIMIT` |
| 5 | Approval gate: nothing held before approve | **PASS** | only `ApproveAsync` creates trip holds; new assertion in `ProposalEndpointTests`; live: 0 holds at PendingApproval |
| 6 | JWT, four roles, 403 tests | **PASS** | 403 tests for every controller (4 new); e2e `roles.spec.ts` |
| 6 | Internal API behind `X-Internal-Key` | **PASS** | `InternalEndpointsTests` (+ proposal without key, unset key) |
| 6 | Three wrappers with fallback, 429 tests, resilience | **PASS** | `*ServiceTests` 429 cases; new `HttpResilienceTests` (retry on 5xx, none on 429, per-try timeout) |
| 6 | Secrets only from configuration | **PASS** | scan of all changes: no keys; new local secrets live only in an untracked scratch file (mode 600) |
| 6 | Landing `/`, `/dashboard`, redirect by role | **PASS** | `roles.test.ts`, `LoginPage.test.tsx`, `HomeLink.test.tsx`, `NotFoundPage.test.tsx` |
| 6 | ≥ 3 Flutter device features | **PASS** (6) | camera, GPS, date-range picker, QR, notifications, map; each has a test and was used live |
| 6 | CI for backend, web, mobile, agents | **PASS** (files) / not run on GitHub | four workflows, `.github/workflows/README.md`; every step run locally |
| 6 | README and docs per 14.1 / 14.2 | **PASS** (live URLs etc. manual) | README, 6 ADRs, ER diagram with 22 tables; 126 links, 0 broken |
| 7 | COMPLIANCE.md: every row DONE with evidence | **PASS** | no PARTIAL/MISSING rows because of code; the rest are manual |
| 8 | `dotnet build -warnaserror` | **PASS** | 0 warnings, 0 errors |
| 8 | `dotnet test` | **PASS** | **333 passed**, 0 failed (twice in a row) |
| 8 | `ruff` + `pytest` | **PASS** | clean, **58 passed** |
| 8 | `npm run lint && npm test && npm run build` | **PASS** | 0 warnings, **82 passed** (19 files), built |
| 8 | `flutter analyze && flutter test` | **PASS** | no issues, **68 passed** |
| 8 | Playwright e2e | **PASS** | **6/6** (fresh random dates) |
| 8 | k6 list-load (p95 < 800 ms, errors < 1 %) | **PASS** | 610,814 requests, p95 **9.56 ms**, 0.00 % failed, checks 100 % |
| 9 | Section 6 from the emulator, run 1 (16–20 Nov) | **PASS** | camera photo → 4 steps in the monitor → PendingApproval → approved (6 holds) → notification + Confirmed → accepted → guide1 check-in 0 m → InProgress |
| 9 | Section 6 from the emulator, run 2 (14–18 Dec) | **PASS** | same, first attempt at every step |
| 9 | Safe-failure path: budget 400 → RevisionRequested → replan | **PASS** | v1 RevisionRequested (USD 532.04) → manager revision → budget-tier replan → v2 |
| 9 | Injection objective | **PASS** (second attempt, after fix 9) | PendingApproval, total LKR 62,100 (not 0), 0 holds, 0 decisions |
| — | `flutter build ios --no-codesign` | **Not run (manual)** | Xcode is installed now but its licence is not accepted (`sudo xcodebuild -license`) |
| — | Deployed system, CI on GitHub | **Not run (manual)** | no accounts, no remote |

**Totals:** every automated check PASS. Two notes:
- **Injection run.** It passed on the second attempt: the first found bug 9, which is now fixed and tested.
- **Emulator run 1.** The Android system server crashed during the guide step (emulator under load). The step was
  repeated and passed. The app did every step on its first try in both runs.

## What was fixed

The full list with tests is in
[COMPLIANCE.md → What this session fixed](COMPLIANCE.md#what-this-session-fixed-28-sep-2026).

1. **Deterministic demo data.** 21 attractions in 6 cities, one hotel per city, and all 15 distances. The seed tops up
   existing databases.
2. **Guide selection in code.** `pick_guide` picks the cheapest available guide with the language; the step summary
   explains the choice.
3. **e2e on fresh dates.** A random 5-day window 30–729 days ahead; no SQL clean-up is needed any more.
4. **Itinerary editor.** A `PUT` endpoint and the React dialog.
5. **Search and sort** on the approvals inbox and the workflow monitor.
6. **C# `DRIVING_LIMIT` rule**, plus tests for every validator code.
7. **Manager revision re-plans with the rejected violations**, so the budget tier is used.
8. **Budget tier priced per party**, not per room.
9. **One-city trips got stuck in Planning.** A zero-quantity vehicle line was rejected by PostgreSQL. Zero lines are
   now dropped, and a proposal the database rejects ends FailedSafely.
10. **`GET /api/hotels/{id}/room-types`**, and the guide's vehicle lookup on the phone.
11. **Mobile.** The workflow load error shows Retry; device-feature tests added; unused `permission_handler` removed.
12. **Missing tests** for the manager schedule route, Resources validators, internal-key cases, the resilience
    pipeline and wrong-role 403s.
13. **Web.** 404/403 home links by role, empty-state actions, min-total filter, the hold-calendar notice, `homeFor`
    tests.
14. **Docs.** ER diagram with 22 tables and seed data, database report, README, backend README, CI README,
    `render.yaml`, DEMO-SCRIPT (guide1 is the deterministic demo guide).

Noted, not changed:
- The small model still sometimes repeats an attraction on later days. It is allowed, and the rules pass.
- The first injection attempt left one workflow in "Planning" in the local database, from before fix 9. It remains
  there as evidence.

## Manual TODOs for you

See [COMPLIANCE.md → Manual TODO](COMPLIANCE.md#manual-todo-for-the-student). In short:
1. Push.
2. Lecturer approval.
3. B and C own their components.
4. GitHub and CI.
5. Deploy, with `VITE_APK_URL` and `VITE_GROUP_NUMBER`.
6. iPhone signing.
7. Check the deployed system.
8. **Accept the Xcode licence** (`sudo xcodebuild -license accept`), then run the iOS build.
9. Report screenshots.
10. The 94 report TODOs.
11. Individual sections.
12. Group AI declaration.
13. Video.
14. Viva practice.
15. Submission.
