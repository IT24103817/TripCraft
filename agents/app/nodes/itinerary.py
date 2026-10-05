"""Itinerary Analysis agent (Student A, reviewed by B): picks stops per day and road vs train."""
import time
from datetime import date
from typing import Any

from app.errors import AgentOutputError, ToolError
from app.llm import call_json
from app.nodes.common import DATA_RULES, failed_update, failure_retries, step_report, wrap_data
from app.schemas import (
    ItineraryDay,
    ItineraryInput,
    ItineraryOutput,
    PlannerConstraints,
    PlanStep,
    Stop,
    WorkflowRequest,
)
from app.state import WorkflowState
from app.tools.check_business_rules import MAX_DRIVING_MINUTES
from app.tools.models import Attraction, Distance
from app.tools.registry import run_tool, start_recording

AGENT = "itinerary"

SYSTEM_PROMPT = f"""
You are the Itinerary Analysis agent of TripCraft, a Sri Lankan tour operator.
Your single responsibility: build the day-by-day itinerary — which city each day, which attractions to visit,
and whether the day's travel is by road or train. You do not choose guides, vehicles, hotels or prices.

RULES
- Return exactly one entry per trip date, numbered day 1, 2, 3... in date order.
- Visit the cities in the order given in DATA.input.cities. A day's city must be one of those cities.
- Every day has between 1 and max_stops_per_day stops (never more than 3).
- stops are attraction ids taken ONLY from DATA.attractions for that day's city.
- Road driving on a day that changes city must be at most 240 minutes (see DATA.distances). If the road
  transfer is longer, use "train" for that day. Prefer "train" when the tourist prefers the train.
- Pace the trip: prefer attractions not used on an earlier day. Every day, including the last one, needs at
  least 1 stop: when a city has no unused attractions left, repeat one of that city's attractions rather than
  leave the day empty.
- Your allowed tools are get_attractions, get_distance and get_weather only.

JSON SCHEMA TO RETURN
{{"days": [{{"day": 1, "city": "Kandy", "stops": ["<attraction id>"], "transport": "road"}}]}}
transport is "road" or "train".

{DATA_RULES}
""".strip()


def _pair(a: str, b: str) -> frozenset[str]:
    return frozenset((a.lower(), b.lower()))


def check_days(output: ItineraryOutput, dates: list[date], cities: list[str], max_stops: int,
               attractions: dict[str, list[Attraction]], distances: dict[frozenset[str], Distance]) -> list[str]:
    """Operator rules enforced in code: one day per date, known cities and stops, <= 3 stops, <= 4 h driving."""
    problems: list[str] = []
    if [d.day for d in output.days] != list(range(1, len(dates) + 1)):
        problems.append(f"days must be numbered 1..{len(dates)}, one per trip date")

    known_ids = {city.lower(): {a.id for a in items} for city, items in attractions.items()}
    previous_city: str | None = None
    for day in output.days:
        city = day.city.lower()
        if city not in known_ids:
            problems.append(f"day {day.day}: city '{day.city}' is not one of {cities}")
            continue
        if not day.stops:
            # The seed has few attractions per city, so the model must be told a repeat beats an empty day.
            problems.append(f"day {day.day}: has 0 stops, allowed 1-{max_stops}; every day, including the last, "
                            f"needs a stop, so repeat one of the {day.city} attractions if none are unused")
        elif len(day.stops) > max_stops:
            problems.append(f"day {day.day}: has {len(day.stops)} stops, allowed 1-{max_stops}")
        unknown = [s for s in day.stops if s not in known_ids[city]]
        if unknown:
            problems.append(f"day {day.day}: unknown attraction ids {unknown} for {day.city}")
        if previous_city is not None and city != previous_city:
            distance = distances.get(_pair(previous_city, city))
            if distance is None:
                problems.append(f"day {day.day}: travel {previous_city} -> {city} does not follow the city order")
            elif day.transport == "road" and distance.duration_minutes > MAX_DRIVING_MINUTES:
                problems.append(f"day {day.day}: road transfer takes {distance.duration_minutes} min "
                                f"(max {MAX_DRIVING_MINUTES}); use train")
        previous_city = city
    return problems


def _build_days(output: ItineraryOutput, dates: list[date], cities: list[str],
                attractions: dict[str, list[Attraction]],
                distances: dict[frozenset[str], Distance]) -> list[ItineraryDay]:
    """Turns the checked LLM draft into state days. Dates, km and driving minutes come from tools, not the LLM."""
    by_id = {a.id: a for items in attractions.values() for a in items}
    city_names = {c.lower(): c for c in cities}
    days: list[ItineraryDay] = []
    previous_city: str | None = None
    # check_days has already proved there is exactly one day per date.
    for draft, day_date in zip(output.days, dates, strict=True):
        transfer_km, driving = 0, 0
        if previous_city is not None and draft.city.lower() != previous_city:
            distance = distances[_pair(previous_city, draft.city)]
            transfer_km = distance.distance_km
            driving = distance.duration_minutes if draft.transport == "road" else 0
        stops = [Stop(attraction_id=s, name=by_id[s].name, entry_fee_lkr=by_id[s].entry_fee_lkr) for s in draft.stops]
        days.append(ItineraryDay(day=draft.day, date=day_date, city=city_names[draft.city.lower()], stops=stops,
                                 transport=draft.transport, transfer_km=transfer_km, driving_minutes=driving))
        previous_city = draft.city.lower()
    return days


def fewer_paid_entries(days: list[ItineraryDay], attractions: dict[str, list[Attraction]]) -> list[ItineraryDay]:
    """
    Lowest-cost strategy, enforced in code: each paid stop is swapped for a free attraction of the same city that
    the day does not visit yet. A paid stop with no free alternative left is dropped, as long as the day keeps at
    least one stop (a day needs 1 to 3 stops); only then does it stay.
    """
    for day in days:
        used = {s.attraction_id for s in day.stops}
        free = [a for a in attractions.get(day.city.lower(), []) if a.entry_fee_lkr == 0]
        for index, stop in enumerate(day.stops):
            if stop.entry_fee_lkr == 0:
                continue
            swap = next((a for a in free if a.id not in used), None)
            if swap is not None:
                day.stops[index] = Stop(attraction_id=swap.id, name=swap.name, entry_fee_lkr=swap.entry_fee_lkr)
                used.add(swap.id)
        while len(day.stops) > 1 and any(s.entry_fee_lkr > 0 for s in day.stops):
            day.stops.remove(next(s for s in day.stops if s.entry_fee_lkr > 0))
    return days


async def itinerary_node(state: WorkflowState) -> dict[str, Any]:
    calls = start_recording()
    started = time.perf_counter()
    request = WorkflowRequest.model_validate(state["request"])
    plan = state["plan"] or {}
    constraints = PlannerConstraints.model_validate(plan["constraints"])
    dates = [date.fromisoformat(d) for d in plan["dates"]]
    agent_input = ItineraryInput(plan=[PlanStep.model_validate(s) for s in plan["plan"]], cities=constraints.cities,
                                 dates=dates, pax=request.pax, preferences=request.preferences)
    input_summary = {"cities": constraints.cities, "days": len(dates)}

    try:
        attractions: dict[str, list[Attraction]] = {}
        for city in constraints.cities:
            attractions[city.lower()] = await run_tool(AGENT, "get_attractions", city=city)

        distances: dict[frozenset[str], Distance] = {}
        for a, b in zip(constraints.cities, constraints.cities[1:], strict=False):  # consecutive pairs
            distances[_pair(a, b)] = await run_tool(AGENT, "get_distance", from_city=a, to_city=b)

        user = wrap_data({
            "input": agent_input.model_dump(mode="json"),
            "max_stops_per_day": constraints.max_stops_per_day,
            "transport_preference": constraints.transport_preference,
            "attractions": {c: [a.model_dump(mode="json") for a in items] for c, items in attractions.items()},
            "distances": [d.model_dump(mode="json") for d in distances.values()],
        })
        output, retries = await call_json(SYSTEM_PROMPT, user, ItineraryOutput, check=lambda o: check_days(
            o, dates, constraints.cities, constraints.max_stops_per_day, attractions, distances))
    except (ToolError, AgentOutputError) as ex:
        return failed_update(AGENT, str(ex), calls, started, failure_retries(ex), input_summary)

    days = _build_days(output, dates, constraints.cities, attractions, distances)
    if constraints.cost_strategy == "lowest":
        days = fewer_paid_entries(days, attractions)

    # Weather is advisory (PLAN.md section 9): a failed forecast is recorded but never stops the workflow.
    warnings: list[str] = []
    for day in days:
        try:
            forecast = await run_tool(AGENT, "get_weather", city=day.city, date=day.date)
            day.weather = forecast.summary
        except ToolError:
            warnings.append(f"no forecast for day {day.day}")

    report = step_report(
        AGENT, calls, started, retries, "Succeeded", input_summary,
        {"days": len(days), "stops": sum(len(d.stops) for d in days), "cost_strategy": constraints.cost_strategy,
         "paid_stops": sum(1 for d in days for s in d.stops if s.entry_fee_lkr > 0),
         "max_driving_minutes": max(d.driving_minutes for d in days), "warnings": warnings},
        {"ok": True, "schema": "ItineraryOutput", "rules": ["1-3 stops/day", "<= 240 min driving/day"]})
    return {"days": [d.model_dump(mode="json") for d in days], "steps": [report]}
