from app.nodes.itinerary import itinerary_node
from app.nodes.planner import planner_node
from app.nodes.resources import cheapest_for_party, pick_guide, resources_node
from app.state import FAILED_SAFELY
from app.tools.models import GuideOption, RateCard, RoomOption
from tests.conftest import apply, load_fixture


async def itinerary_ready(demo_state):
    state = apply(demo_state, await planner_node(demo_state))
    return apply(state, await itinerary_node(state))


async def test_resources_golden_proposal_without_holds(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)

    update = await resources_node(state)

    res = update["resources"]
    assert res["guide_id"] == "g-1" and res["vehicle_id"] == "v-1"
    assert len(res["rooms"]) == 8 and res["gaps"] == []
    assert res["guide_languages"] == ["en", "si"] and res["vehicle_seats"] == 6
    assert res["rate_card"]["margin_pct"] == 15
    assert update["steps"][0]["output_summary"]["holds_created"] == 0
    # Only read-only GET calls reached the API: nothing was held.
    assert all(call.request.method == "GET" for call in api.router.calls)


async def test_resources_lists_gap_when_no_guide_available(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    api.data["guides"] = []
    no_guide = load_fixture("resources")
    no_guide["guide_id"] = None
    fake_llm.queue("resources", no_guide)

    update = await resources_node(state)

    res = update["resources"]
    assert res["guide_id"] is None
    assert any("No guide speaking 'en'" in gap for gap in res["gaps"])  # added by code, not by the LLM


async def test_resources_invented_guide_id_fails_safely(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    invented = load_fixture("resources")
    invented["guide_id"] = "g-999"
    fake_llm.queue("resources", invented)

    update = await resources_node(state)

    assert update["status"] == FAILED_SAFELY
    assert "guide_id must be one of ['g-1']" in update["error_summary"]


async def test_resources_drops_rooms_booked_on_the_departure_day(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    departure = state["days"][-1]["date"]
    with_departure = load_fixture("resources")
    extra = dict(with_departure["rooms"][0], night=departure)
    with_departure["rooms"] = [*with_departure["rooms"], extra, extra]
    fake_llm.queue("resources", with_departure)

    update = await resources_node(state)

    rooms = update["resources"]["rooms"]
    assert len(rooms) == 8 and all(r["night"] != departure for r in rooms)
    assert update["steps"][0]["output_summary"]["room_nights_dropped"] == 2
    assert update["steps"][0]["retries"] == 0  # cleaned in code, no repair round-trip needed


async def test_resources_dropping_never_hides_a_real_shortfall(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    departure = state["days"][-1]["date"]
    only_departure = load_fixture("resources")
    only_departure["rooms"] = [dict(r, night=departure) for r in only_departure["rooms"]]
    fake_llm.queue("resources", only_departure)

    update = await resources_node(state)

    # Every room was on the departure day: after dropping them the nights have no beds, so the check fails.
    assert update["status"] == FAILED_SAFELY
    assert "need at least 4" in update["error_summary"]


def test_suggested_rooms_are_the_cheapest_plan_that_sleeps_everyone():
    from datetime import date
    from decimal import Decimal

    from app.nodes.resources import check_selection, suggest_rooms
    from app.schemas import ResourceActionOutput
    from app.tools.models import RateCard, RoomOption

    night = date(2026, 10, 10)
    double = RoomOption(hotel_id="h", hotel_name="H", room_type_id="dbl", room_type_name="Double", capacity=2,
                        available_rooms=5)
    family = RoomOption(hotel_id="h", hotel_name="H", room_type_id="fam", room_type_name="Family", capacity=4,
                        available_rooms=1)
    card = RateCard(margin_pct=Decimal(15), guide_day_rates={}, vehicle_km_rates={},
                    room_night_rates={"dbl": Decimal(12000), "fam": Decimal(20000)})

    plan = suggest_rooms({night: [double, family]}, 4, card)

    assert [r.room_type_id for r in plan] == ["fam"]  # one family room (20000) beats two doubles (24000)
    output = ResourceActionOutput(guide_id=None, vehicle_id=None, rooms=plan, gaps=[])
    assert check_selection(output, [], [], {night: [double, family]}, 4) == []

    tight = RoomOption(**{**double.model_dump(), "available_rooms": 1})
    mixed = suggest_rooms({night: [tight, family]}, 6, card)
    assert sorted(r.room_type_id for r in mixed) == ["dbl", "fam"]  # no single type fits 6: combine


async def test_model_gaps_that_contradict_the_selection_are_dropped(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    contradictory = load_fixture("resources")
    contradictory["gaps"] = ["No guide available for language 'en' on day 2",
                             "No vehicle available with 4 seats on day 2",
                             "Tourist prefers a sea view"]
    fake_llm.queue("resources", contradictory)

    update = await resources_node(state)

    assert update["resources"]["guide_id"] == "g-1"
    assert update["resources"]["gaps"] == ["Tourist prefers a sea view"]


def _guide(gid: str, name: str, *languages: str) -> GuideOption:
    return GuideOption(id=gid, name=name, languages=list(languages), max_pax=10)


def _card(**rates: int) -> RateCard:
    return RateCard(margin_pct=15, guide_day_rates=rates, vehicle_km_rates={}, room_night_rates={})


def test_pick_guide_is_the_cheapest_candidate_with_the_language():
    candidates = [_guide("g-3", "Ruwan Fernando", "en", "fr"), _guide("g-1", "Nimal Perera", "en", "si"),
                  _guide("g-5", "Chen Wei", "zh")]
    card = _card(**{"g-3": 7000, "g-1": 6000, "g-5": 4000})  # the cheapest guide does not speak English

    guide, reason = pick_guide(candidates, card, "en")

    assert guide is not None and guide.id == "g-1"
    assert reason == "Nimal Perera: cheapest of 2 available 'en' guide(s), LKR 6,000/day"


def test_pick_guide_breaks_ties_by_name_and_handles_no_speaker():
    tied = [_guide("g-2", "Kumari Silva", "de"), _guide("g-9", "Anura Bandara", "de")]

    guide, _ = pick_guide(tied, _card(**{"g-2": 6500, "g-9": 6500}), "de")
    nobody, reason = pick_guide(tied, _card(), "ja")

    assert guide is not None and guide.name == "Anura Bandara"  # same price: alphabetical, so always the same
    assert nobody is None and reason == "no available guide speaks 'ja'"


async def test_code_overrides_a_dearer_guide_proposed_by_the_model(fake_llm, api, demo_state):
    state = await itinerary_ready(demo_state)
    api.data["guides"] = [{"id": "g-1", "name": "Nimal Perera", "languages": ["en", "si"], "maxPax": 10},
                          {"id": "g-3", "name": "Ruwan Fernando", "languages": ["en", "fr"], "maxPax": 12}]
    api.data["rate_card"]["guideDayRates"] = {"g-1": 6000, "g-3": 7000}
    dearer = load_fixture("resources")
    dearer["guide_id"] = "g-3"  # a valid but more expensive choice
    fake_llm.queue("resources", dearer)

    update = await resources_node(state)

    assert update["resources"]["guide_id"] == "g-1"
    assert update["resources"]["guide_languages"] == ["en", "si"]
    summary = update["steps"][0]["output_summary"]
    assert summary["model_guide_id"] == "g-3" and summary["guide_overridden"] is True
    assert summary["guide_choice"].startswith("Nimal Perera: cheapest of 2 available 'en' guide(s)")


def _room(rtid: str, name: str, capacity: int, free: int = 5) -> RoomOption:
    return RoomOption(hotel_id="h-1", hotel_name="Kandy Hills", room_type_id=rtid, room_type_name=name,
                      capacity=capacity, available_rooms=free)


def test_budget_tier_keeps_the_room_type_cheapest_for_the_whole_party():
    double, family = _room("rt-dbl", "Standard Double", 2), _room("rt-fam", "Family Room", 4)
    card = RateCard(margin_pct=15, guide_day_rates={}, vehicle_km_rates={},
                    room_night_rates={"rt-dbl": 12000, "rt-fam": 20000})

    # 4 people: one family room (20,000) beats two doubles (24,000) even though a double is cheaper per room.
    assert [o.room_type_id for o in cheapest_for_party([double, family], card, 4)] == ["rt-fam"]
    # 2 people: one double (12,000) beats one family room (20,000).
    assert [o.room_type_id for o in cheapest_for_party([double, family], card, 2)] == ["rt-dbl"]


def test_budget_tier_keeps_every_option_when_no_single_type_sleeps_the_party():
    options = [_room("rt-dbl", "Standard Double", 2, free=1), _room("rt-fam", "Family Room", 4, free=1)]
    card = RateCard(margin_pct=15, guide_day_rates={}, vehicle_km_rates={},
                    room_night_rates={"rt-dbl": 12000, "rt-fam": 20000})

    assert cheapest_for_party(options, card, 6) == options
