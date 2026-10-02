import { expect, test } from '@playwright/test';
import {
  createTripAndStartPlanning,
  login,
  MANAGER,
  sentQuotation,
  tripWorkflow,
  TOURIST,
  waitForTripStatus,
} from './helpers/api';
import { loginInBrowser } from './helpers/ui';

/**
 * The budget rule (docs/API-V11.md "Auto-send and the budget rule"): budget USD 400 is too low for the demo trip.
 * The agents re-plan with the lowest-cost strategy; when the total is still over the budget the quotation is sent
 * anyway as the best available price, with the "Best price we can offer" sentence. Real agents may re-plan
 * several times, so the wait is up to 6 minutes.
 */
test('an over-budget request is sent automatically at the best price we can offer', async ({ page, request }) => {
  test.setTimeout(9 * 60_000);
  const tourist = await login(request, TOURIST);
  const { tripId } = await createTripAndStartPlanning(request, tourist, 400);

  const status = await waitForTripStatus(request, tourist, tripId, ['QuotationSent'], 6 * 60_000);
  const workflow = await tripWorkflow(request, tourist, tripId);
  expect(status, workflow.errorSummary ?? '').toBe('QuotationSent');

  const quotation = await sentQuotation(request, tourist, tripId);
  expect(quotation.bestAvailablePrice).toBe(true);
  expect(quotation.overBudgetUsd).toBeGreaterThan(0);
  expect(quotation.budgetNote).toMatch(/^Best price we can offer — USD [\d,]+\.\d{2} above your budget$/);

  // The manager sees the same sentence on the trip's Quotation tab, on the version that was sent.
  await loginInBrowser(page, MANAGER);
  await page.goto(`/trips/${tripId}?tab=quotation`);
  const version = page.getByRole('article', { name: `Quotation version ${quotation.version}` });
  await expect(version).toContainText('Sent to client');
  await expect(version.getByText(quotation.budgetNote as string)).toBeVisible();
  await page.screenshot({ path: '../../docs/evidence/e2e/best-price-quotation.png', fullPage: true });
});
