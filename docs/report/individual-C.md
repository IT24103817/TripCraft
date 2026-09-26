# Individual report — Student C

**Name:** TODO  **Student ID:** TODO  **GitHub username:** TODO

## 1. Contribution statement

TODO: one paragraph in your own words — what you built, what you reviewed, what you integrated. State clearly
which files below you wrote yourself, which you reviewed, and which were written with AI assistance
(cross-reference your AI log). The Git history is the evidence.

## 2. Owned component and technical work

| | |
|---|---|
| Component | Quotation, Approval & Reporting |
| Agent | Validation & Safety |
| Third-party integration | Exchange rate (open.er-api.com) |
| Business operation | Quotation calculation with LKR→USD conversion and approve / reject / revise in one transaction (`QuotationApprovalService`, `ProposalValidator`; implements `IQuotationStore`). |
| ADRs to defend | ADR-001 (React state), ADR-004 (agent workflow state schema) |
| Flutter screens | quotation view, accept quotation, notifications |
| React screens | approval inbox, agent workflow monitor, reports dashboard |

### Files in this component (generated from the repository)

> **Status at the time of writing:** the approval transaction, deterministic validation and workflow monitor are
> in the repository; the quotation entities and store (`IQuotationStore`), the quotation list and the reports API
> are **not built yet** (placeholders answer 503). Add your new folders to this list as you build them.

- `agents/app/nodes/validation.py`
- `agents/app/tools/calculate_quotation.py`
- `agents/app/tools/check_business_rules.py`
- `agents/tests/test_validation.py`
- `backend/src/TripCraft.Api/Controllers/Internal/InternalKeyAuthFilter.cs`
- `backend/src/TripCraft.Api/Controllers/Internal/InternalToolsController.cs`
- `backend/src/TripCraft.Api/Controllers/Internal/InternalWorkflowsController.cs`
- `backend/src/TripCraft.Api/Controllers/Quotations/QuotationApprovalsController.cs`
- `backend/src/TripCraft.Api/Controllers/Workflows/WorkflowsController.cs`
- `backend/src/TripCraft.Application/Quotations/IQuotationApprovalService.cs`
- `backend/src/TripCraft.Application/Quotations/QuotationApprovalService.cs`
- `backend/src/TripCraft.Application/Quotations/QuotationDecisionRequests.cs`
- `backend/src/TripCraft.Application/Quotations/QuotationDecisionValidators.cs`
- `backend/src/TripCraft.Application/Workflows/AgentStep.cs`
- `backend/src/TripCraft.Application/Workflows/AgentWorkflow.cs`
- `backend/src/TripCraft.Application/Workflows/AgentWorkflowStatus.cs`
- `backend/src/TripCraft.Application/Workflows/ComponentNotAvailableException.cs`
- `backend/src/TripCraft.Application/Workflows/Dtos/AgentProposalRequest.cs`
- `backend/src/TripCraft.Application/Workflows/Dtos/AgentStepReportRequest.cs`
- `backend/src/TripCraft.Application/Workflows/Dtos/InternalToolQueries.cs`
- `backend/src/TripCraft.Application/Workflows/Dtos/ProposalValidationResult.cs`
- `backend/src/TripCraft.Application/Workflows/Dtos/WorkflowDtos.cs`
- `backend/src/TripCraft.Application/Workflows/External/CityDistance.cs`
- `backend/src/TripCraft.Application/Workflows/External/IDistanceService.cs`
- `backend/src/TripCraft.Application/Workflows/External/IExchangeRateService.cs`
- `backend/src/TripCraft.Application/Workflows/External/IWeatherService.cs`
- `backend/src/TripCraft.Application/Workflows/IAgentServiceClient.cs`
- `backend/src/TripCraft.Application/Workflows/IAgentWorkflowRepository.cs`
- `backend/src/TripCraft.Application/Workflows/Ports/IQuotationStore.cs`
- `backend/src/TripCraft.Application/Workflows/Ports/IResourceCatalog.cs`
- `backend/src/TripCraft.Application/Workflows/Ports/IResourceHoldService.cs`
- `backend/src/TripCraft.Application/Workflows/ProposalFacts.cs`
- `backend/src/TripCraft.Application/Workflows/ProposalQuotationCheck.cs`
- `backend/src/TripCraft.Application/Workflows/ProposalValidator.cs`
- `backend/src/TripCraft.Application/Workflows/Services/IWorkflowProposalService.cs`
- `backend/src/TripCraft.Application/Workflows/Services/IWorkflowQueryService.cs`
- `backend/src/TripCraft.Application/Workflows/Services/IWorkflowStepService.cs`
- `backend/src/TripCraft.Application/Workflows/Services/WorkflowProposalService.cs`
- `backend/src/TripCraft.Application/Workflows/Services/WorkflowQueryService.cs`
- `backend/src/TripCraft.Application/Workflows/Services/WorkflowStepService.cs`
- `backend/src/TripCraft.Application/Workflows/Validation/AgentProposalRequestValidator.cs`
- `backend/src/TripCraft.Application/Workflows/Validation/AgentStepReportRequestValidator.cs`
- `backend/src/TripCraft.Application/Workflows/Validation/InternalToolQueryValidators.cs`
- `backend/src/TripCraft.Application/Workflows/Validation/WorkflowListQueryValidator.cs`
- `backend/src/TripCraft.Application/Workflows/WorkflowJson.cs`
- `backend/src/TripCraft.Application/Workflows/WorkflowOutcome.cs`
- `backend/src/TripCraft.Infrastructure/External/ExchangeRateService.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/AgentServiceClient.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/AgentStepConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/CityDistanceConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/PendingComponents.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/WorkflowsSeeder.cs`
- `backend/src/TripCraft.Infrastructure/Workflows/WorkflowsSetup.cs`
- `backend/tests/TripCraft.Tests/Quotations/QuotationApprovalServiceTests.cs`
- `backend/tests/TripCraft.Tests/Quotations/QuotationApprovalTests.cs`
- `backend/tests/TripCraft.Tests/Quotations/QuotationDecisionValidatorsTests.cs`
- `backend/tests/TripCraft.Tests/Quotations/QuotationStatusCodeTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/AgentContractTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/AgentServiceClientTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/DistanceServiceTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/ExchangeRateServiceTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/StubHandler.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/WeatherServiceTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/Fakes/FakeAgentAndExternal.cs`
- `backend/tests/TripCraft.Tests/Workflows/Fakes/FakeQuotations.cs`
- `backend/tests/TripCraft.Tests/Workflows/Fakes/FakeResources.cs`
- `backend/tests/TripCraft.Tests/Workflows/InternalEndpointsTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/ProposalEndpointTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/ProposalValidatorTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/TestProposals.cs`
- `backend/tests/TripCraft.Tests/Workflows/WorkflowFlow.cs`
- `backend/tests/TripCraft.Tests/Workflows/WorkflowStatusCodeTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/WorkflowValidatorsTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/WorkflowsEndpointsTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/WorkflowsListFlowTests.cs`
- `docs/adr/ADR-001-react-state-management.md`
- `docs/adr/ADR-004-agent-workflow-state-schema.md`
- `mobile/lib/features/quotations/application/quotation_providers.dart`
- `mobile/lib/features/quotations/application/status_watcher.dart`
- `mobile/lib/features/quotations/data/local_notifications.dart`
- `mobile/lib/features/quotations/data/quotation_models.dart`
- `mobile/lib/features/quotations/data/quotations_repository.dart`
- `mobile/lib/features/quotations/presentation/notifications_screen.dart`
- `mobile/lib/features/quotations/presentation/quotation_screen.dart`
- `mobile/test/quotations/quotation_screen_test.dart`
- `tests/e2e/README.md`
- `tests/e2e/helpers/api.ts`
- `tests/e2e/helpers/db.ts`
- `tests/e2e/helpers/ui.ts`
- `tests/e2e/playwright.config.ts`
- `tests/e2e/safe-failure.spec.ts`
- `tests/e2e/workflow.spec.ts`
- `web/src/features/quotations/ApprovalReviewPage.tsx`
- `web/src/features/quotations/ApprovalsPage.tsx`
- `web/src/features/quotations/DecisionActions.tsx`
- `web/src/features/quotations/ProposalDetails.tsx`
- `web/src/features/quotations/QuotationPanel.tsx`
- `web/src/features/quotations/ReportsPage.tsx`
- `web/src/features/quotations/StepTimeline.tsx`
- `web/src/features/quotations/ValidationChecklist.tsx`
- `web/src/features/quotations/WorkflowDetailPage.tsx`
- `web/src/features/quotations/WorkflowsPage.tsx`
- `web/src/features/quotations/__tests__/ApprovalReviewPage.test.tsx`
- `web/src/features/quotations/__tests__/WorkflowDetailPage.test.tsx`
- `web/src/features/quotations/api.ts`
- `web/src/features/quotations/revisionSchema.ts`
- `web/src/features/quotations/types.ts`
- `web/src/features/quotations/validationRules.ts`
- `web/src/features/quotations/workflowColumns.tsx`

TODO: for three of these files, explain the design in 3–5 sentences each (controller → service → repository, one
migration/constraint, your agent's contract and tools).

## 3. Key commits, pull requests and test evidence

| Evidence | Link / screenshot |
|----------|-------------------|
| Commits (e.g. `git log --author=<you> --oneline`) | TODO |
| Pull requests authored | TODO |
| Pull requests reviewed | TODO |
| Test runs for your component (backend, React, Flutter, agent) | TODO screenshots in `docs/evidence/screenshots/` |
| CI runs | TODO links |

## 4. Challenges and learning

TODO: at least two technical challenges, how you diagnosed them, what you changed, and what you learned.

## 5. AI usage log

`docs/ai-log-<your-name>.md` (from `docs/ai-log-template.md`): date, tool and model, task, what it produced, what
was changed or rejected, how it was verified. TODO: create it and keep it consistent with your commits.

## 6. Reflection (one page, hand-written in your own words)

TODO — to be written by you, not generated.

## 7. Declaration

I declare that this individual report describes my own contribution, that my AI usage is fully recorded in my
AI log, and that I can explain and modify the code of my component.

Signature: ____________________  Date: __________
