# Addendum: TripCraft v1.1 (after the v1.0 submission)

> **v1.0 is the submitted state.** The assessed submission is tag `v1.0` (commit `fab245e`, 28 September 2026),
> and the consolidated report built by `build.sh` describes that state. This addendum is not part of the built
> report. It records what changed afterwards in tag `v1.1.1` and why, so a reader of the repository can tell the two
> apart.

## What changed and why

| Change in v1.1 | Why |
|----------------|-----|
| **Quotations go straight to the client; Confirm is the human approval gate.** A proposal that passes the deterministic `ProposalValidator` becomes a sent quotation automatically (actor `System`). The tourist accepts or declines (with a reason) in the app. An Operations Manager then **Confirms** the accepted quotation in React. Confirm is manager-only and is the only place guide, vehicle and rooms are held, in one transaction. One state machine (`TripStatusMachine`) owns the trip statuses: Submitted → Planning → QuotationSent → ClientAccepted → Confirmed → InProgress → Completed, plus ClientDeclined, NeedsOperator and Cancelled. An illegal move returns 409. | In v1.0 the operator reviewed every proposal before the client saw a price, so the operator sat between the agents and the client. The plan's rule that a human approves before anything is booked is kept: nothing is held before Confirm. |
| **Budget rule.** When the total is over budget, the agents re-plan with a lowest-cost strategy (budget rooms, the cheapest eligible guide and vehicle, fewer paid entries). If the total is still over, the quote is sent with "Best price we can offer — USD X above your budget". A Hard rule never sends; the trip goes to NeedsOperator. | A budget that is too small is a fact for the client to decide on, not an error. A broken rule (overlapping hold, too few seats, wrong language) must never reach a client. |
| **Operator actions.** The dashboard tiles are Accepted — confirm, Declined — needs a decision, Needs operator, Guide change requests and Cancellations. The trip page offers Confirm, Edit & resend (the client must accept again), Replan with note, Retry planning and Cancel with reason. | The operator works only on trips that need a person, and every action leaves an audited history entry. |
| **Mood packages.** The app's home screen offers five packages (`trip_templates`), priced from today's rate cards, with "Book as is" or "Customize with the planner". | Most tourists start from an idea, not a blank form. A package is a booked trip in two taps, and the planner is still one tap away. |
| **Guide accounts.** A manager creates a guide's account with a temporary password; the guide must change it at first login and can request a replacement guide. | v1.0 had no safe way to give a guide a login; public registration only creates tourists. |
| **Vouchers and check-in.** Confirm issues HMAC-signed QR vouchers (one for the trip, one per hotel night) and a voucher PDF. The guide scans the trip voucher (or uses GPS within 500 m) to check in; the first check-in sets InProgress and the last one Completed. | A voucher proves the booking at the hotel and at each stop, and a signed code cannot be forged. GPS stays as the fallback. |
| **Notifications.** In-app notifications appear on the web bell and, through 30-second polling, as phone notifications. An open trip screen reloads when a notification about that trip arrives. | The client must learn about a sent or updated quote, and the manager about an accepted or declined one, without refreshing. |
| **Email as the fourth third-party integration.** The Mailtrap sandbox sends "Your quotation is ready" and "Your trip is confirmed". Without a token, emails go to an `.eml` pickup folder; sending happens after the database commit. | The client is told even when the app is closed, and a mail failure can never undo a booking. |
| **Settings.** An Admin Settings page holds the LLM provider (Ollama or Groq), the cancellation notice, the margin and the deposit, stored in the database and read per request. | Operators change policy without a redeploy. |
| **Availability grid.** Guides, vehicles and rooms by day, with Confirmed holds and manual maintenance blocks. | The manager sees at a glance what is free before editing a trip. |
| **Dark mode and PDF.** Dark mode across the staff web (tokens only). The itinerary PDF includes the quotation and deposit, and there is a voucher PDF. | Accessibility and printable documents for the client and the hotels. |

The full contract is in [../API-V11.md](../API-V11.md) and [../API-V11-WEB.md](../API-V11-WEB.md). The state diagram
is in [../diagrams/workflow.md](../diagrams/workflow.md).

## Database

The v1.0 chain had 6 EF Core migrations; v1.1 adds 5, giving one linear chain of 11. They add the lifecycle,
trip templates and guide ratings, settings and deposits, the best-price fields, and the mapping of retired statuses.
`dotnet ef migrations has-pending-model-changes` reports none, and all 11 apply cleanly to an empty database.

## Test results

**Run date: 5 October 2026**, on `main` after merging the remote work (commit `aae3ffd`). The GitHub Actions runs
on the group repository passed with the same counts.

| Suite | v1.0 (28 Sep 2026) | v1.1 (5 Oct 2026) |
|-------|--------------------|-------------------|
| `dotnet build -warnaserror` | 0 warnings | 0 warnings |
| `dotnet test` (unit, integration, real PostgreSQL) | 333 passed | **467 passed** |
| `ruff check` + `pytest` (agents) | 58 passed | **64 passed** |
| `npm run lint && npm test && npm run build` (web) | 82 passed | **204 passed** (36 files) |
| `flutter analyze && flutter test` (mobile) | 68 passed | **186 passed** |
| Playwright end to end (real Ollama agents) | 6/6 | **6/6** |

Device runs of the v1.1 flow on the Android emulator (submit → automatic quote on the phone → accept → manager
Confirm → vouchers) are recorded in [../evidence/v1.1-e2e.md](../evidence/v1.1-e2e.md).
