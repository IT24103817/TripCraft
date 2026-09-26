# ADR-001: React state management

- **Status:** Accepted
- **Date:** 2026-09-26
- **Author:** Student C (drafted during the build; to be reviewed and defended by the author)

## Context

The staff app (`web/`) has two kinds of state: a little client state (who is signed in, their role, the open
dialog) and a lot of server state (trip lists, workflows, steps, approvals) that must refresh after mutations
and show loading, error and empty states on every page. We had four days, three developers working on separate
feature folders, free tooling only, and every line must be explainable at the viva.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **Context API only** | Built into React, nothing to install. Fine for the auth user. | We would hand-write fetching, caching, refetch and loading/error flags on every page. Every context update re-renders all consumers. |
| **Redux Toolkit (+ RTK Query)** | One well-documented pattern; RTK Query handles caching. | Slices, store setup and action boilerplate for a small app. Its patterns take time to learn for students new to it — risky in four days. |
| **Zustand + TanStack Query** | Zustand: a store in ~20 lines, no provider, easy to persist to `sessionStorage`. TanStack Query: caching, `isLoading`/`isError`/`refetch`, `invalidateQueries` after mutations. | Two libraries instead of one. Query keys must be designed so one feature can refresh another's data without importing it. |

## Decision

**Zustand** for the auth session (token, user, role) and **TanStack Query** for all server data.

## Consequences

- Each page's four states come straight from the query (`isLoading`, `isError` + `refetch`, empty data, data)
  through one `PageState` component.
- Mutations invalidate by shared root keys (`trips`, `workflows`, …), so approving a quotation refreshes trips
  without the quotations feature importing the trips feature (the ESLint boundary rule forbids that).
- The token lives in `sessionStorage` (cleared when the tab closes). XSS could read it; the Vercel CSP
  (`script-src 'self'`) limits that risk.
- 4xx errors are not retried; 5xx once.

## Where this shows in the code

- `web/src/auth/authStore.ts` — Zustand store persisted to `sessionStorage`
- `web/src/app/queryClient.ts` — stale time and retry policy
- `web/src/shared/api/queryKeys.ts` — root keys shared across features
- `web/src/features/trips/api.ts`, `web/src/features/quotations/api.ts` — queries and mutations (`useQuotationDecision` invalidates `trips` and `workflows`)
- `web/src/shared/components/PageState.tsx` — the four page states
- `web/eslint.config.js` — `import/no-restricted-paths` feature boundaries
