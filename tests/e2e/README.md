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
| `workflow.spec.ts` | Tourist creates the demo request and starts planning (API) → workflow reaches `PendingApproval` within 3 min → Operations Manager approves in the browser → trip `Confirmed`, `resource_holds` rows exist |
| `safe-failure.spec.ts` | Budget USD 400 → `RevisionRequested`; the review page shows the failed budget rule and Approve is disabled |

Screenshots, traces and the HTML report are written to `docs/evidence/e2e/`.

Both workflow specs submit the PLAN.md demo trip (4 travellers, Kandy and Ella) on **fresh dates**:
`freshTripDates()` in `helpers/api.ts` picks a random 5-day window 30-729 days after today for every trip. Reruns
never compete with the holds of earlier runs or demos, so the suite needs no database clean-up.
