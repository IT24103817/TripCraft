# Deploy checklist (hosted with Groq; Ollama stays the local provider)

**This deployment:** API https://tripcraft-api-h37l.onrender.com · agents https://tripcraft-agents.onrender.com ·
web https://trip-craft-sepia.vercel.app · APK
[Release v1.1.1-hosted](https://github.com/ilhamhilmy63/TripCraft/releases/tag/v1.1.1-hosted).

Tick each box in order. The details behind every step are in [DEPLOYMENT.md](DEPLOYMENT.md). Never paste a secret
into a file, a commit, a screenshot or a chat: every value marked **secret** goes only into the platform dashboard.

## 1. Before you start

- [ ] **Groq key** (**secret**): console.groq.com → **API Keys** → create a key (free tier). Check that the key can
      use the model: `qwen/qwen3.8-27b` must appear in `GET https://api.groq.com/openai/v1/models` for that key.
      Free tier: 1,000 requests a day, 8,000 tokens a minute, 200,000 tokens a day (one planning run uses about
      7,000, so about 28 trips a day). Do not run the live test suite against the demo key on the demo day.
- [ ] **Generate the shared secrets** (**secret**), once:
      `openssl rand -base64 48` for `JWT_SECRET`, `openssl rand -hex 32` for `INTERNAL_AGENT_KEY` and for
      `VOUCHER_SIGNING_KEY`.
- [ ] **Neon**: a `tripcraft` project; copy the **direct** connection string for `DATABASE_URL` (**secret**).
- [ ] Optional: `MAILTRAP_API_TOKEN` and `MAILTRAP_INBOX_ID` (without them emails go to the pickup folder),
      `ORS_API_KEY`, `OWM_API_KEY`.

## 2. Render (Blueprint from `render.yaml`)

- [ ] render.com → **New → Blueprint** → this repo. Render proposes `tripcraft-api` and `tripcraft-agents`.
- [ ] **tripcraft-agents**: `LLM_PROVIDER=groq`, `GROQ_MODEL=qwen/qwen3.8-27b`, `NODE_TIMEOUT_SECONDS=90` and
      `RATE_LIMIT_MAX_WAIT_SECONDS=60` come from `render.yaml`. Enter `GROQ_API_KEY`, `INTERNAL_AGENT_KEY`, and
      `API_BASE_URL=https://tripcraft-api-h37l.onrender.com`.
- [ ] **tripcraft-api**: `LLM_PROVIDER=groq` comes from `render.yaml`. Enter `DATABASE_URL`, `JWT_SECRET`,
      `JWT_ISSUER` (e.g. `tripcraft-prod`), the same `INTERNAL_AGENT_KEY`,
      `AGENT_SERVICE_URL=https://tripcraft-agents.onrender.com`, `VOUCHER_SIGNING_KEY`, `OPERATOR_CONTACT`,
      `RUN_MIGRATIONS=true`, and `ALLOWED_ORIGINS` (set it after step 3).
- [ ] **Apply.** In the API logs: `RUN_MIGRATIONS=true: applying database migrations` and the seed lines.
- [ ] `https://tripcraft-agents.onrender.com/health` → `{"status":"ok"}`.

## 3. Vercel (React)

Vercel deploys from the **fork mirror** `https://github.com/IT24103817/TripCraft` (git remote `fork`), not from the
group repository. After every push to `origin/main`, run `scripts/sync-fork.sh` (a normal `git push fork main`) so
Vercel builds the same commit; never force-push `origin`.

- [ ] Import the **fork** repo in Vercel. Root Directory `web`; `VITE_API_URL=https://tripcraft-api-h37l.onrender.com` (no trailing slash) → **Deploy**.
- [ ] Put the Vercel URL (`https://trip-craft-sepia.vercel.app`) into the API's `ALLOWED_ORIGINS` on Render →
      redeploy the API.

## 4. Wake everything (5 minutes before testing or the demo)

- [ ] `https://tripcraft-api-h37l.onrender.com/health` → wait for `"db":"ok"` (the first call can take about 50 s).
- [ ] `https://tripcraft-agents.onrender.com/health` → `{"status":"ok"}`.

## 5. Smoke tests

| # | Check | Expected |
|---|-------|----------|
| 1 | `GET https://tripcraft-api-h37l.onrender.com/health` | `200`, `"status":"ok"`, `"db":"ok"` |
| 2 | `https://tripcraft-api-h37l.onrender.com/swagger` | Swagger loads; **Authorize** accepts a token |
| 3 | `POST /api/auth/login` as `manager1@tripcraft.test` / `Passw0rd!`; then a wrong password | `200` with `accessToken`; `401` |
| 4 | Web `https://trip-craft-sepia.vercel.app/login` as the manager | Dashboard with "Needs your action" tiles; no CORS error in the console |
| 5 | Web as `admin1@tripcraft.test` → **Settings** | LLM provider shows **Groq** selected |
| 6 | Tourist (app or Swagger) submits a trip with cities Kandy, Ella → **Start planning** | `202`; the trip goes Planning → **Quotation sent** by itself (history actor `System`) |
| 7 | Web: **Agent runs** → that run | Four steps Succeeded. A step warning "groq rate limited (429); retry 1 after N s" is acceptable (the per-minute token limit); a Failed step is not |
| 8 | Tourist accepts the quotation | Trip **Accepted**; the dashboard tile **Accepted — confirm** counts it |
| 9 | Manager opens the trip → **Confirm** | "Trip confirmed: N holds created"; trip **Confirmed**; vouchers on the phone |
| 10 | Security: tourist token on `POST /api/trip-requests/{id}/confirm` | `403` |

If step 6 ends **Needs operator**, open the run: "GROQ_API_KEY is not set" means the key is missing on
`tripcraft-agents`; "LLM call failed: NotFoundError" means the key cannot use `GROQ_MODEL` (see step 1);
"quota used up … retry in …" or "rate limited … after 3 retries" means the free-tier limit is used up for now
(wait, then **Retry planning**); a 401 between the services means `INTERNAL_AGENT_KEY` differs.

**Local (Ollama).** For a laptop demo, run the agents with `LLM_PROVIDER=ollama` as in `agents/README.md` and point
the API's `AGENT_SERVICE_URL` at them; the smoke tests are the same.
