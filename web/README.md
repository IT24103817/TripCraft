# web — TripCraft staff app

React 18 + TypeScript (strict) + Vite staff app for **Operations Managers** and **Admins**
(PLAN.md sections 2 and 7). Tourists and Guides use the Flutter app; if they sign in here they
are sent to a "use the mobile app" page. The app talks **only** to the ASP.NET Core API.

## Setup

```bash
cd web
npm install
cp .env.example .env.local     # then set VITE_API_URL
npm run dev                    # http://localhost:5173
```

| Variable | Example | Purpose |
|----------|---------|---------|
| `VITE_API_URL` | `http://localhost:5080` | Base URL of the ASP.NET Core API, no trailing slash |

The API must allow this origin: `ALLOWED_ORIGINS=http://localhost:5173` (see the root README). If port
5173 is already taken, Vite picks the next free port — add that exact origin to `ALLOWED_ORIGINS` too, or
run `npm run dev -- --port 5199 --strictPort`.

## Scripts

| Script | What it does |
|--------|--------------|
| `npm run dev` | Vite dev server on port 5173 |
| `npm run build` | `tsc -b` (strict, zero errors) then a production build into `dist/` |
| `npm run preview` | Serves the production build |
| `npm run lint` | ESLint (TypeScript, React hooks, jsx-a11y, feature boundaries), zero warnings allowed |
| `npm run format` / `format:check` | Prettier |
| `npm test` | Vitest + React Testing Library + MSW (no API needed) |

## Test accounts (seeded by the API, password `Passw0rd!`)

| Role | Email | Sees |
|------|-------|------|
| Operations Manager | `manager1@tripcraft.test` | Everything except Users |
| Admin | `admin1@tripcraft.test` | Dashboard, Agent workflows, Users |
| Tourist / Guide | `tourist1@…`, `guide1@…` | "Use the mobile app" page |

## Folder structure

```
src/
  app/          router (lazy routes + RoleGuard per route), providers, layout, navigation, dashboard
  auth/         Zustand auth store (sessionStorage), login, ProtectedRoute, RoleGuard, 403, users/ (Admin)
  shared/       api (Axios + JWT/401 interceptors, types, query keys), components, hooks, utils, statuses
  features/
    trips/       trip list + detail (status timeline, itinerary, Start planning), attractions CRUD + map
    resources/   guides, vehicles, hotels, availability  (placeholders — see below)
    quotations/  approvals inbox + review, workflow monitor + step timeline, reports
  test/         MSW server, fixtures, renderApp helper (real routes, fresh providers)
```

**Boundaries (enforced by ESLint `import/no-restricted-paths`):** a feature never imports another
feature; `shared` never imports `auth`, `app` or features; `auth` never imports `app` or features. Only
`app` composes features (router, dashboard). Cross-feature cache refresh uses the shared root query keys
in `shared/api/queryKeys.ts` (e.g. approving a quotation invalidates `trips`).

Types in each feature's `types.ts` are copied from the C# DTOs named in the file header — change both together.

## Routes and roles

| Route | Roles | API |
|-------|-------|-----|
| `/` dashboard | OperationsManager, Admin | `GET /api/workflows`, `GET /api/trip-requests` |
| `/trips`, `/trips/:id` | OperationsManager | `GET /api/trip-requests`, `/{id}`, `/{id}/itinerary`, `POST /{id}/start-planning` |
| `/attractions` | OperationsManager | `GET/POST/PUT/DELETE /api/attractions` |
| `/approvals`, `/approvals/:workflowId` | OperationsManager | `GET /api/workflows?status=`, `GET /api/workflows/{id}`, `POST /api/quotations/{id}/approve \| reject \| request-revision` |
| `/workflows`, `/workflows/:id` | OperationsManager, Admin | `GET /api/workflows`, `/{id}`, `/{id}/steps` (polls every 5 s while Planning) |
| `/reports` | OperationsManager | requests by status from `GET /api/trip-requests` totals |
| `/admin/users` | Admin | `GET/POST /api/admin/users`, `POST /{id}/deactivate` |
| `/resources/*`, `/availability` | OperationsManager | not in the API yet |

The approval review page reads the itinerary, proposed resources, quotation (LKR, USD, FX rate and
as-of time) and the deterministic `ValidationResult` from the workflow's `finalOutcome` and
`validationResult`. The checklist rules in `features/quotations/validationRules.ts` mirror the codes
in `ProposalValidator.cs`.

## Not built yet (waiting for Students B and C)

These screens show a placeholder that names the missing endpoints instead of invented data:

- Guides, vehicles, hotels + room types CRUD and the availability calendar — Resource Management (Student B).
- Revenue by month and guide utilisation charts, and the "revenue this month" KPI — `/api/reports/*` (Student C).
- Guide, vehicle and room **names** on the approval page (ids are shown until Resource Management exists).

## Known API limits reflected in the UI

- `GET /api/workflows` only filters by status and always sorts newest first, so the workflow lists have
  a status filter/tabs but no search or column sorting.
- `GET /api/admin/users` returns the whole list, so search, filters, sorting and paging on the Users page
  happen in the browser.
- `POST /api/trip-requests/{id}/start-planning` is currently **Tourist-only** in the API, so the
  "Start planning" button in this app gets a 403 for a manager (shown as an error toast).
