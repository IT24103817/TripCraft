# Deploying TripCraft (free tiers)

| Part | Platform | Config in the repo |
|------|----------|--------------------|
| PostgreSQL | **Neon** (free) | — (migrations run from the API) |
| ASP.NET Core API | **Render** web service, Docker (free) | `render.yaml`, `backend/Dockerfile` |
| React staff app | **Vercel** (Hobby) | `web/vercel.json` |
| Agent service | **Render with Groq** (hosted) or **your laptop with Ollama** | `agents/Dockerfile`, `render.yaml` (`tripcraft-agents`) |
| Android app | **GitHub Release** `v1.0` | `mobile/scripts/build-release-apk.sh`, `docs/APK-INSTALL.md` |

**Order:** Neon → Render API → Vercel web → agent service → APK. The startup order when running is
PostgreSQL → Ollama → agents → API → web → mobile (see `agents/README.md`).

**Step-by-step checklist with the smoke tests:** [DEPLOY-CHECKLIST.md](DEPLOY-CHECKLIST.md).

**Secrets:** every value below is entered in the platform's dashboard. Never commit them; the repo only has
names (`.env.example`, `render.yaml` with `sync: false`).

---

## 1. PostgreSQL on Neon

1. Sign up at neon.tech → **New project** `tripcraft`, region close to Render's (Singapore → `aws-ap-southeast-1`), Postgres 16.
2. **Dashboard → Connect** → choose the **direct** connection (not "pooled": migrations need a normal session),
   copy the URL. It looks like
   `postgresql://neondb_owner:<password>@ep-xxxx.ap-southeast-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require`.
   The API accepts this URL form as-is.
3. Nothing else to do: the API creates the tables and seed data on its first start (`RUN_MIGRATIONS=true`).

`[screenshot: docs/evidence/screenshots/deploy-neon-connection.png]`
After the first API start: `[screenshot: docs/evidence/screenshots/deploy-neon-tables.png]` (Tables view).

## 2. API on Render

1. render.com → **New → Blueprint** → connect the GitHub repo → Render reads `render.yaml` and proposes
   `tripcraft-api` and `tripcraft-agents` (Docker, free, health check `/health`).
2. Fill in the blank environment variables (table below). Set `RUN_MIGRATIONS=true`.
3. **Apply**. The first build takes ~5 minutes. Watch the logs (JSON lines) for
   `RUN_MIGRATIONS=true: applying database migrations` and `Seeded 12 demo users`.
4. Open `https://tripcraft-api.onrender.com/health` → `{"status":"ok","version":"1.0.0+<commit>","db":"ok",…}`
   and `/swagger`.

`[screenshot: docs/evidence/screenshots/deploy-render-env.png]` `[screenshot: docs/evidence/screenshots/deploy-render-health.png]`

The image listens on 8080 (`ASPNETCORE_URLS`); `render.yaml` sets `PORT=8080` so Render routes to it.
It runs as a non-root user. Passport photos are stored in `/app/uploads`, which is **not persistent** on the
free tier (lost on redeploy); fine for the demo, use object storage for real use.

## 3. React app on Vercel

**Vercel deploys from the fork mirror** `https://github.com/IT24103817/TripCraft` (git remote `fork`), not from the
group repository. The fork's `main` is a mirror of the group's `main`: after pushing to `origin`, run
`scripts/sync-fork.sh` (`git push fork main`, a normal push) so Vercel builds the same commit. Never force-push
`origin`; if the fork's `main` ever diverges, only the fork may be updated with `git push --force-with-lease fork main`.

1. vercel.com → **Add New → Project** → import the **fork** repo → **Root Directory** `web` (framework Vite is detected;
   `web/vercel.json` sets the build, SPA rewrite and security headers).
2. **Environment Variables:** `VITE_API_URL=https://tripcraft-api.onrender.com` (no trailing slash). It is baked in
   at build time: redeploy after changing it.
3. **Deploy** → note the URL, e.g. `https://tripcraft.vercel.app`.
4. Back on Render: set the API's `ALLOWED_ORIGINS` to that exact URL (comma-separate several, e.g. a preview URL)
   and redeploy, or the browser's CORS check blocks every call.

`[screenshot: docs/evidence/screenshots/deploy-vercel-env.png]` `[screenshot: docs/evidence/screenshots/deploy-vercel-login.png]`

## 4. Agent service

**Hosted — Render + Groq (default).** The Blueprint creates `tripcraft-agents` with `LLM_PROVIDER=groq`,
`GROQ_MODEL=qwen/qwen3.8-27b`, `NODE_TIMEOUT_SECONDS=90` and `RATE_LIMIT_MAX_WAIT_SECONDS=60`. Fill in
`GROQ_API_KEY` (console.groq.com; free tier), `INTERNAL_AGENT_KEY` (same as the API's) and
`API_BASE_URL=https://tripcraft-api.onrender.com`. Then set the API's
`AGENT_SERVICE_URL=https://tripcraft-agents.onrender.com` and redeploy the API. If `LLM_PROVIDER` is left unset on
Render, the agent service picks Groq by itself (`RENDER=true`). Groq's free tier allows 1,000 requests a day,
8,000 tokens a minute and 200,000 tokens a day; one planning run uses about 12,000 tokens, so each trip waits once for
Groq's `retry-after` (shown as a warning on that agent's step), trips should start about two minutes apart, and about
16 fit in a day. Model choice and limits: `agents/README.md`, "Groq free tier".

**Local — laptop + Ollama.** Run it as in `agents/README.md`. A Render API must reach it over HTTPS, so expose it
with a tunnel: `cloudflared tunnel --url http://localhost:8001` → set the API's `AGENT_SERVICE_URL` to the printed
`https://…trycloudflare.com` URL (it changes every run) and redeploy. Alternative: run the API locally too.

Gemini (`LLM_PROVIDER=gemini`, `GEMINI_API_KEY`) also works, but its free tier allows only 20 requests a day.

In both modes the API calls the agents with a 10 s timeout (one retry); if the agent service is asleep or
down, planning ends safely (the trip shows **Needs operator**) and a manager can retry — so wake it first
(section 7).

## 5. Android APK

`./mobile/scripts/build-release-apk.sh https://tripcraft-api.onrender.com`, then publish GitHub Release `v1.0`
with the APK — full steps in `docs/APK-INSTALL.md`.

---

## 6. Environment variables

### API (Render)

| Name | Required | Meaning |
|------|----------|---------|
| `DATABASE_URL` | yes | Neon connection string (URL or `Host=…;` form). **Secret.** |
| `JWT_SECRET` | yes | HMAC key that signs login tokens; at least 32 bytes (`openssl rand -base64 48`). **Secret.** |
| `JWT_ISSUER` | yes | Issuer and audience in every token, e.g. `tripcraft-prod`. |
| `ALLOWED_ORIGINS` | yes | Browser origins allowed by CORS: the Vercel URL(s), comma-separated. |
| `INTERNAL_AGENT_KEY` | yes | Shared secret between API and agent service (`X-Internal-Key`), both directions. **Secret.** |
| `AGENT_SERVICE_URL` | yes | Base URL of the agent service. Missing → planning ends `FailedSafely`. |
| `LLM_PROVIDER` | set in `render.yaml` | `groq`: what the Admin Settings page shows and sends until an Admin saves a choice. |
| `RUN_MIGRATIONS` | yes on Render | `true`: apply EF migrations and seed demo data on start (idempotent). |
| `ORS_API_KEY` | no | OpenRouteService key; without it the seeded `city_distances` table is used. Existing keys work on HeiGIT's new host unchanged. **Secret.** |
| `OWM_API_KEY` | no | OpenWeatherMap key; without it weather is skipped. **Secret.** |
| `AGENT_CALLBACK_BASE_URL` | no | URL the agents post results to, if different from the agent's own `API_BASE_URL`. |
| `FX_FALLBACK_LKR_PER_USD` | no | Rate used (flagged stale) if the FX provider fails before any success; default 300. |
| `ORS_BASE_URL` | no | OpenRouteService base URL; default `https://api.heigit.org/openrouteservice/` (`api.openrouteservice.org` is deprecated and shuts down on 2–6 Nov 2026). The older name `ORS_API_BASE_URL` still works. |
| `FX_API_BASE_URL`, `OWM_API_BASE_URL` | no | Override a provider's base URL; leave unset in production. Used to test the fallbacks by pointing a provider at an unreachable host. |
| `UPLOADS_DIR` | no | Passport photo folder; the image sets `/app/uploads`. |
| `PORT` | set in `render.yaml` | `8080`, the port Render routes to. |
| `ASPNETCORE_ENVIRONMENT` | set in `render.yaml` | `Production` (JSON logs; Swagger stays on at `/swagger` for the assessment). |

### Agent service

| Name | Required | Meaning |
|------|----------|---------|
| `INTERNAL_AGENT_KEY` | yes | Same value as the API's. **Secret.** |
| `API_BASE_URL` | yes | The API, for tool calls and callbacks, e.g. `https://tripcraft-api.onrender.com`. |
| `LLM_PROVIDER` | set in `render.yaml` | `groq` (hosted) or `ollama` (local); unset → `groq` on Render, `ollama` elsewhere. `gemini` is optional. |
| `GROQ_API_KEY`, `GROQ_MODEL` | hosted | console.groq.com key (**secret**) and `qwen/qwen3.8-27b`. |
| `RATE_LIMIT_RETRIES`, `RATE_LIMIT_BACKOFF_SECONDS`, `RATE_LIMIT_MAX_WAIT_SECONDS` | no | 3 / 2 / 20 (hosted Groq: 60): on a 429 or 503 wait Groq's `retry-after` (or 2, 4, 8 s) and retry, then fail safely. |
| `OLLAMA_MODEL`, `OLLAMA_BASE_URL` | local | `llama3.1:8b`, `http://localhost:11434` (from Docker: `http://host.docker.internal:11434`). |
| `GEMINI_API_KEY`, `GEMINI_MODEL` | no | Optional provider; free tier 20 requests a day. |
| `NODE_TIMEOUT_SECONDS`, `MAX_RETRIES`, `MAX_REPLANS` | no | 30 / 2 / 3 by default; hosted Groq 90 s (waits for the token limit). Use 60–120 s for Ollama on a slow laptop. |

### Web (Vercel) and mobile

| Name | Where | Meaning |
|------|-------|---------|
| `VITE_API_URL` | Vercel env var, build time | API base URL for the React app. |
| `API_URL` | `--dart-define` when building the APK | API base URL compiled into the Android app. |

## 7. Waking the Render free service before a demo

Free web services sleep after 15 minutes without traffic; the first request then takes ~50 s.

1. **5 minutes before** the demo (and before submitting links), open `https://tripcraft-api.onrender.com/health`
   and wait for `"db":"ok"`. Do the same for `https://tripcraft-agents.onrender.com/health`.
   Neon's free compute also suspends when idle; this same request wakes it (the first `db` may take a few seconds).
2. Keep one browser tab on Swagger or the dashboard during the demo; each click keeps it awake.
3. Optional: a free uptime monitor (e.g. UptimeRobot) pinging `/health` every 10 minutes during the demo day only.
   Remove it afterwards to stay within free-tier hours.

## 8. Rotating secrets

| Secret | How | Effect |
|--------|-----|--------|
| `JWT_SECRET` | New value (`openssl rand -base64 48`) in Render → **Save, rebuild and deploy** | Every signed-in user is logged out (their tokens stop validating) and must log in again. |
| `INTERNAL_AGENT_KEY` | Generate (`openssl rand -hex 32`), set the **same** value on the API and the agent service, redeploy both | Until both are updated, planning ends `FailedSafely` (401 between them). |
| Neon password | Neon → **Roles → Reset password** → paste the new URL into `DATABASE_URL` on Render → redeploy | Brief downtime while the API restarts. |
| `GROQ_API_KEY`, `ORS_API_KEY`, `OWM_API_KEY` | Revoke in the provider's console, create a new key, update Render, redeploy | Old key stops working immediately. |

Rotate immediately if a secret ever appears in a commit, a screenshot, a log or a chat. Removing it from git
history is not enough — the old value must be revoked.

## 9. Smoke-test checklist

The smoke tests for the v1.1 flow (quotation sent automatically, client accepts, manager confirms) are in
[DEPLOY-CHECKLIST.md](DEPLOY-CHECKLIST.md), section 5. Run them after every deploy and before the demo.

## Local rehearsal with Docker

```bash
docker build -t tripcraft-api backend
docker run -p 8080:8080 -e DATABASE_URL="postgresql://…" -e JWT_SECRET="$(openssl rand -base64 48)" \
  -e JWT_ISSUER=tripcraft-local -e ALLOWED_ORIGINS=http://localhost:5173 -e INTERNAL_AGENT_KEY=… \
  -e AGENT_SERVICE_URL=http://host.docker.internal:8001 -e RUN_MIGRATIONS=true tripcraft-api
curl localhost:8080/health
```

This exact flow was verified against an empty PostgreSQL 16 database: all 4 migrations applied, 12 tables,
12 users, 8 attractions and 6 city distances seeded (the seed now has 21 attractions and 15 distances; see `docs/diagrams/er.md`), `/health` → `{"status":"ok","version":"1.0.0","db":"ok"}`,
Swagger served in Production, login `200`, container user `app` (uid 1654).
