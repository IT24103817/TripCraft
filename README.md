# TripCraft

<!-- Replace OWNER/REPO with the GitHub repository (e.g. your-org/SE3090_G<nn>) once it exists. -->
[![backend-ci](https://github.com/OWNER/REPO/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/backend-ci.yml)
[![web-ci](https://github.com/OWNER/REPO/actions/workflows/web-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/web-ci.yml)
[![mobile-ci](https://github.com/OWNER/REPO/actions/workflows/mobile-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/mobile-ci.yml)
[![agents-ci](https://github.com/OWNER/REPO/actions/workflows/agents-ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/agents-ci.yml)

Integrated tour-operator platform for Sri Lankan inbound tour operators. Tourists submit a trip objective from a Flutter app, four AI agents draft an itinerary, allocate guides, vehicles and hotel rooms and calculate a quotation, and the Operations Manager approves it in a React dashboard before any booking is held.

SE3090 Assignment 1 — repository `SE3090_G<nn>`.

## Repository layout

| Path | Contents |
|------|----------|
| `backend/` | ASP.NET Core Web API solution + tests |
| `agents/` | Python LangGraph agent service (internal only) |
| `web/` | React staff dashboard |
| `mobile/` | Flutter app (Tourist, Guide) |
| `docs/` | ADRs, diagrams, report sources, AI logs |
| `.github/workflows/` | CI pipelines |

## Component ownership

| | Student A (group leader) | Student B | Student C |
|---|---|---|---|
| **Component** | Trip Requests & Itinerary Management | Resource Management (guides, vehicles, hotels) | Quotation, Approval & Reporting |
| **Entities** | Tourist, TripRequest, Itinerary, ItineraryDay, ItineraryStop, Attraction | Guide, GuideLanguage, Vehicle, Hotel, RoomType, ResourceHold, RateCard | Quotation, QuotationLine, ApprovalDecision, AgentWorkflow, AgentStep, AuditLog |
| **Business op beyond CRUD** | Build a day-by-day itinerary skeleton from the objective (dates, cities, pace) and validate passport/dates | Availability check + transactional resource hold: no guide or vehicle double-booking, no negative room count | Full quotation calculation (vehicle km rate × distance + guide day rate × days + rooms × nights + margin) with LKR→USD conversion, and approve/reject/revise |
| **Agent owned** | Planner / Coordinator Agent | Resource & Action Agent | Validation & Safety Agent |
| **Shared 4th agent** | Itinerary Analysis Agent — Student A writes it, Student B reviews the PR | | |
| **Third-party** | Weather forecast per itinerary day (OpenWeatherMap) | Distance between cities (OpenRouteService) | Exchange rate (open.er-api.com) |
| **Flutter screens** | Register/login, trip request form, itinerary view, status timeline | Guide schedule, GPS check-in, hotel/vehicle lookup for guides | Quotation view, accept quotation, notifications |
| **React screens** | Trip request list, itinerary editor, attraction CRUD | Guide/vehicle/hotel CRUD, availability calendar | Approval inbox, agent workflow monitor, reports dashboard |

## Live URLs

| Service | URL |
|---------|-----|
| API health | _TBD_ |
| Swagger | _TBD_ |
| React web app | _TBD_ |
| Android APK | _TBD_ |

## Environment variables

See [`.env.example`](.env.example) for required variable names. Never commit real values.
