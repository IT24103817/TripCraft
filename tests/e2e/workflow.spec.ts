import { expect, test } from '@playwright/test';
import {
  acceptQuotation,
  createTripAndStartPlanning,
  login,
  MANAGER,
  tripStatus,
  TOURIST,
  waitForWorkflow,
} from './helpers/api';
import { countResourceHolds } from './helpers/db';
import { loginInBrowser } from './helpers/ui';

/**
 * The v1.1 lifecycle (docs/API-V11.md): Tourist submits with cities (API) -> agents plan -> PendingReview ->
 * Operations Manager clicks "Send to client" in React -> QuotationSent (nothing booked) -> Tourist accepts (API)
 * -> ClientAccepted -> Manager clicks "Confirm trip" in React -> Confirmed and resources held.
 */
test('demo request is reviewed, sent, accepted by the client and confirmed in the browser', async ({
  page,
  request,
}) => {
  const tourist = await login(request, TOURIST);
  const { tripId, workflowId } = await createTripAndStartPlanning(request, tourist);

  const workflow = await waitForWorkflow(request, tourist, workflowId);
  expect(workflow.status, workflow.errorSummary ?? '').toBe('PendingApproval');
  expect(await tripStatus(request, tourist, tripId)).toBe('PendingReview');

  // 1. The manager reviews the proposal and sends it to the client.
  await loginInBrowser(page, MANAGER);
  await page.goto('/approvals');
  await page.getByRole('button', { name: `Open proposal ${workflowId.slice(0, 8)}` }).click();
  await expect(page.getByRole('list', { name: 'Validation checklist' })).toBeVisible();
  const banner = page.getByRole('region', { name: 'Trip status' });
  await expect(banner).toContainText('Pending review');
  await page.screenshot({ path: '../../docs/evidence/e2e/review-page.png', fullPage: true });

  const send = page.getByRole('button', { name: 'Send to client' });
  await expect(send).toBeEnabled();
  await send.click();
  await page.getByRole('dialog', { name: 'Send to client' }).getByRole('button', { name: 'Send to client' }).click();
  await expect(page.getByText(/Sent to the client\. Trip is now quotation sent/)).toBeVisible();
  await expect(banner).toContainText('Waiting for the client');
  expect(await tripStatus(request, tourist, tripId)).toBe('QuotationSent');
  // Nothing is booked before the client accepts.
  expect(await countResourceHolds(tripId)).toBe(0);

  // 2. The tourist accepts the quotation in the app (here through the API).
  await acceptQuotation(request, tourist, tripId);
  expect(await tripStatus(request, tourist, tripId)).toBe('ClientAccepted');

  // 3. The manager confirms: holds, itinerary, vouchers and email in one step.
  await page.reload();
  await expect(banner).toContainText('Client accepted');
  await banner.getByRole('button', { name: 'Confirm trip' }).click();
  await page.getByRole('dialog', { name: 'Confirm trip' }).getByRole('button', { name: 'Confirm trip' }).click();
  await expect(page.getByText(/Trip confirmed: \d+ holds created/)).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/confirmed-toast.png' });

  expect(await tripStatus(request, tourist, tripId)).toBe('Confirmed');
  expect(await countResourceHolds(tripId)).toBeGreaterThan(0);
});
