import { expect, test } from '@playwright/test';
import { createTripAndStartPlanning, login, MANAGER, tripStatus, TOURIST, waitForWorkflow } from './helpers/api';
import { loginInBrowser } from './helpers/ui';

/**
 * PLAN.md section 6, second path, in the v1.1 lifecycle: budget USD 400 -> over budget. The trip still comes
 * back for review (PendingReview) with the OVER_BUDGET warning, and "Send to client" stays disabled.
 */
test('an over-budget request comes back for review with a warning and cannot be sent', async ({ page, request }) => {
  const tourist = await login(request, TOURIST);
  const { tripId, workflowId } = await createTripAndStartPlanning(request, tourist, 400);

  const workflow = await waitForWorkflow(request, tourist, workflowId);
  expect(workflow.status, workflow.errorSummary ?? '').toBe('RevisionRequested');
  const codes = (workflow.validationResult?.violations ?? []).map((v: { code: string }) => v.code);
  expect(codes).toContain('OVER_BUDGET');
  expect(await tripStatus(request, tourist, tripId)).toBe('PendingReview');

  await loginInBrowser(page, MANAGER);
  await page.goto(`/approvals/${workflowId}`);
  const checklist = page.getByRole('list', { name: 'Validation checklist' });
  await expect(checklist).toContainText("Total is within the tourist's budget — failed");
  await expect(page.getByRole('region', { name: 'Trip status' })).toContainText('Pending review');
  await expect(page.getByRole('button', { name: 'Send to client' })).toBeDisabled();
  await expect(page.getByText(/over the budget\)\. Request a revision, or edit and re-price/)).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/over-budget-review.png', fullPage: true });
});
