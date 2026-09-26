# Appendix

## A. Environment variable names

| Component | Variables |
|-----------|-----------|
| API | `DATABASE_URL`, `JWT_SECRET`, `JWT_ISSUER`, `ALLOWED_ORIGINS`, `INTERNAL_AGENT_KEY`, `AGENT_SERVICE_URL`, `AGENT_CALLBACK_BASE_URL`, `RUN_MIGRATIONS`, `ORS_API_KEY`, `OWM_API_KEY`, `FX_FALLBACK_LKR_PER_USD`, `UPLOADS_DIR` |
| Agent service | `INTERNAL_AGENT_KEY`, `API_BASE_URL`, `LLM_PROVIDER`, `OLLAMA_MODEL`, `OLLAMA_BASE_URL`, `GROQ_API_KEY`, `GROQ_MODEL`, `NODE_TIMEOUT_SECONDS`, `MAX_RETRIES`, `MAX_REPLANS` |
| Web | `VITE_API_URL` |
| Mobile | `API_URL` (`--dart-define`) |

Meanings: `docs/DEPLOYMENT.md` section 6.

## B. Startup order

PostgreSQL → Ollama → agent service → API → web → mobile.

## C. APK install

See `docs/APK-INSTALL.md`: download `app-release.apk` from GitHub Release v1.0, allow installs from the browser,
install (Android 7.0+), wake the API once, sign in with `tourist1@tripcraft.test` / `Passw0rd!`.
