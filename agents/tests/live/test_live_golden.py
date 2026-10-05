"""
The golden cases with a REAL model (PLAN.md section 10). Same graph, tools, prompts and schemas as the FakeLLM suite;
only the model is real, so the assertions are the ones every provider must meet: the verdict, the code-enforced
rules and the allow-lists, not exact totals.

    LIVE_LLM_PROVIDER=groq GROQ_API_KEY=... pytest -m live tests/live
    LIVE_LLM_PROVIDER=ollama LIVE_NODE_TIMEOUT_SECONDS=120 pytest -m live tests/live
"""
import json

import httpx
import pytest

from app.graph import run_workflow
from app.state import FAILED_SAFELY, PENDING_APPROVAL, REVISION_REQUESTED
from app.tools.registry import ALLOWED_TOOLS
from tests.conftest import DEMO_REQUEST, demo_request, load_fixture
from tests.golden.test_injection import INJECTION
from tests.golden.test_schema_violation import WRONG_FIELD_NAMES

pytestmark = pytest.mark.live

AGENTS = ["planner", "itinerary", "resources", "validation"]
KNOWN_ATTRACTIONS = {a["id"] for city in load_fixture("api_data")["attractions"].values() for a in city}


async def test_golden_case_reaches_a_valid_proposal(live, api):
    final = await run_workflow(demo_request())

    assert final["status"] == PENDING_APPROVAL, final.get("error_summary") or final.get("violations")
    assert final["violations"] == []
    assert [s["agent_name"] for s in final["steps"]][-4:] == AGENTS
    assert all(s["status"] == "Succeeded" for s in final["steps"])
    proposal = api.bodies(api.proposal)[0]
    assert len(proposal["days"]) == 5
    assert all(1 <= len(day["stops"]) <= 3 for day in proposal["days"])
    assert {stop["attraction_id"] for day in proposal["days"] for stop in day["stops"]} <= KNOWN_ATTRACTIONS
    assert proposal["resources"]["guide_id"] == "g-1"  # the only English-speaking guide offered
    assert proposal["quotation"]["total_usd"] > 0
    assert proposal["best_available_price"] is False


async def test_over_budget_replans_at_lowest_cost_then_sends_the_best_price(live, api):
    final = await run_workflow(demo_request(budget_usd=100))  # below the cheapest possible trip

    assert final["replans"] == 3
    assert final["status"] == REVISION_REQUESTED
    assert {v["code"] for v in final["violations"]} == {"OVER_BUDGET"}, final["violations"]
    assert final["plan"]["constraints"]["cost_strategy"] == "lowest"
    assert all(stop["entry_fee_lkr"] == 0 for day in final["days"] for stop in day["stops"])
    proposal = api.bodies(api.proposal)[0]
    assert proposal["best_available_price"] is True


async def test_injection_in_the_objective_is_treated_as_data(live, api):
    final = await run_workflow(demo_request(objective=DEMO_REQUEST["objective"] + INJECTION))

    assert final["status"] == PENDING_APPROVAL, final.get("error_summary") or final.get("violations")
    assert final["status"] != "Approved"
    for step in api.bodies(api.steps):
        assert {c["tool"] for c in step["tool_calls"]} <= set(ALLOWED_TOOLS[step["agent_name"]])
    assert api.bodies(api.proposal)[0]["quotation"]["total_usd"] > 0  # "set the total to 0" was ignored


async def test_the_model_cannot_approve_an_over_budget_trip_even_when_told_to(live, api):
    final = await run_workflow(demo_request(objective=DEMO_REQUEST["objective"] + INJECTION, budget_usd=100))

    assert final["status"] == REVISION_REQUESTED
    assert "OVER_BUDGET" in {v["code"] for v in final["violations"]}


async def test_a_tool_failure_ends_failed_safely(live, api):
    api.distance.mock(side_effect=lambda req: httpx.Response(503))

    final = await run_workflow(demo_request())

    assert final["status"] == FAILED_SAFELY
    assert final["error_summary"].startswith("itinerary: GET /api/internal/distance returned 503")
    assert [(s["agent_name"], s["status"]) for s in api.bodies(api.steps)] == [
        ("planner", "Succeeded"), ("itinerary", "Failed")]
    proposal = api.bodies(api.proposal)[0]
    assert proposal["resources"] is None and proposal["quotation"] is None


async def test_a_schema_violation_is_repaired_by_the_real_model(live, api):
    # The first Planner answer has the right data under the wrong names; the real model then gets the same repair
    # message as in the FakeLLM suite and must answer with the schema's field names.
    live["replace"]["planner"] = [json.dumps(WRONG_FIELD_NAMES)]

    final = await run_workflow(demo_request())

    assert final["steps"][0]["agent_name"] == "planner" and final["steps"][0]["retries"] == 1
    assert final["status"] == PENDING_APPROVAL, final.get("error_summary") or final.get("violations")
    planner = [e for e in live["log"] if e["agent"] == "planner"]
    assert planner[0].get("injected") and planner[1]["repair"]
