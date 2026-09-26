# ADR-005: Cloud deployment platform

- **Status:** Accepted
- **Date:** 2026-09-26
- **Author:** Student B (drafted during the build; to be reviewed and defended by the author)

## Context

Evaluators must reach a live API (health + Swagger), a live React app and a downloadable APK. The assignment
requires no-cost services; none of us has a credit card to spare, and there are four days. The API is .NET 8
with PostgreSQL; the web app is a static Vite build.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **Azure App Service (student credit)** | First-class .NET hosting; managed PostgreSQL available. | Student credit and account verification take time and can run out; more portal configuration than we can learn in the build window. |
| **Render + Neon + Vercel** | All three have free tiers with no card. Render builds our `Dockerfile` from the repo (`render.yaml` Blueprint); Neon is serverless PostgreSQL 16; Vercel hosts the Vite build with preview URLs. | Render's free service sleeps after 15 minutes (≈ 50 s cold start) and has no persistent disk; Neon's compute also suspends when idle. |
| **Railway** | Simple deploys, good DX. | The free allowance is small and time-limited; a card is often required to keep services running. |

## Decision

**Neon** for PostgreSQL, **Render** (Docker, free) for the API, **Vercel** for the React app, the APK on a
**GitHub Release**.

## Consequences

- The API image is portable: multi-stage .NET 8, non-root user, port 8080; `RUN_MIGRATIONS=true` applies
  migrations and seeds on start, so Neon needs no manual setup.
- Before a demo we must wake Render and Neon (`GET /health`); documented in `docs/DEPLOYMENT.md`.
- Behind Render's proxy the API trusts one forwarded hop so HTTPS and client IPs are correct (the login rate
  limit is per IP).
- Uploaded passport photos are lost on redeploy (no free persistent disk) — acceptable for the assignment.

## Where this shows in the code

- `render.yaml` — the Blueprint (secrets left blank with `sync: false`)
- `backend/Dockerfile`, `backend/.dockerignore`
- `backend/src/TripCraft.Api/Program.cs` — `RUN_MIGRATIONS`, forwarded headers, JSON logs
- `backend/src/TripCraft.Api/Controllers/HealthController.cs` — `{status, version, db}` with a real DB ping
- `web/vercel.json` — SPA rewrite and security headers
- `docs/DEPLOYMENT.md`
