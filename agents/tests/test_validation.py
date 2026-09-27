import copy
from decimal import Decimal

from app.graph import initial_state
from app.nodes.itinerary import itinerary_node
from app.nodes.planner import planner_node
from app.nodes.resources import resources_node
from app.nodes.validation import validation_node
from app.schemas import ItineraryDay, ResourceSelection
from app.state import PENDING_APPROVAL, REVISION_REQUESTED
from app.tools.calculate_quotation import calculate_quotation
from app.tools.models import FxRate
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
