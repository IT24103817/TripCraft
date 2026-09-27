# Final verification

This is the evaluator run of checklist A–K on **27 Sep 2026**, on merged `main` (A + B + C; `git branch --no-merged main`
is empty) plus the final-integration changes: landing page, Hallmark UI, iOS setup and the fixes below.

It ran on the local stack:
- PostgreSQL 16;
- the API, started with the new code;
- the agent service on Ollama `llama3.1:8b`;
- React on Vite (:5199);
- the Flutter release APK on an Android emulator.

The **Result** column only says PASS for things run today. Results carried over from the 26 Sep run
(`chore: final verification and fixes`) say so.

## Result table

| # | Check | Result | Evidence |
|---|-------|--------|----------|
| A1 | `dotnet build -warnaserror` | **PASS** | 0 warnings, 0 errors |
| A2 | `npm run lint`, `tsc` | **PASS** | ESLint `--max-warnings 0` clean (after fix 1); `npm run build` runs `tsc -b` |
| A3 | `flutter analyze` | **PASS** | No issues found |
| A4 | `ruff check` on `agents/` | **PASS** | All checks passed |
| B1 | `dotnet test` | **PASS** | **298 passed**, 0 failed (PostgreSQL 16 via `TEST_DATABASE_URL`) |
| B2 | `npm test` | **PASS** | **45 passed** (13 files) |
| B3 | `flutter test` | **PASS** | **58 passed** |
| B4 | `pytest` | **PASS** | **49 passed** |
| B5 | Playwright e2e (local, real model) | **PASS** (after fixes 5–6) | **6/6**: `roles.spec.ts` ×4, `safe-failure.spec.ts`, `workflow.spec.ts` |
| B6 | k6 `list-load.js` | **PASS** | 50 VUs × 60 s: 800,901 requests, p95 **6.6 ms**, 0.00 % errors, checks 100 % (thresholds p95 < 800 ms, errors < 1 %) |
| C1 | Roles in React + API | **PASS** | `roles.spec.ts` 4/4 with the dashboard now at `/dashboard` |
| C2 | Roles in Flutter | **PASS** (tourist, guide today) | tourist1, tourist2 and guide3 on the emulator today; manager/admin "use the web" screens carried over from 26 Sep |
| C3 | 403s | **PASS** | Covered by `roles.spec.ts` (approve as tourist, admin API as manager, catalogue as guide) and the endpoint tests |
| D1–D2 | Trips and attractions CRUD (A) | **PASS** | Endpoint tests in B1; live run on 26 Sep |
| D3 | Resources CRUD (B) | **PASS** | `GuidesEndpointsTests`, `VehiclesAndHotelsEndpointsTests` in B1; Guides page live (`docs/evidence/ui-after/web-7-guides.png`) |
| D4 | Quotations list and reports (C) | **PASS** | `QuotationsEndpointsTests`, report tests in B1; Reports page live (`ui-after/web-8-reports.png`) |
| D5 | Swagger documents every endpoint | **PASS** | Live `/swagger/v1/swagger.json`: **72 operations**; `SwaggerDocumentationTests` in B1 |
| E | Assessed workflow from the emulator | **PASS** (after fix 5) | See [E in detail](#e-assessed-workflow-from-the-emulator) |
| F1 | Budget 400 → RevisionRequested | **PASS** (after fix 6) | Live via `safe-failure.spec.ts`: Validation 18 s → RevisionRequested; the review page shows the failed budget rule and Approve is disabled |
| F2 | Agent service stopped → FailedSafely | **PASS** (26 Sep) | Not re-run; the live FailedSafely + **Try again** path was used again today in E |
| F3 | Injection objective | **PASS** (tests) | `test_injection.py`, `test_approval_enforcement.py` in B4; live run on 26 Sep treated it as data |
| F4 | Conflicting hold on approve → 409, no partial rows | **PASS** (tests) | `ApprovalWithRealResourcesPostgresTests`, `ApprovalTransactionPostgresTests` in B1 |
| G | Third-party fallbacks with hosts blocked | **PASS** (26 Sep) | `ExternalServicesSetupTests` and the 429 tests in B1; today the weather provider had no forecast for October/December (beyond its range) and the workflow continued (advisory) |
| H1–H4 | 400 / 404 / 401 / 500 ProblemDetails | **PASS** | `ErrorHandlingTests` in B1. Today's fix 7: handled 4xx are logged with their real status (verified live: 404 logged as 404) |
| I | Deployed system (Render, Vercel, real phone) | **Not run (manual)** | Nothing is deployed; steps in `docs/DEPLOYMENT.md` |
| I-iOS | `flutter build ios --no-codesign` | **Not run (manual)** | This Mac has no Xcode or CocoaPods (`flutter doctor`: "Xcode installation is incomplete"). Project, plist strings and plugins are ready: `docs/RUN-ON-IPHONE.md` |
| J1 | No secrets in the changes | **PASS** | Today's diff and new files scanned for DB passwords, JWT/internal keys, `gsk_`/`sk-`/`AIza`/`ghp_`/`npg_`, private keys: none. History scan from 26 Sep still applies |
| J2 | `.env.example` complete | **PASS** | `web/.env.example` adds `VITE_APK_URL`, `VITE_GROUP_NUMBER` |
| J3 | README reproducible from a fresh clone | **PASS** (26 Sep) | Not re-run; README now also covers routes, the landing page and the iPhone |
| J4 | Four CI workflows green | **Not run (manual)** | No GitHub remote; every CI step was run locally (A, B) |
| K1 | Every TODO listed | **PASS** | [Manual TODOs](#manual-todos-for-you); 94 `TODO` markers in `docs/report/` |
| K2 | Every link resolves | **PASS** | 55 Markdown files, 115 relative links including anchors: 0 broken |
| K3 | ADRs reference real paths | **PASS** | 37 backticked paths: all exist (the one non-path hit is the ESLint rule name `import/no-restricted-paths`) |
| L1 | Landing page, Lighthouse accessibility ≥ 90 | **PASS** | **100** (`docs/evidence/lighthouse-landing.json`); no horizontal scroll at 390 px |
| L2 | Before/after UI screenshots | **PASS** | `docs/evidence/ui-before/` and `ui-after/`: 8 React + 6 Flutter each, plus the landing page |

**Totals:** every check PASS except I, I-iOS and J4, which need your accounts, a GitHub remote or Xcode.

## E: assessed workflow from the emulator

The run used tourist1 on the release APK (`API_URL=http://10.0.2.2:5080`). The trip was:
- the PLAN.md demo objective, dates 10–14 Oct from the date-range picker;
- 4 travellers, USD 1,500, train + English guide;
- UK passport, with a passport photo taken with the **emulator camera**.

Screenshots are in `docs/evidence/final-run/`.

| Step | Result |
|------|--------|
| Submit → planning starts | **PASS**: trip created, photo stored, `start-planning` 202 |
| Runs 1 and 2 | **FailedSafely** at the Itinerary agent: "day 5: has 0 stops, allowed 1-3" after 2 repair retries. The phone showed the reason and **Try again** (`03-failed-safely.png`). This led to fix 5. |
| Run 3 (Try again after fix 5) | **PASS**: Planner 7.7 s, Itinerary 26.2 s (1 retry), Resource & Action 30.9 s, Validation & Safety 37.2 s, all Succeeded → **PendingApproval** (`04-pending-approval.png`) |
| Approve in React (manager1) | **PASS**: every deterministic check passed. The review shows Ruwan Fernando, Van CAB-1234 and rooms by name (`05-review.png`). "Approved. Trip is now confirmed" (`06-approved.png`). |
| PostgreSQL | quotation v1 **Approved**, LKR 188,370 = USD 570.34 (live rate 330.2776, not stale); `resource_holds` 1 guide + 1 vehicle + 4 room-nights; audit trail TripRequestCreated → … → AgentWorkflowStarted → AgentProposalReceived → StatusChanged |
| Confirmed on the phone | **PASS**: "Trip confirmed" notification, timeline **Confirmed**, tourist accepts the quotation (`07-confirmed-phone.png`, `08-accepted.png`) |
| Guide check-in | **PASS**: guide3 (Ruwan) sees the trip (`09-guide-schedule.png`). Emulator location set to the Temple of the Tooth, "0 m from …", Check in → `stop_check_ins` row, trip **InProgress** (`11-checked-in.png`) |

## What was fixed today

Each fix has a test that was run.

| # | Problem found | Root cause and fix | Test |
|---|---------------|--------------------|------|
| 1 | `npm run lint` failed on merged `main` | ESLint also linted `web/.vite/` (Vite's dependency cache). It is now ignored. | lint clean |
| 2 | Three files sat in one component's folder but served others | `TripAccess` → `Application/Common/Security/`; `ApprovedItinerary` → `Application/Workflows/`; `ReportQueries` → `Infrastructure/Persistence/Reporting/` | `TripAccessTests`, `ApprovedItineraryTests`, report tests |
| 3 | Approval review showed raw guide, vehicle and room ids | `WorkflowDto.ResourceNames`, labelled in `ProposalDetails.tsx` | `WorkflowsEndpointsTests`, `ApprovalReviewPage.test.tsx` |
| 4 | The Resource agent could say "no guide available" while it had picked one | `consistent_gaps` drops model gaps that contradict the selection | `test_model_gaps_that_contradict_the_selection_are_dropped` |
| 5 | **The section 6 demo failed safely at the Itinerary agent (twice)** | The seed has 4 attractions in Kandy + Ella for a 5-day trip, and the prompt said "do not repeat an attraction", so the model left the last day empty. The prompt now prefers unused attractions but never allows an empty day, and the 0-stops repair message says to repeat one. The 1–3 stops rule is still enforced in code. | `test_itinerary_empty_last_day_is_repaired_by_repeating_a_stop`; live run 3 passed with 1 retry |
| 6 | **`safe-failure.spec.ts` failed**: Validation timed out (120 s) on over-budget trips | The prompt asked the model to copy the whole quotation into `quotation_final`, which the node discards. It now asks for `null`. Validation went from 120 s to 18 s. | `test_validation_prompt_asks_for_no_quotation_copy`; e2e 6/6 |
| 7 | Handled 404/409 were logged as "responded 500" | Serilog request logging was inside the exception middleware; it is now outside (`Program.cs`) | verified live |
| 8 | Staff home was the only page; nothing for tourists | Public landing page at `/`, dashboard at `/dashboard`, role redirect kept | `LandingPage.test.tsx` (4), `guards.test.tsx` |
| 9 | iOS would crash on the first status notification | `flutter_local_notifications` needs iOS (Darwin) settings; added | `flutter analyze`, `flutter test` |

Noted, not changed:
- **Thin seed.** Kandy and Ella have 2 attractions each, so longer trips revisit them. Cities that are not seeded
  (e.g. Nuwara Eliya) fail safely with "No distance known between Nuwara Eliya and Kandy". This happened live
  with a "tea country" objective.
- **E2E dates.** The e2e specs share the 10–14 Oct demo dates, and each passing workflow run holds a guide and a
  vehicle. The suite needs a freshly seeded database or released holds; this is documented in `tests/e2e/README.md`.
  One earlier e2e trip's holds were released by SQL for today's run.
- **Guide choice.** The Resource agent picked Ruwan (LKR 7,000/day) although Nimal (LKR 6,000, free on those
  dates) is the cheapest English guide. The guide is the model's choice, only checked by the rules; rooms are
  code-computed. `docs/DEMO-SCRIPT.md` now says to log in as whichever guide the review page names.

## Manual TODOs for you

See the numbered list in [docs/COMPLIANCE.md → Manual TODO](COMPLIANCE.md#manual-todo-for-the-student). In short:
1. Push.
2. Lecturer approval.
3. B and C own their components.
4. GitHub and CI.
5. Deploy, including `VITE_APK_URL` and `VITE_GROUP_NUMBER`.
6. Xcode and iPhone signing.
7. Check the deployed system.
8. Optionally, seed more attractions.
9. Report screenshots.
10. The 94 report TODOs.
11. Individual sections.
12. The group AI declaration.
13. The video.
14. Viva practice.
15. Submission.
