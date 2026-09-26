# Individual report — Student B

**Name:** TODO  **Student ID:** TODO  **GitHub username:** TODO

## 1. Contribution statement

TODO: one paragraph in your own words — what you built, what you reviewed, what you integrated. State clearly
which files below you wrote yourself, which you reviewed, and which were written with AI assistance
(cross-reference your AI log). The Git history is the evidence.

## 2. Owned component and technical work

| | |
|---|---|
| Component | Resource Management (guides, vehicles, hotels) |
| Agent | Resource & Action |
| Third-party integration | OpenRouteService |
| Business operation | Availability check and transactional resource hold — no guide or vehicle double-booking, no negative room count (implements `IResourceCatalog` and `IResourceHoldService`). |
| ADRs to defend | ADR-005 (deployment platform), ADR-006 (LLM provider) |
| Flutter screens | guide schedule, GPS check-in, hotel/vehicle lookup for guides |
| React screens | guide/vehicle/hotel CRUD, availability calendar |

### Files in this component (generated from the repository)

> **Status at the time of writing:** the Resource Management entities, migrations, controllers, availability
> service and hold service are **not in the repository yet**. The workflow already calls them through the ports
> above; the placeholders in `backend/src/TripCraft.Infrastructure/Workflows/PendingComponents.cs` answer 503 until
> you register your implementations in `WorkflowsSetup.cs`. Add your new folders (e.g. `TripCraft.Application/Resources/`,
> `Tests/Resources/`) to this list as you build them.

- `agents/app/nodes/resources.py`
- `agents/tests/test_resources.py`
- `backend/src/TripCraft.Application/Workflows/Ports/IResourceCatalog.cs`
- `backend/src/TripCraft.Application/Workflows/Ports/IResourceHoldService.cs`
- `backend/src/TripCraft.Infrastructure/External/DistanceService.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/DistanceServiceTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/Fakes/FakeResources.cs`
- `docs/adr/ADR-005-cloud-deployment-platform.md`
- `docs/adr/ADR-006-llm-provider.md`
- `mobile/lib/features/resources/data/check_in.dart`
- `mobile/lib/features/resources/presentation/check_in_panel.dart`
- `mobile/lib/features/resources/presentation/qr_scan_screen.dart`
- `mobile/lib/features/resources/presentation/schedule_screen.dart`
- `mobile/lib/features/resources/presentation/trip_day_screen.dart`
- `mobile/test/resources/check_in_test.dart`
- `web/src/features/resources/AvailabilityPage.tsx`
- `web/src/features/resources/GuidesPage.tsx`
- `web/src/features/resources/HotelsPage.tsx`
- `web/src/features/resources/VehiclesPage.tsx`
- `web/src/features/resources/__tests__/placeholders.test.tsx`

TODO: for three of these files, explain the design in 3–5 sentences each (controller → service → repository, one
migration/constraint, your agent's contract and tools).

## 3. Key commits, pull requests and test evidence

| Evidence | Link / screenshot |
|----------|-------------------|
| Commits (e.g. `git log --author=<you> --oneline`) | TODO |
| Pull requests authored | TODO |
| Pull requests reviewed | TODO |
| Test runs for your component (backend, React, Flutter, agent) | TODO screenshots in `docs/evidence/screenshots/` |
| CI runs | TODO links |

## 4. Challenges and learning

TODO: at least two technical challenges, how you diagnosed them, what you changed, and what you learned.

## 5. AI usage log

`docs/ai-log-<your-name>.md` (from `docs/ai-log-template.md`): date, tool and model, task, what it produced, what
was changed or rejected, how it was verified. TODO: create it and keep it consistent with your commits.

## 6. Reflection (one page, hand-written in your own words)

TODO — to be written by you, not generated.

## 7. Declaration

I declare that this individual report describes my own contribution, that my AI usage is fully recorded in my
AI log, and that I can explain and modify the code of my component.

Signature: ____________________  Date: __________
