# End-to-end tests (Playwright)

Runs the PLAN.md section 6 workflow against a running stack: PostgreSQL → agent service → API → React.

```bash
cd tests/e2e
npm install
npx playwright install chromium
BASE_URL=http://localhost:5173 API_URL=http://localhost:5080 \
E2E_DATABASE_URL=postgres://user:pass@localhost:5432/tripcraft \
npx playwright test
```

Against the deployed system: set `BASE_URL` to the Vercel URL and `API_URL` to the Render URL.

| Spec | Proves |
|------|--------|
| `workflow.spec.ts` | Tourist creates the demo request (with cities) and starts planning (API) → the proposal passes the checks and the trip reaches `QuotationSent` on its own (no manager step), no `resource_holds` yet → Tourist accepts (API) → `ClientAccepted` → Operations Manager opens the trip from the dashboard's **Accepted** list and presses **Confirm** in the browser → trip `Confirmed`, `resource_holds` rows exist |
| `budget.spec.ts` | Budget USD 400 → the agents re-plan with the lowest-cost strategy, the trip still reaches `QuotationSent` automatically with `bestAvailablePrice = true`, and the manager's trip Quotation tab shows "Best price we can offer — USD X above your budget" (allows up to 6 minutes for real agents) |
| `roles.spec.ts` | Each seeded role sees only its screens; the API refuses the rest (403) |

Screenshots, traces and the HTML report are written to `docs/evidence/e2e/`.

Both lifecycle specs submit the PLAN.md demo trip (4 travellers, Kandy and Ella) on **fresh dates**:
`freshTripDates()` in `helpers/api.ts` picks a random 5-day window 30-729 days after today for every trip. Reruns
never compete with the holds of earlier runs or demos, so the suite needs no database clean-up.
