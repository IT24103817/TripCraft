# Student C scheduled publication — October 2, 2026

Ten prepared and tested changes are published to `IT24103079`, starting from
`ac2f049eeed39b38a9cfa42ada6adbea1b046766`. The laptop is not involved after setup:
GitHub-hosted runners do the work. Existing commit history is never rewritten.

| Sri Lanka time | Change |
| --- | --- |
| 08:00 | Decision comment boundaries |
| 09:00 | Nonpositive quotation inputs and fractional quantities |
| 10:00 | Leap days and inclusive reporting boundaries |
| 11:00 | Utilisation ordering, idle resources and capacity |
| 12:00 | FX cancellation, timeout and recovery |
| 13:00 | Approval hold IDs and room grouping |
| 14:00 | Saved itinerary ordering and parent links |
| 15:00 | Quotation description fallbacks |
| 16:00 | Workflow JSON persistence |
| 17:00 | Internal availability capacity boundaries |

Each run overlays Student C's backend onto the pinned shared project at
`IT24103817/TripCraft@fab245e65f4f41412c5e818dda330d2417f46c43`, runs all quotation
and workflow .NET tests, and builds the solution. Only then can it commit and push
the next due item. Build dependencies are never added to the Student C branch.

This queue contains 55 new test cases. The complete queue passed 235 tests locally.
The first `push` run validates the full queue in the cloud without committing.
Git commit messages explicitly identify automated publication of prepared changes;
timestamps are the actual publication time, with no backdating.

GitHub schedules may run late or occasionally be dropped. A second trigger ten
minutes after each target time retries failures. The publisher checks successful
commit trailers and file hashes, so retries cannot duplicate a change. It publishes
at most one new change per run, never before that change's target time. If a slot is
missed, a later run can publish the oldest due item; exact hourly completion is not
guaranteed. A user commit made after the baseline pauses publication with a failure
instead of merging or overwriting work.

After ten successful publications, the workflow disables itself. A date/year guard
also prevents writes outside October 2, 2026 (Asia/Colombo). `workflow_dispatch`
defaults to **validate**; **publish** can retry a due item during that date only.
Test results are attached to each run. Disable the workflow in GitHub Actions to
cancel the remaining queue. No API keys, PATs or additional secrets are stored;
the workflow uses its repository-scoped `GITHUB_TOKEN`.

## Recovery when GitHub does not emit cron events

On October 2 at 11:58 Sri Lanka time, no scheduled workflow events had appeared,
despite the workflow being active and its initial validation succeeding. The exact
GitHub scheduler cause is unknown. `workflow_dispatch` mode **recover** starts one
cloud runner immediately. It publishes overdue items with their actual current
timestamps, then waits in that runner for each remaining original hourly slot.
Every item still goes through the same tests, history checks and idempotency guards.
This avoids relying on another cron event while the recovery job is running. The
job is limited to six hours, so launch it within roughly five hours of the final
17:00 slot. It needs no running laptop. Its failure remains visible in Actions;
restart recovery if an infrastructure failure interrupts it.
