import { expect, test } from '@playwright/test';
import { createTripAndStartPlanning, login, MANAGER, tripStatus, TOURIST, waitForWorkflow } from './helpers/api';
import { countResourceHolds } from './helpers/db';
import { loginInBrowser } from './helpers/ui';

/**
 * PLAN.md section 6, the assessed workflow: Tourist submits (API) -> agents plan -> PendingApproval ->
 * Operations Manager approves in React -> trip Confirmed and resources held.
 */
test('demo request is planned, approved in the browser and confirmed', async ({ page, request }) => {
  const tourist = await login(request, TOURIST);
  const { tripId, workflowId } = await createTripAndStartPlanning(request, tourist);

  const workflow = await waitForWorkflow(request, tourist, workflowId);
  expect(workflow.status, workflow.errorSummary ?? '').toBe('PendingApproval');

  await loginInBrowser(page, MANAGER);
  await page.goto('/approvals');
  await page.getByRole('button', { name: `Open proposal ${workflowId.slice(0, 8)}` }).click();
  await expect(page.getByRole('list', { name: 'Validation checklist' })).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/approval-review.png', fullPage: true });

  await page.getByRole('button', { name: 'Approve' }).first().click();
  await page.getByRole('dialog', { name: 'Approve quotation' }).getByRole('button', { name: 'Approve' }).click();
  await expect(page.getByText(/Approved\. Trip is now confirmed/)).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/approved-toast.png' });

  expect(await tripStatus(request, tourist, tripId)).toBe('Confirmed');
  expect(await countResourceHolds(tripId)).toBeGreaterThan(0);
});
