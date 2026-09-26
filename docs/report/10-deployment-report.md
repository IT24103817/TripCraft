# 10. Deployment report

TODO: 3–5 pages. Step-by-step guide: `docs/DEPLOYMENT.md`; decision: ADR-005.

| Part | Platform | Status |
|------|----------|--------|
| PostgreSQL | Neon | TODO URL / screenshot of tables |
| API | Render (Docker, `render.yaml`) | TODO URL, `/health`, `/swagger` |
| React | Vercel (`web/vercel.json`) | TODO URL |
| Agent service | local Ollama (demo) / Render with Groq (optional) | TODO mode used |
| APK | GitHub Release v1.0 | TODO link, SHA-256 |

## Verified locally

The API image (multi-stage .NET 8, non-root user `app`, port 8080) was run against an empty PostgreSQL 16
database with `RUN_MIGRATIONS=true`: 4 migrations applied, 12 tables, 12 users, 8 attractions and 6 city
distances seeded; `GET /health` → `{"status":"ok","version":"1.0.0","db":"ok"}`; Swagger served in
Production; login 200; logs in JSON. The agent image runs as a non-root user and answers 401 without the key.
The release APK (73.4 MB) was built and ran on an Android 15 emulator.

TODO: the same check against Neon (paste `GET /health`), Render and Vercel screenshots, the CI runs.

## Operations

Waking the free services, rotating secrets and the smoke-test checklist: `docs/DEPLOYMENT.md` sections 7–9.
