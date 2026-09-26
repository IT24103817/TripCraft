# 5. API, React and Flutter design

## API

Swagger: `/swagger` (JWT **Authorize** button). Endpoint table by component: README "API documentation".
Conventions:

- DTOs only (records in `TripCraft.Application/*/Dtos`), FluentValidation for each (`*/Validators`).
- Lists return `{items, page, pageSize, total}` with `search`, filters, whitelisted `sort` (`-field` descending), `page`, `pageSize` ≤ 100.
- Status codes: 201 create, 202 start-planning (work continues in the background), 204 delete, 400 validation,
  401 no/invalid token, 403 role or ownership, 404, 409 conflicts (status workflow, duplicate, running workflow,
  hold overlap), 429 login rate limit, 503 component or database unavailable.
- Internal API for the agents under `/api/internal/*` with `X-Internal-Key`, excluded from JWT.

TODO: screenshot of Swagger with the endpoint groups.

## React staff app (`web/`)

Vite + React 18 + TypeScript strict; `src/app` (router, providers, layout), `src/auth`, `src/shared`, and
`src/features/{trips,resources,quotations}`. A feature never imports another feature (ESLint
`import/no-restricted-paths`). Lazy routes with `ProtectedRoute` and `RoleGuard`; Zustand for the session;
TanStack Query for server data; shared `DataTable` (server-side sort and paging), `SearchFilterBar` (debounced),
`FormField` (react-hook-form + zod), `PageState` (loading, empty, error with retry, data), `ConfirmDialog`,
toasts; screens for trips, attractions (map preview), approvals with the validation checklist, workflow timeline
(5 s polling while Planning), reports, users. Resource screens are placeholders naming the missing endpoints.

TODO: screenshots — login, dashboard, trips list, trip detail, approval review, workflow timeline, 360 px layout.

## Flutter app (`mobile/`)

Riverpod 3 (code generation), go_router with a redirect by auth and role, dio with a JWT interceptor and 401
logout, freezed models; `lib/core` (config, api, auth, router, storage), `lib/shared` (theme, widgets, utils),
`lib/features/{trips,resources,quotations}`. Device features: camera/gallery (passport photo), date-range picker,
GPS (check-in distance, 500 m rule), QR scanner (hotel vouchers), local notifications (status changes), OpenStreetMap.
Every screen uses `AsyncView` for the four states; layouts are tested at 360×640 and 412×915.

TODO: screenshots — login, trip form, my trips, trip detail with timeline and map, quotation, alerts, guide check-in.
