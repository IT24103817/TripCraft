import { expect, test } from '@playwright/test';
import { createTripAndStartPlanning, login, MANAGER, TOURIST, waitForWorkflow } from './helpers/api';
import { loginInBrowser } from './helpers/ui';

/** PLAN.md section 6, second path: budget USD 400 -> over budget -> RevisionRequested, shown on the review page. */
test('an over-budget request ends RevisionRequested and the review page says why', async ({ page, request }) => {
  const tourist = await login(request, TOURIST);
  const { workflowId } = await createTripAndStartPlanning(request, tourist, 400);

  const workflow = await waitForWorkflow(request, tourist, workflowId);
  expect(workflow.status, workflow.errorSummary ?? '').toBe('RevisionRequested');

  await loginInBrowser(page, MANAGER);
  await page.goto(`/approvals/${workflowId}`);
  const checklist = page.getByRole('list', { name: 'Validation checklist' });
  await expect(checklist).toContainText("Total is within the tourist's budget — failed");
  await expect(page.getByText('Revision requested').first()).toBeVisible();
  await expect(page.getByRole('button', { name: 'Approve' })).toBeDisabled();
  await page.screenshot({ path: '../../docs/evidence/e2e/revision-requested.png', fullPage: true });
});
