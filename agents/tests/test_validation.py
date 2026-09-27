import copy
from datetime import date
from decimal import Decimal

from app.graph import initial_state
from app.nodes.itinerary import itinerary_node
from app.nodes.planner import planner_node
from app.nodes.resources import resources_node
from app.nodes.validation import validation_node
from app.schemas import ItineraryDay, ResourceSelection, RoomNight, Stop
from app.state import PENDING_APPROVAL, REVISION_REQUESTED
from app.tools.calculate_quotation import calculate_quotation
from app.tools.check_business_rules import check_business_rules
from app.tools.models import FxRate, RateCard
from tests.conftest import DEMO_REQUEST, apply, demo_request


async def proposal_ready(state):
    for node in (planner_node, itinerary_node, resources_node):
        state = apply(state, await node(state))
    return state


async def test_validation_golden_is_valid_with_quotation(fake_llm, api, demo_state):
    state = await proposal_ready(demo_state)
    days_before = copy.deepcopy(state["days"])

    update = await validation_node(state)

    assert update["status"] == PENDING_APPROVAL
    assert update["violations"] == []
    q = update["quotation"]
    assert q["subtotal_lkr"] == 162800 and q["margin_lkr"] == 24420 and q["total_lkr"] == 187220
    assert q["total_usd"] == 624.07 and q["fx_rate"] == 300
    assert "days" not in update and "resources" not in update  # validation never rewrites the proposal
    assert state["days"] == days_before
    assert [c["tool"] for c in update["steps"][0]["tool_calls"]] == [
        "validate_schema", "get_fx_rate", "calculate_quotation", "check_business_rules"]


async def test_validation_flags_rule_breaks_even_if_llm_says_valid(fake_llm, api, demo_state):
    state = await proposal_ready(demo_state)
    state["days"][0]["stops"] = state["days"][0]["stops"] * 2  # 4 stops
    state["resources"]["vehicle_seats"] = 3
    state["resources"]["guide_languages"] = ["de"]

    update = await validation_node(state)  # the fake LLM still answers valid=true

    codes = [v["code"] for v in update["violations"]]
    assert {"DAY_STOPS", "VEHICLE_SEATS", "GUIDE_LANGUAGE"} <= set(codes)
    assert update["status"] == REVISION_REQUESTED
    assert update["steps"][0]["validation_result"]["valid"] is False


async def test_validation_over_budget(fake_llm, api):
    state = dict(initial_state(demo_request(budget_usd=400)))
    state = await proposal_ready(state)

    update = await validation_node(state)

    assert [v["code"] for v in update["violations"]] == ["OVER_BUDGET"]


def test_calculate_quotation_mirrors_formula():
    day = {"day": 1, "date": "2026-10-10", "city": "Kandy", "transport": "road", "transfer_km": 10.5,
           "stops": [{"attraction_id": "a", "name": "A", "entry_fee_lkr": 1000}]}
    days = [ItineraryDay.model_validate(day), ItineraryDay.model_validate({**day, "day": 2, "date": "2026-10-11"})]
    resources = ResourceSelection.model_validate({
        "guide_id": "g", "vehicle_id": "v",
        "rooms": [{"hotel_id": "h", "room_type_id": "r", "night": "2026-10-10"}],
        "rate_card": {"margin_pct": 10, "guide_day_rates": {"g": 5000}, "vehicle_km_rates": {"v": 100},
                      "room_night_rates": {"r": 7000}}})
    fx = FxRate(rate=Decimal("299.5"), as_of="2026-10-01T00:00:00Z")

    q = calculate_quotation(days, resources, pax=DEMO_REQUEST["pax"], fx=fx)

    # guide 2 x 5000 + vehicle 21 km x 100 + room 1 x 7000 + entry 2 days x 4 pax x 1000 = 27100
    assert q.subtotal_lkr == Decimal("27100.00")
    assert q.margin_lkr == Decimal("2710.00") and q.total_lkr == Decimal("29810.00")
    assert q.total_usd == Decimal("99.53")  # 29810 / 299.5 = 99.532..., rounded half up


def test_validation_prompt_asks_for_no_quotation_copy():
    # The node always keeps the calculated quotation, so a copied one is only slow output (timeouts on local models).
    from app.nodes.validation import SYSTEM_PROMPT

    assert "Set quotation_final to null" in SYSTEM_PROMPT
    assert "Copy quotation_draft" not in SYSTEM_PROMPT


def _rule_inputs(driving_minutes: int = 0, rooms_on_first_night: int = 2):
    """Two days (one night) for 4 pax: guide en, 6-seat van, rooms of capacity 2."""
    first, second = date(2026, 10, 10), date(2026, 10, 11)
    days = [ItineraryDay(day=1, date=first, city="Kandy", transport="road", driving_minutes=driving_minutes,
                         stops=[Stop(attraction_id="a-1", name="Temple", entry_fee_lkr=2000)]),
            ItineraryDay(day=2, date=second, city="Kandy", transport="road",
                         stops=[Stop(attraction_id="a-2", name="Lake", entry_fee_lkr=0)])]
    resources = ResourceSelection(
        guide_id="g-1", vehicle_id="v-1", guide_languages=["en"], vehicle_seats=6,
        rooms=[RoomNight(hotel_id="h-1", room_type_id="rt-std", night=first)] * rooms_on_first_night,
        room_capacity={"rt-std": 2},
        rate_card=RateCard(margin_pct=15, guide_day_rates={}, vehicle_km_rates={}, room_night_rates={}))
    return days, resources


def test_business_rules_flag_more_than_four_hours_of_driving():
    days, resources = _rule_inputs(driving_minutes=270)

    violations = check_business_rules(days, resources, 4, "en", Decimal("500"), Decimal("1500"))

    assert [v.code for v in violations] == ["DRIVING_LIMIT"]
    assert violations[0].message == "Day 1 needs 270 min driving (max 240)."
    assert check_business_rules(*_rule_inputs(driving_minutes=240), 4, "en", Decimal("500"), Decimal("1500")) == []


def test_business_rules_flag_a_night_that_does_not_sleep_everyone():
    days, resources = _rule_inputs(rooms_on_first_night=1)  # 1 room x 2 beds for 4 people

    violations = check_business_rules(days, resources, 4, "en", Decimal("500"), Decimal("1500"))

    assert [v.code for v in violations] == ["ROOM_CAPACITY"]
    assert violations[0].message == "Night 2026-10-10 sleeps 2 but pax is 4."


def test_a_one_city_trip_has_no_zero_km_vehicle_line():
    days, resources = _rule_inputs()  # both days in Kandy: no transfer km
    fx = FxRate(rate=Decimal("300"), as_of="2026-10-01T00:00:00Z", stale=False)
    resources.rate_card.guide_day_rates["g-1"] = Decimal("6000")
    resources.rate_card.vehicle_km_rates["v-1"] = Decimal("120")
    resources.rate_card.room_night_rates["rt-std"] = Decimal("12000")

    quotation = calculate_quotation(days, resources, 4, fx)

    assert [line.line_type for line in quotation.lines] == ["guide", "room", "entry"]
    assert all(line.qty > 0 for line in quotation.lines)
