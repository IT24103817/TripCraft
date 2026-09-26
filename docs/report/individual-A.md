# Individual report — Student A

**Name:** TODO  **Student ID:** TODO  **GitHub username:** TODO

## 1. Contribution statement

TODO: one paragraph in your own words — what you built, what you reviewed, what you integrated. State clearly
which files below you wrote yourself, which you reviewed, and which were written with AI assistance
(cross-reference your AI log). The Git history is the evidence.

## 2. Owned component and technical work

| | |
|---|---|
| Component | Trip Requests & Itinerary Management (group leader) |
| Agent | Planner / Coordinator and Itinerary Analysis (shared, reviewed by Student B) |
| Third-party integration | OpenWeatherMap |
| Business operation | Build a day-by-day itinerary skeleton from the objective (dates, cities, pace) and validate passport/dates (`TripPlanningRules`, `TripPlanningService`); start the agent workflow. |
| ADRs to defend | ADR-002 (Flutter state), ADR-003 (agentic AI framework) |
| Flutter screens | register/login, trip request form (camera + date range), itinerary view, status timeline |
| React screens | trip request list, trip detail, attraction CRUD |

### Files in this component (generated from the repository)

- `agents/app/nodes/itinerary.py`
- `agents/app/nodes/planner.py`
- `agents/tests/test_itinerary.py`
- `agents/tests/test_planner.py`
- `backend/src/TripCraft.Api/Controllers/AttractionsController.cs`
- `backend/src/TripCraft.Api/Controllers/TripRequestsController.cs`
- `backend/src/TripCraft.Application/Trips/Attraction.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/AttractionDto.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/AttractionListQuery.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/CreateTripRequestRequest.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/ITripDetails.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/ItineraryDto.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/PassportPhotoResponse.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/SaveAttractionRequest.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/StartPlanningResponse.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/TripRequestDto.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/TripRequestListQuery.cs`
- `backend/src/TripCraft.Application/Trips/Dtos/UpdateTripRequestRequest.cs`
- `backend/src/TripCraft.Application/Trips/IAttractionRepository.cs`
- `backend/src/TripCraft.Application/Trips/IPassportPhotoStore.cs`
- `backend/src/TripCraft.Application/Trips/ITripRequestRepository.cs`
- `backend/src/TripCraft.Application/Trips/Itinerary.cs`
- `backend/src/TripCraft.Application/Trips/ItineraryDay.cs`
- `backend/src/TripCraft.Application/Trips/ItinerarySource.cs`
- `backend/src/TripCraft.Application/Trips/ItineraryStop.cs`
- `backend/src/TripCraft.Application/Trips/Planning/SkeletonDay.cs`
- `backend/src/TripCraft.Application/Trips/Planning/TripPlanningRules.cs`
- `backend/src/TripCraft.Application/Trips/Services/AttractionService.cs`
- `backend/src/TripCraft.Application/Trips/Services/IAttractionService.cs`
- `backend/src/TripCraft.Application/Trips/Services/IPassportPhotoService.cs`
- `backend/src/TripCraft.Application/Trips/Services/ITripPlanningService.cs`
- `backend/src/TripCraft.Application/Trips/Services/ITripRequestService.cs`
- `backend/src/TripCraft.Application/Trips/Services/PassportPhotoService.cs`
- `backend/src/TripCraft.Application/Trips/Services/TripPlanningService.cs`
- `backend/src/TripCraft.Application/Trips/Services/TripRequestService.cs`
- `backend/src/TripCraft.Application/Trips/Tourist.cs`
- `backend/src/TripCraft.Application/Trips/TripRequest.cs`
- `backend/src/TripCraft.Application/Trips/TripRequestStatus.cs`
- `backend/src/TripCraft.Application/Trips/Validators/AttractionListQueryValidator.cs`
- `backend/src/TripCraft.Application/Trips/Validators/CreateTripRequestRequestValidator.cs`
- `backend/src/TripCraft.Application/Trips/Validators/PagedQueryRules.cs`
- `backend/src/TripCraft.Application/Trips/Validators/SaveAttractionRequestValidator.cs`
- `backend/src/TripCraft.Application/Trips/Validators/TripDetailsValidator.cs`
- `backend/src/TripCraft.Application/Trips/Validators/TripRequestListQueryValidator.cs`
- `backend/src/TripCraft.Application/Trips/Validators/UpdateTripRequestRequestValidator.cs`
- `backend/src/TripCraft.Infrastructure/External/WeatherService.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/LocalPassportPhotoStore.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Repositories/AttractionRepository.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Repositories/TripRequestRepository.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Seeding/TripsSeeder.cs`
- `backend/tests/TripCraft.Tests/Trips/AttractionsEndpointsTests.cs`
- `backend/tests/TripCraft.Tests/Trips/PassportPhotoEndpointTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripPlanningRulesTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripPlanningSafeFailureTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripPlanningServiceTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripRequestsEndpointsTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripWorkflowLookupTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripsCreateAndListFlowTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripsModelConfigurationTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripsSeederTests.cs`
- `backend/tests/TripCraft.Tests/Trips/TripsValidatorTests.cs`
- `backend/tests/TripCraft.Tests/Workflows/External/WeatherServiceTests.cs`
- `docs/adr/ADR-002-flutter-state-management.md`
- `docs/adr/ADR-003-agentic-ai-framework.md`
- `mobile/lib/features/trips/application/trips_providers.dart`
- `mobile/lib/features/trips/data/trip_models.dart`
- `mobile/lib/features/trips/data/trips_repository.dart`
- `mobile/lib/features/trips/presentation/my_trips_screen.dart`
- `mobile/lib/features/trips/presentation/new_trip_screen.dart`
- `mobile/lib/features/trips/presentation/trip_detail_screen.dart`
- `mobile/lib/features/trips/presentation/trip_form_rules.dart`
- `mobile/lib/features/trips/presentation/trip_map.dart`
- `mobile/test/trips/my_trips_test.dart`
- `mobile/test/trips/navigation_test.dart`
- `mobile/test/trips/new_trip_form_test.dart`
- `mobile/test/trips/trip_detail_test.dart`
- `web/src/features/trips/AttractionFormDialog.tsx`
- `web/src/features/trips/AttractionsPage.tsx`
- `web/src/features/trips/MapPreview.tsx`
- `web/src/features/trips/StatusTimeline.tsx`
- `web/src/features/trips/TripDetailPage.tsx`
- `web/src/features/trips/TripsListPage.tsx`
- `web/src/features/trips/__tests__/AttractionForm.test.tsx`
- `web/src/features/trips/__tests__/TripDetailPage.test.tsx`
- `web/src/features/trips/__tests__/TripsListPage.test.tsx`
- `web/src/features/trips/api.ts`
- `web/src/features/trips/attractionSchema.ts`
- `web/src/features/trips/types.ts`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/AttractionConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/ItineraryConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/ItineraryDayConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/ItineraryStopConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/TouristConfiguration.cs`
- `backend/src/TripCraft.Infrastructure/Persistence/Configurations/TripRequestConfiguration.cs`

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
