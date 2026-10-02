import { expect, test } from '@playwright/test';
import {
  acceptQuotation,
  createTripAndStartPlanning,
  login,
  MANAGER,
  tripStatus,
  tripWorkflow,
  TOURIST,
  waitForTripStatus,
} from './helpers/api';
import { countResourceHolds } from './helpers/db';
import { loginInBrowser } from './helpers/ui';

/**
 * The v1.1 lifecycle (docs/API-V11.md): Tourist submits with cities (API) -> agents plan -> the proposal passes the
 * checks and is sent to the client automatically (QuotationSent, no manager step, nothing booked) -> Tourist
 * accepts (API) -> ClientAccepted -> Operations Manager opens the trip from the dashboard and presses Confirm in
 * React (the human approval gate) -> Confirmed and resources held.
 */
test('demo request is sent automatically, accepted by the client and confirmed in the browser', async ({
  page,
  request,
}) => {
  test.setTimeout(10 * 60_000); // the local 8B model can take several minutes for the four agents
  const tourist = await login(request, TOURIST);
  const { tripId } = await createTripAndStartPlanning(request, tourist);

  // 1. No manager step: the quotation reaches the client on its own.
  const status = await waitForTripStatus(request, tourist, tripId, ['QuotationSent']);
  const workflow = await tripWorkflow(request, tourist, tripId);
  expect(status, workflow.errorSummary ?? '').toBe('QuotationSent');
  expect(workflow.status).toBe('Approved');
  // Nothing is booked before the client accepts.
  expect(await countResourceHolds(tripId)).toBe(0);

  // 2. The tourist accepts the quotation in the app (here through the API).
  await acceptQuotation(request, tourist, tripId);
  expect(await tripStatus(request, tourist, tripId)).toBe('ClientAccepted');

  // 3. The manager finds the trip under "Accepted" on the dashboard and confirms it on the trip page.
  await loginInBrowser(page, MANAGER);
  const accepted = page.getByRole('list', { name: 'Accepted trips' });
  await accepted.locator(`a[href="/trips/${tripId}"]`).click();
  const panel = page.getByRole('region', { name: /^Next step/ });
  await expect(panel).toContainText('Accepted — confirm to book');
  await page.screenshot({ path: '../../docs/evidence/e2e/accepted-trip.png', fullPage: true });

  const confirm = panel.getByRole('button', { name: 'Confirm', exact: true });
  await expect(confirm).toBeEnabled();
  await confirm.click();
  await page.getByRole('dialog', { name: 'Confirm trip' }).getByRole('button', { name: 'Confirm trip' }).click();
  await expect(page.getByText(/Trip confirmed: \d+ holds created/)).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/confirmed-toast.png' });

  expect(await tripStatus(request, tourist, tripId)).toBe('Confirmed');
  expect(await countResourceHolds(tripId)).toBeGreaterThan(0);
});
