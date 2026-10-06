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
  null if the list is empty. DATA.suggested_guide_id is the cheapest of them, computed by code; code makes the
  final guide choice with the same rule, so choose it unless the list is empty.
- vehicle_id must be the id of a vehicle in DATA.vehicles (they already have enough seats). null if the list is empty.
- rooms: one entry per room per night, only from DATA.room_options for that night. Pick enough rooms so the
  total capacity each night is at least pax. Do not pick more rooms of a type than available_rooms.
- DATA.suggested_rooms is a valid, cheapest room plan computed by code for exactly these nights. To use it,
  answer "use_suggested_rooms": true with an empty rooms list; code fills the rooms in. Do not copy it.
  List rooms yourself only when the preferences clearly need other rooms that are also in DATA.room_options.
- Prefer the cheapest options that meet the rules (rates are in DATA.rate_card, LKR).
- gaps: one short sentence for every resource you could not find. Never invent ids.
- Your allowed tools are check_guide_availability, check_vehicle_availability, check_room_availability and
  get_rate_card only.

JSON SCHEMA TO RETURN
{{"guide_id": "<id or null>", "vehicle_id": "<id or null>",
  "use_suggested_rooms": true,
  "rooms": [],
  "gaps": ["<text>"]}}
(or "use_suggested_rooms": false with "rooms": [{{"hotel_id": "<id>", "room_type_id": "<id>", "night": "YYYY-MM-DD"}}])

{DATA_RULES}
""".strip()


def cheapest_for_party(options: list[RoomOption], card: RateCard, pax: int) -> list[RoomOption]:
    """
    Budget tier: keep only the room type that sleeps the whole party for the least money that night
    (rooms needed x rate, within the free rooms). Priced per party, not per room: for 4 people one family room
    at 20,000 beats two doubles at 12,000 each. If no single type can sleep everyone, the options are kept.
    """
    def cost(o: RoomOption) -> Decimal:
        return math.ceil(pax / o.capacity) * card.room_night_rates.get(o.room_type_id, Decimal("Infinity"))

    whole = [o for o in options if math.ceil(pax / o.capacity) <= o.available_rooms]
    return [min(whole, key=cost)] if whole else options


def pick_guide(candidates: list[GuideOption], card: RateCard, language: str) -> tuple[GuideOption | None, str]:
    """
    Enforced in code, the same way as the room plan: the final guide is the cheapest candidate who speaks the
    required language. Candidates are the guides the availability tool returned for the trip dates and pax.
    Ties are broken by name, so the same data always gives the same guide. Returns the guide and the reason.
    """
    speaking = [g for g in candidates if language in g.languages]
    if not speaking:
        return None, f"no available guide speaks '{language}'"

    def rate(g: GuideOption) -> Decimal:
        return card.guide_day_rates.get(g.id, Decimal("Infinity"))

    best = min(speaking, key=lambda g: (rate(g), g.name))
    price = f"LKR {rate(best):,.0f}/day" if best.id in card.guide_day_rates else "no rate on the card"
    return best, f"{best.name}: cheapest of {len(speaking)} available '{language}' guide(s), {price}"


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


def cheapest_vehicle(vehicles: list[VehicleOption], card: RateCard) -> VehicleOption | None:
    """The available vehicle with the lowest km rate (they all have enough seats); ties broken by registration."""
    if not vehicles:
        return None
    return min(vehicles, key=lambda v: (card.vehicle_km_rates.get(v.id, Decimal("Infinity")), v.registration_no))


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


def consistent_gaps(model_gaps: list[str], output: ResourceActionOutput, nights: set[date]) -> list[str]:
    """
    Enforced in code: the model's gaps may not contradict its own selection (e.g. "no guide available" while a
    guide is chosen). Such gaps are dropped; real gaps are added by missing_resource_gaps from the tool results.
    """
    booked_nights = {r.night for r in output.rooms}

    def contradicts(gap: str) -> bool:
        text = gap.lower()
        return (("guide" in text and output.guide_id is not None)
                or ("vehicle" in text and output.vehicle_id is not None)
                or ("room" in text and nights <= booked_nights))

    return [g for g in model_gaps if not contradicts(g)]


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
            room_options = {night: cheapest_for_party(options, card, pax) for night, options in room_options.items()}
        suggested, guide_reason = pick_guide(guides, card, language)
        suggested_rooms = suggest_rooms(room_options, pax, card)

        user = wrap_data({
            "input": agent_input.model_dump(mode="json"),
            "guides": [g.model_dump(mode="json") for g in guides],
            "vehicles": [v.model_dump(mode="json") for v in vehicles],
            "room_options": {n.isoformat(): [o.model_dump(mode="json") for o in opts]
                             for n, opts in room_options.items()},
            "rate_card": card.model_dump(mode="json"),
            "suggested_rooms": [r.model_dump(mode="json") for r in suggested_rooms],
            "suggested_guide_id": suggested.id if suggested else None,
        })
        dropped: list[int] = []
        rooms_from: list[str] = []

        def normalise(o: ResourceActionOutput) -> ResourceActionOutput:
            # The model may pick code's suggested plan by reference instead of copying it.
            rooms_from.append("suggested" if o.use_suggested_rooms else "model")
            if o.use_suggested_rooms:
                o = o.model_copy(update={"rooms": list(suggested_rooms)})
            cleaned, count = drop_rooms_outside_stay(o, set(room_options))
            dropped.append(count)
            return cleaned

        output, retries = await call_json(SYSTEM_PROMPT, user, ResourceActionOutput, check=lambda o: check_selection(
            o, guides, vehicles, room_options, pax), normalise=normalise)
    except (ToolError, AgentOutputError) as ex:
        return failed_update(AGENT, str(ex), calls, started, failure_retries(ex), input_summary)

    # The model proposed a guide; code makes the final, deterministic choice (pick_guide above).
    model_guide_id = output.guide_id
    guide = suggested
    output = output.model_copy(update={"guide_id": guide.id if guide else None})
    # Lowest-cost strategy (a budget re-plan), enforced in code: the cheapest valid room plan and the cheapest
    # eligible vehicle, whatever the model picked.
    if constraints.cost_strategy == "lowest":
        cheapest = cheapest_vehicle(vehicles, card)
        output = output.model_copy(update={"rooms": suggest_rooms(room_options, pax, card),
                                           "vehicle_id": cheapest.id if cheapest else output.vehicle_id})
    gaps = list(dict.fromkeys(consistent_gaps(output.gaps, output, set(room_options))
                              + missing_resource_gaps(guides, vehicles, room_options, language, pax)))
    vehicle = next((v for v in vehicles if v.id == output.vehicle_id), None)
    selection = ResourceSelection(
        guide_id=output.guide_id, vehicle_id=output.vehicle_id, rooms=output.rooms, gaps=gaps,
        guide_languages=guide.languages if guide else [], vehicle_seats=vehicle.seats if vehicle else None,
        room_capacity={o.room_type_id: o.capacity for opts in room_options.values() for o in opts},
        rate_card=card)

    report = step_report(
        AGENT, calls, started, retries, "Succeeded", input_summary,
        {"guide_id": selection.guide_id, "guide_choice": guide_reason,
         "cost_strategy": constraints.cost_strategy, "model_guide_id": model_guide_id,
         "guide_overridden": model_guide_id != selection.guide_id, "vehicle_id": selection.vehicle_id,
         "room_nights": len(selection.rooms), "rooms_from": rooms_from[-1] if rooms_from else "model",
         "gaps": gaps, "holds_created": 0,
         "room_nights_dropped": dropped[-1] if dropped else 0},
        {"ok": True, "schema": "ResourceActionOutput"})
    return {"resources": selection.model_dump(mode="json"), "steps": [report]}
