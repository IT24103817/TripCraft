# 7. Testing report

TODO: 6–10 pages. Commands, counts and the PLAN.md section 11 mapping: `docs/TEST-EVIDENCE.md` (paste the
relevant tables here with screenshots).

## Summary (latest local run)

| Layer | Tool | Tests | Result |
|-------|------|------:|--------|
| Backend unit, integration, database | xUnit, FluentAssertions, Moq, WebApplicationFactory, Testcontainers / PostgreSQL 16 | 197 | all pass |
| Agent evaluation | pytest, respx, FakeLLM | 43 | all pass |
| React | Vitest, React Testing Library, MSW | 25 | all pass |
| Flutter | flutter_test, mocktail | 44 | all pass |
| End to end | Playwright | 2 | fail — the Resource agent's first tool gets 503 until Student B merges |
| Performance | k6 | 3 scripts | see section 9 |

Backend per folder: Trips 74, Workflows 46 (+ External 18), Quotations 21, Identity 16, Shared/Database 15, Common 7.

## What the tests prove

- Business operations: `TripPlanningServiceTests`, `TripPlanningRulesTests`, `ProposalValidatorTests`, `QuotationApprovalServiceTests`.
- Status codes 201/204/400/401/403/404/409 per component; auth: wrong password 401, expired/re-signed/tampered token 401, tourist on approve 403, login rate limit 429.
- Database (real PostgreSQL): migrations from empty, column types, model = migrations, 8 constraint violations, approval rollback, start-up migration and seeding.
- Clients: validation, protected routes and redirects, lists and paging, error state with retry, approve calls the API, secure storage, 401 → logout.

TODO: screenshots of `dotnet test`, `pytest`, `npm test`, `flutter test`, and the four green GitHub Actions runs
(`docs/evidence/screenshots/`).
TODO: final e2e run after Students B and C merge (`docs/evidence/e2e/`).
