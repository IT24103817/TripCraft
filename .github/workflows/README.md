# GitHub Actions workflows

Four workflows, one per component. Each runs on `ubuntu-latest` on every push to `main` and every pull request
to `main`. A newer run on the same branch cancels the older one (`concurrency`).

| Workflow | Trigger | Working directory | Steps |
|----------|---------|-------------------|-------|
| [backend-ci.yml](backend-ci.yml) | push / PR to `main` | `backend` | PostgreSQL 16 service container → .NET 8 → `dotnet restore` → `dotnet build -warnaserror` (Release) → `dotnet test` → upload the `.trx` results |
| [web-ci.yml](web-ci.yml) | push / PR to `main` | `web` | Node 20 → `npm ci` → `npm run lint` → `npm test` → `npm run build` |
| [mobile-ci.yml](mobile-ci.yml) | push / PR to `main` | `mobile` | Flutter stable → `flutter pub get` → `flutter analyze` → `flutter test` |
| [agents-ci.yml](agents-ci.yml) | push / PR to `main` | `agents` | Python 3.11 → `pip install -r requirements.txt` → `ruff check .` → `pytest -q` (with `LLM_PROVIDER=fake`) |

The backend database tests use the service container through `TEST_DATABASE_URL`. The agent tests use a fake
LLM, so CI needs no model.

**Not in CI:** the Playwright end-to-end tests (`tests/e2e`) and the k6 performance scripts (`tests/perf`) are run
locally. They need the full stack (database, API, agent service, web) and a real model for the agent runs. Commands and results:
[docs/TEST-EVIDENCE.md](../../docs/TEST-EVIDENCE.md).
