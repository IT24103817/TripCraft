import { expect, test, type Page } from '@playwright/test';
import { API_URL, PASSWORD } from './helpers/api';

/**
 * PLAN.md section 2: each seeded role sees only its permitted screens, and the API refuses the rest.
 * The API check reuses the browser session's token (the login endpoint allows 5 attempts per minute).
 */
async function signIn(page: Page, email: string) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(PASSWORD);
  await page.getByRole('button', { name: 'Sign in' }).click();
}

async function sessionToken(page: Page): Promise<string> {
  const raw = await page.evaluate(() => sessionStorage.getItem('tripcraft-auth'));
  return JSON.parse(raw ?? '{}').state.token as string;
}

async function status(page: Page, method: 'GET' | 'POST', path: string): Promise<number> {
  const token = await sessionToken(page);
  const response = await page.request.fetch(`${API_URL}${path}`, {
    method,
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    data: method === 'POST' ? {} : undefined,
  });
  return response.status();
}

const nav = (page: Page) => page.getByRole('navigation', { name: 'Main' });

test('Operations Manager sees operations screens, not user admin; admin API is 403', async ({ page }) => {
  await signIn(page, 'manager1@tripcraft.test');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  for (const link of ['Dashboard', 'Approvals', 'Trip requests', 'Attractions', 'Guides', 'Vehicles', 'Hotels',
    'Availability', 'Agent workflows', 'Reports']) {
    await expect(nav(page).getByRole('link', { name: link })).toBeVisible();
  }
  await expect(nav(page).getByRole('link', { name: 'Users' })).toHaveCount(0);
  await expect(nav(page).getByRole('link', { name: 'Audit log' })).toHaveCount(0);
  await page.goto('/admin/users');
  await expect(page.getByText('You do not have access to this page')).toBeVisible();
  expect(await status(page, 'GET', '/api/admin/users')).toBe(403);
  expect(await status(page, 'GET', '/api/admin/audit-logs')).toBe(403);
});

test('Admin sees users, audit log and workflows only; the approval inbox is 403', async ({ page }) => {
  await signIn(page, 'admin1@tripcraft.test');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  for (const link of ['Dashboard', 'Agent workflows', 'Users', 'Audit log']) {
    await expect(nav(page).getByRole('link', { name: link })).toBeVisible();
  }
  for (const link of ['Approvals', 'Trip requests', 'Attractions', 'Reports']) {
    await expect(nav(page).getByRole('link', { name: link })).toHaveCount(0);
  }
  await page.goto('/approvals');
  await expect(page.getByText('You do not have access to this page')).toBeVisible();
  expect(await status(page, 'POST', `/api/quotations/${crypto.randomUUID()}/approve`)).toBe(403);
  expect(await status(page, 'GET', '/api/admin/audit-logs')).toBe(200);
});

test('Tourist is sent to the mobile app; staff pages and approve are 403', async ({ page }) => {
  await signIn(page, 'tourist1@tripcraft.test');
  await expect(page.getByRole('heading', { name: 'Please use the TripCraft mobile app' })).toBeVisible();
  await page.goto('/approvals');
  await expect(page.getByText('You do not have access to this page')).toBeVisible();
  expect(await status(page, 'POST', `/api/quotations/${crypto.randomUUID()}/approve`)).toBe(403);
  expect(await status(page, 'GET', '/api/workflows')).toBe(403);
});

test('Guide is sent to the mobile app; catalogue changes and trips are 403', async ({ page }) => {
  await signIn(page, 'guide1@tripcraft.test');
  await expect(page.getByRole('heading', { name: 'Please use the TripCraft mobile app' })).toBeVisible();
  await page.goto('/trips');
  await expect(page.getByText('You do not have access to this page')).toBeVisible();
  // Catalogue and resource CRUD is the Operations Manager's job; a guide may not change any of it.
  expect(await status(page, 'POST', '/api/attractions')).toBe(403);
  expect(await status(page, 'POST', '/api/guides')).toBe(403);
  expect(await status(page, 'POST', '/api/vehicles')).toBe(403);
  expect(await status(page, 'GET', '/api/trip-requests')).toBe(403);
});
