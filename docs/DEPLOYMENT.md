# Deploying TripCraft (free tiers)

| Part | Platform | Config in the repo |
|------|----------|--------------------|
| PostgreSQL | **Neon** (free) | — (migrations run from the API) |
| ASP.NET Core API | **Render** web service, Docker (free) | `render.yaml`, `backend/Dockerfile` |
| React staff app | **Vercel** (Hobby) | `web/vercel.json` |
| Agent service | **your laptop with Ollama** (demo default) or **Render with Groq** (optional) | `agents/Dockerfile`, `render.yaml` (commented block) |
| Android app | **GitHub Release** `v1.0` | `mobile/scripts/build-release-apk.sh`, `docs/APK-INSTALL.md` |

**Order:** Neon → Render API → Vercel web → agent service → APK. The startup order when running is
PostgreSQL → Ollama → agents → API → web → mobile (see `agents/README.md`).

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
   `tripcraft-api` (Docker, free, health check `/health`).
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

1. vercel.com → **Add New → Project** → import the repo → **Root Directory** `web` (framework Vite is detected;
   `web/vercel.json` sets the build, SPA rewrite and security headers).
2. **Environment Variables:** `VITE_API_URL=https://tripcraft-api.onrender.com` (no trailing slash). It is baked in
   at build time: redeploy after changing it.
3. **Deploy** → note the URL, e.g. `https://tripcraft.vercel.app`.
4. Back on Render: set the API's `ALLOWED_ORIGINS` to that exact URL (comma-separate several, e.g. a preview URL)
   and redeploy, or the browser's CORS check blocks every call.

`[screenshot: docs/evidence/screenshots/deploy-vercel-env.png]` `[screenshot: docs/evidence/screenshots/deploy-vercel-login.png]`

## 4. Agent service

**Mode A — laptop + Ollama (default for the demo, allowed by PLAN.md section 12).**
Run it as in `agents/README.md`. The Render API must reach it over HTTPS, so expose it with a tunnel:
`cloudflared tunnel --url http://localhost:8001` → set the API's `AGENT_SERVICE_URL` to the printed
`https://…trycloudflare.com` URL (it changes every run) and redeploy. The agent's `API_BASE_URL` is the Render
API URL. Alternative: run the API locally for the agent part of the demo.

**Mode B — Render + Groq (optional, always online).** Uncomment the `tripcraft-agents` block in `render.yaml`,
sync the Blueprint, fill `GROQ_API_KEY`, `INTERNAL_AGENT_KEY` (same as the API's) and
`API_BASE_URL=https://tripcraft-api.onrender.com`. Then set the API's
`AGENT_SERVICE_URL=https://tripcraft-agents.onrender.com`.

In both modes the API calls the agents with a 10 s timeout (one retry); if the agent service is asleep or
down, planning ends `FailedSafely` and the tourist can retry — so wake it first (section 7).

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
| `RUN_MIGRATIONS` | yes on Render | `true`: apply EF migrations and seed demo data on start (idempotent). |
| `ORS_API_KEY` | no | OpenRouteService key; without it the seeded `city_distances` table is used. **Secret.** |
| `OWM_API_KEY` | no | OpenWeatherMap key; without it weather is skipped. **Secret.** |
| `AGENT_CALLBACK_BASE_URL` | no | URL the agents post results to, if different from the agent's own `API_BASE_URL`. |
| `FX_FALLBACK_LKR_PER_USD` | no | Rate used (flagged stale) if the FX provider fails before any success; default 300. |
| `UPLOADS_DIR` | no | Passport photo folder; the image sets `/app/uploads`. |
| `PORT` | set in `render.yaml` | `8080`, the port Render routes to. |
| `ASPNETCORE_ENVIRONMENT` | set in `render.yaml` | `Production` (JSON logs; Swagger stays on at `/swagger` for the assessment). |

### Agent service

| Name | Required | Meaning |
|------|----------|---------|
| `INTERNAL_AGENT_KEY` | yes | Same value as the API's. **Secret.** |
| `API_BASE_URL` | yes | The API, for tool calls and callbacks, e.g. `https://tripcraft-api.onrender.com`. |
| `LLM_PROVIDER` | yes | `ollama` (Mode A) or `groq` (Mode B). |
| `OLLAMA_MODEL`, `OLLAMA_BASE_URL` | Mode A | `llama3.1:8b`, `http://localhost:11434` (from Docker: `http://host.docker.internal:11434`). |
| `GROQ_API_KEY`, `GROQ_MODEL` | Mode B | Groq key (**secret**) and `llama-3.1-8b-instant`. |
| `NODE_TIMEOUT_SECONDS`, `MAX_RETRIES`, `MAX_REPLANS` | no | 30 / 2 / 3 by default. Use 60–120 s for a slow laptop. |

### Web (Vercel) and mobile

| Name | Where | Meaning |
|------|-------|---------|
| `VITE_API_URL` | Vercel env var, build time | API base URL for the React app. |
| `API_URL` | `--dart-define` when building the APK | API base URL compiled into the Android app. |

## 7. Waking the Render free service before a demo

Free web services sleep after 15 minutes without traffic; the first request then takes ~50 s.

1. **5 minutes before** the demo (and before submitting links), open `https://tripcraft-api.onrender.com/health`
   and wait for `"db":"ok"`. Do the same for `https://tripcraft-agents.onrender.com/health` in Mode B.
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

Run after every deploy and before the demo. Record screenshots in `docs/evidence/screenshots/`.

| # | Check | Expected |
|---|-------|----------|
| 1 | `GET https://<api>/health` | `200` `{"status":"ok","version":"1.0.0+…","db":"ok"}` |
| 2 | `https://<api>/swagger` | Swagger UI loads; **Authorize** accepts a token |
| 3 | `POST /api/auth/login` as `manager1@tripcraft.test` / `Passw0rd!` | `200` with `accessToken`; wrong password → `401` |
| 4 | `GET /api/trip-requests` with the token | `200` paged list (seeded completed trip included) |
| 5 | Web: `https://<vercel>/login` as the manager | Dashboard loads with KPI cards, no CORS error in the console |
| 6 | Tourist submits a trip (APK or Swagger) → **Start planning** | `202`; `GET /api/workflows/{id}` moves from `Planning` to a final status; agent steps appear in `/workflows/{id}` |
| 7 | Manager: **Approvals** → open the proposal → **Approve** | Toast "Approved. Trip is now confirmed"; trip status `Confirmed` |
| 8 | Tourist on the phone pulls to refresh | Trip shows **Confirmed** |
| 9 | Security: tourist token on `POST /api/quotations/{id}/approve` | `403` |

Until Resource Management (Student B) and Quotations (Student C) are merged, step 6 ends `FailedSafely` at the
Resource agent (their endpoints answer 503) and steps 7–8 cannot be done; steps 1–6 and 9 work.

## Local rehearsal with Docker

```bash
docker build -t tripcraft-api backend
docker run -p 8080:8080 -e DATABASE_URL="postgresql://…" -e JWT_SECRET="$(openssl rand -base64 48)" \
  -e JWT_ISSUER=tripcraft-local -e ALLOWED_ORIGINS=http://localhost:5173 -e INTERNAL_AGENT_KEY=… \
  -e AGENT_SERVICE_URL=http://host.docker.internal:8001 -e RUN_MIGRATIONS=true tripcraft-api
curl localhost:8080/health
```

This exact flow was verified against an empty PostgreSQL 16 database: all 4 migrations applied, 12 tables,
12 users, 8 attractions and 6 city distances seeded, `/health` → `{"status":"ok","version":"1.0.0","db":"ok"}`,
Swagger served in Production, login `200`, container user `app` (uid 1654).
