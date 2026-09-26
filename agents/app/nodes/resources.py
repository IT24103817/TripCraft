"""Resource & Action agent (Student B): proposes a guide, a vehicle and rooms. It never creates holds."""
import math
import time
from collections import Counter
from datetime import date
from decimal import Decimal
from typing import Any

from app.errors import AgentOutputError, ToolError
from app.llm import call_json
from app.nodes.common import DATA_RULES, failed_update, failure_retries, step_report, wrap_data
from app.schemas import (
    ItineraryDay,
    PlannerConstraints,
    ResourceActionOutput,
    ResourceInput,
    ResourceSelection,
    RoomNight,
    WorkflowRequest,
)
from app.state import WorkflowState
from app.tools.models import GuideOption, RateCard, RoomOption, VehicleOption
from app.tools.registry import run_tool, start_recording

AGENT = "resources"

SYSTEM_PROMPT = f"""
You are the Resource & Action agent of TripCraft, a Sri Lankan tour operator.
Your single responsibility: propose ONE available guide, ONE available vehicle and the hotel rooms for every
night of the itinerary. You only propose; you never book or hold anything.

RULES
- guide_id must be the id of a guide in DATA.guides (they already speak the required language).
  null if the list is empty.
- vehicle_id must be the id of a vehicle in DATA.vehicles (they already have enough seats). null if the list is empty.
- rooms: one entry per room per night, only from DATA.room_options for that night. Pick enough rooms so the
  total capacity each night is at least pax. Do not pick more rooms of a type than available_rooms.
- DATA.suggested_rooms is a valid, cheapest room plan computed by code for exactly these nights. Copy it
  unless the preferences clearly need other rooms that are also in DATA.room_options.
- Prefer the cheapest options that meet the rules (rates are in DATA.rate_card, LKR).
- gaps: one short sentence for every resource you could not find. Never invent ids.
- Your allowed tools are check_guide_availability, check_vehicle_availability, check_room_availability and
  get_rate_card only.

JSON SCHEMA TO RETURN
{{"guide_id": "<id or null>", "vehicle_id": "<id or null>",
  "rooms": [{{"hotel_id": "<id>", "room_type_id": "<id>", "night": "YYYY-MM-DD"}}],
  "gaps": ["<text>"]}}

{DATA_RULES}
""".strip()


def _cheapest_only(options: list[RoomOption], card: RateCard) -> list[RoomOption]:
    """Budget tier: keep only the cheapest room type for the night."""
    if not options:
        return options
    cheapest = min(options, key=lambda o: card.room_night_rates.get(o.room_type_id, Decimal("Infinity")))
    return [cheapest]


def missing_resource_gaps(guides: list[GuideOption], vehicles: list[VehicleOption],
                          room_options: dict[date, list[RoomOption]], language: str, pax: int) -> list[str]:
    """Enforced in code: every resource that has no available option is listed as a gap."""
    gaps: list[str] = []
    if not guides:
        gaps.append(f"No guide speaking '{language}' for {pax} pax is available for the trip dates.")
    if not vehicles:
        gaps.append(f"No vehicle with at least {pax} seats is available for the trip dates.")
    for night, options in room_options.items():
        if not options:
            gaps.append(f"No rooms available on {night.isoformat()}.")
    return gaps


def suggest_rooms(room_options: dict[date, list[RoomOption]], pax: int, card: RateCard) -> list[RoomNight]:
    """
    Code's cheapest valid room plan: for each night, the room type that sleeps everyone for the least money
    (rooms = ceil(pax / capacity), within the free rooms). A night with no such type gets the cheapest beds
    per person until everyone sleeps, as far as rooms allow. Given to the model as a starting point.
    """
    def rate(o: RoomOption) -> Decimal:
        return card.room_night_rates.get(o.room_type_id, Decimal("Infinity"))

    plan: list[RoomNight] = []
    for night, options in room_options.items():
        whole = [(math.ceil(pax / o.capacity), o) for o in options if math.ceil(pax / o.capacity) <= o.available_rooms]
        if whole:
            count, best = min(whole, key=lambda c: c[0] * rate(c[1]))
            plan += [RoomNight(hotel_id=best.hotel_id, room_type_id=best.room_type_id, night=night)] * count
            continue
        beds = 0
        for o in sorted(options, key=lambda o: rate(o) / o.capacity):
            taken = 0
            while beds < pax and taken < o.available_rooms:
                plan.append(RoomNight(hotel_id=o.hotel_id, room_type_id=o.room_type_id, night=night))
                beds += o.capacity
                taken += 1
    return plan


def drop_rooms_outside_stay(output: ResourceActionOutput, nights: set[date]) -> tuple[ResourceActionOutput, int]:
    """
    Enforced in code: rooms can only be booked for nights of the stay (every date except the departure day).
    A small model sometimes adds the departure day; those entries are removed (never added), and the result is
    still checked by check_selection. Returns the cleaned output and how many room-nights were dropped.
    """
    kept = [r for r in output.rooms if r.night in nights]
    return output.model_copy(update={"rooms": kept}), len(output.rooms) - len(kept)


def check_selection(output: ResourceActionOutput, guides: list[GuideOption], vehicles: list[VehicleOption],
                    room_options: dict[date, list[RoomOption]], pax: int) -> list[str]:
    """Enforced in code: only offered ids, no over-booking of a room type, enough beds where possible."""
    problems: list[str] = []
    guide_ids = {g.id for g in guides}
    if guide_ids and output.guide_id not in guide_ids:
        problems.append(f"guide_id must be one of {sorted(guide_ids)}")
    if not guide_ids and output.guide_id is not None:
        problems.append("no guide is available, guide_id must be null")

    vehicle_ids = {v.id for v in vehicles}
    if vehicle_ids and output.vehicle_id not in vehicle_ids:
        problems.append(f"vehicle_id must be one of {sorted(vehicle_ids)}")
    if not vehicle_ids and output.vehicle_id is not None:
        problems.append("no vehicle is available, vehicle_id must be null")

    picked = Counter((r.night, r.hotel_id, r.room_type_id) for r in output.rooms)
    for (night, hotel_id, room_type_id), count in picked.items():
        option = next((o for o in room_options.get(night, [])
                       if o.hotel_id == hotel_id and o.room_type_id == room_type_id), None)
        if option is None:
            problems.append(f"room {room_type_id} at {hotel_id} is not offered on {night}")
        elif count > option.available_rooms:
            problems.append(f"only {option.available_rooms} rooms of {room_type_id} are free on {night}")

    for night, options in room_options.items():
        possible = sum(o.capacity * o.available_rooms for o in options)
        chosen = sum(o.capacity * picked[(night, o.hotel_id, o.room_type_id)] for o in options)
        if possible >= pax and chosen < pax:
            problems.append(f"rooms on {night} sleep {chosen}, need at least {pax}")
    return problems


async def resources_node(state: WorkflowState) -> dict[str, Any]:
    calls = start_recording()
    started = time.perf_counter()
    request = WorkflowRequest.model_validate(state["request"])
    constraints = PlannerConstraints.model_validate((state["plan"] or {})["constraints"])
    days = [ItineraryDay.model_validate(d) for d in state["days"]]
    dates = [d.date for d in days]
    language, pax = constraints.guide_language, request.pax
    agent_input = ResourceInput(days=days, pax=pax, language=language, dates=dates)
    input_summary = {"days": len(days), "pax": pax, "language": language, "hotel_tier": constraints.hotel_tier}

    try:
        guides = await run_tool(AGENT, "check_guide_availability",
                                from_date=dates[0], to_date=dates[-1], language=language, pax=pax)
        vehicles = await run_tool(AGENT, "check_vehicle_availability",
                                  from_date=dates[0], to_date=dates[-1], seats=pax)
        rooms_needed = math.ceil(pax / 2)
        room_options: dict[date, list[RoomOption]] = {}
        for day in days[:-1]:  # every date except the last is a night
            room_options[day.date] = await run_tool(AGENT, "check_room_availability",
                                                    hotel_city=day.city, night=day.date, rooms=rooms_needed)
        card: RateCard = await run_tool(AGENT, "get_rate_card")

        if constraints.hotel_tier == "budget":
            room_options = {night: _cheapest_only(options, card) for night, options in room_options.items()}

        user = wrap_data({
            "input": agent_input.model_dump(mode="json"),
            "guides": [g.model_dump(mode="json") for g in guides],
            "vehicles": [v.model_dump(mode="json") for v in vehicles],
            "room_options": {n.isoformat(): [o.model_dump(mode="json") for o in opts]
                             for n, opts in room_options.items()},
            "rate_card": card.model_dump(mode="json"),
            "suggested_rooms": [r.model_dump(mode="json") for r in suggest_rooms(room_options, pax, card)],
        })
        dropped: list[int] = []

        def normalise(o: ResourceActionOutput) -> ResourceActionOutput:
            cleaned, count = drop_rooms_outside_stay(o, set(room_options))
            dropped.append(count)
            return cleaned

        output, retries = await call_json(SYSTEM_PROMPT, user, ResourceActionOutput, check=lambda o: check_selection(
            o, guides, vehicles, room_options, pax), normalise=normalise)
    except (ToolError, AgentOutputError) as ex:
        return failed_update(AGENT, str(ex), calls, started, failure_retries(ex), input_summary)

    gaps = list(dict.fromkeys(output.gaps + missing_resource_gaps(guides, vehicles, room_options, language, pax)))
    guide = next((g for g in guides if g.id == output.guide_id), None)
    vehicle = next((v for v in vehicles if v.id == output.vehicle_id), None)
    selection = ResourceSelection(
        guide_id=output.guide_id, vehicle_id=output.vehicle_id, rooms=output.rooms, gaps=gaps,
        guide_languages=guide.languages if guide else [], vehicle_seats=vehicle.seats if vehicle else None,
        room_capacity={o.room_type_id: o.capacity for opts in room_options.values() for o in opts},
        rate_card=card)

    report = step_report(
        AGENT, calls, started, retries, "Succeeded", input_summary,
        {"guide_id": selection.guide_id, "vehicle_id": selection.vehicle_id,
         "room_nights": len(selection.rooms), "gaps": gaps, "holds_created": 0,
         "room_nights_dropped": dropped[-1] if dropped else 0},
        {"ok": True, "schema": "ResourceActionOutput"})
    return {"resources": selection.model_dump(mode="json"), "steps": [report]}
