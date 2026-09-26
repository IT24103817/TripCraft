# ADR-004: Agent workflow state schema

- **Status:** Accepted
- **Date:** 2026-09-26
- **Author:** Student C (drafted during the build; to be reviewed and defended by the author)

## Context

The React workflow monitor shows each run's status, plan, validation result and a timeline of agent steps; the
approval inbox lists workflows by status; auditors must be able to reconstruct what happened. PLAN.md section 4
says only state and summaries may be stored — never raw prompts or hidden reasoning. The agents' outputs
(plans, days, quotations) are nested JSON whose shape may still change during the build.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **One jsonb blob per workflow** | Simplest to write; any shape fits. | Hard to list "all PendingApproval workflows" or order steps; no constraints; the timeline needs custom JSON queries. |
| **Fully relational** (tables for plan steps, days, stops, tool calls…) | Every field queryable and constrained. | Many tables and migrations for data that is mostly displayed, not queried; every agent-contract change needs a migration — too slow for 4 days. |
| **Hybrid**: `agent_workflows` + `agent_steps` rows, jsonb for nested summaries | Status, timings, step order and agent name are real columns (indexed, constrained); nested outputs stay flexible jsonb. | jsonb content is not validated by the database beyond being valid JSON, so the API validates size and shape. |

## Decision

**Hybrid.** `agent_workflows` (status, current step, timings, error summary as columns; `plan`, `final_outcome`,
`validation_result` as jsonb) and `agent_steps` (step number, agent, tools, duration, retries, status as columns;
input/output/validation summaries as jsonb).

## Consequences

- The inbox and monitor use indexed columns: `(status, started_at)` on workflows, unique `(workflow_id, step_no)` on steps.
- Summaries are capped at 8,000 characters each by `AgentStepReportRequestValidator`, which also keeps raw
  prompts out; the agent service only sends summaries.
- `final_outcome` holds the proposal and, after approval, the decision and the holds created.
- Real PostgreSQL tests check the jsonb columns and constraints (`backend/tests/TripCraft.Tests/Shared/Database`).

## Where this shows in the code

- `backend/src/TripCraft.Application/Workflows/AgentWorkflow.cs`, `AgentStep.cs`, `WorkflowOutcome.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/AgentWorkflowConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/AgentStepConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Migrations/20260925202645_AddAgentWorkflows.cs`
- `backend/src/TripCraft.Application/Workflows/Validation/AgentStepReportRequestValidator.cs` — 8,000-character cap
- `backend/tests/TripCraft.Tests/Shared/Database/ConstraintTests.cs`
