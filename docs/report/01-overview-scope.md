# 1. Overview and scope

## Problem

Small and mid-size Sri Lankan inbound tour operators plan custom trips by hand: WhatsApp requests, Excel
availability sheets for guides and vehicles, phone calls to hotels and manually typed quotations. This causes
double-booked guides, vehicles with too few seats, rooms that were never confirmed, quotations that miss the
tourist's budget, and slow replies that lose the sale.

## Solution

TripCraft is an integrated tour-operator platform. Tourists submit a trip objective from a Flutter app; four AI
agents (Planner, Itinerary Analysis, Resource & Action, Validation & Safety) draft an itinerary, propose a
guide, vehicle and rooms and calculate a quotation in LKR and USD; deterministic C# rules check the proposal; the
Operations Manager approves it in a React dashboard; only then are resources held, in one database transaction.

## Scope

| In scope (this submission) | Status |
|----------------------------|--------|
| Authentication, 4 roles, user management | built |
| Component A — trip requests, attractions, itinerary skeleton, start-planning, passport photo | built |
| Agent service — 4 agents, tools, guards, evaluation | built |
| Workflow integration — internal API, step/proposal persistence, deterministic validation, approval transaction | built (against interfaces for B and C) |
| Component B — guides, vehicles, hotels, availability, resource holds | TODO: to be merged by Student B |
| Component C — quotations, approval decisions, reports | approval transaction built; quotation store, list and reports TODO by Student C |
| React staff app, Flutter app, CI, deployment configuration | built |

Mandatory stack: ASP.NET Core Web API, EF Core + PostgreSQL, React, Flutter, a Python LangGraph agent service
called only by ASP.NET Core.

TODO: one paragraph on what changed from the original plan and why (e.g. the B/C integration order).
