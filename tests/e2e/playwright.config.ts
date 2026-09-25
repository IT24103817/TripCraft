import { defineConfig, devices } from '@playwright/test';

/**
 * End-to-end tests of the PLAN.md section 6 workflow against a running stack.
 *   BASE_URL          React app            (default http://localhost:5173)
 *   API_URL           ASP.NET Core API     (default http://localhost:5080)
 *   E2E_DATABASE_URL  PostgreSQL, only to count resource_holds (postgres://user:pass@host:port/db)
 * The agent service must be running (the API calls it). Screenshots and traces go to docs/evidence/e2e.
 */
export default defineConfig({
  testDir: '.',
  testMatch: '*.spec.ts',
  timeout: 5 * 60_000,
  workers: 1, // the specs share seeded users and the login rate limit (5 per minute)
  retries: 0,
  reporter: [['list'], ['html', { outputFolder: '../../docs/evidence/e2e/report', open: 'never' }]],
  outputDir: '../../docs/evidence/e2e/results',
  use: {
    baseURL: process.env.BASE_URL ?? 'http://localhost:5173',
    screenshot: 'on',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
