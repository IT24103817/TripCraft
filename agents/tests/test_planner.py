from app.graph import initial_state
from app.nodes.planner import planner_node
from app.schemas import ReplanRequest
from app.state import FAILED_SAFELY, RUNNING
from tests.conftest import DEMO_REQUEST, load_fixture


async def test_planner_golden_plan_and_constraints(fake_llm, api, demo_state):
    update = await planner_node(demo_state)

    assert update["status"] == RUNNING
    plan = update["plan"]
    assert [s["agent"] for s in plan["plan"]][-1] == "validation"
    assert len(plan["plan"]) == 6
    assert plan["constraints"]["cities"] == ["Kandy", "Ella"]
    assert plan["constraints"]["guide_language"] == "en"
    assert plan["constraints"]["hotel_tier"] == "standard"
    assert plan["dates"] == ["2026-10-10", "2026-10-11", "2026-10-12", "2026-10-13", "2026-10-14"]

    report = update["steps"][0]
    assert report["agent_name"] == "planner" and report["status"] == "Succeeded" and report["retries"] == 0
    assert [c["tool"] for c in report["tool_calls"]] == ["parse_dates", "list_agents"]

    user_message = fake_llm.calls_for("planner")[0][1].content
    assert user_message.startswith("<DATA>") and user_message.endswith("</DATA>")


async def test_planner_budget_replan_forces_budget_hotel_tier(fake_llm, api, demo_state):
    demo_state["violations"] = [{"code": "OVER_BUDGET", "message": "Total USD 624.07 is over the budget."}]

    update = await planner_node(demo_state)

    assert update["plan"]["constraints"]["hotel_tier"] == "budget"  # fixture says standard; code overrides
    assert '"revision_context": {"violations"' in fake_llm.calls_for("planner")[0][1].content


async def test_planner_invalid_plan_fails_safely_after_retries(fake_llm, api, demo_state):
    bad = load_fixture("planner")
    bad["plan"] = [s for s in bad["plan"] if s["agent"] != "validation"]
    fake_llm.queue("planner", bad)

    update = await planner_node(demo_state)

    assert update["status"] == FAILED_SAFELY
    assert update["error_summary"].startswith("planner:")
    assert "validation" in update["error_summary"]
    assert update["steps"][0]["status"] == "Failed" and update["steps"][0]["retries"] == 2
    assert len(fake_llm.calls_for("planner")) == 3  # first try + 2 repair messages


async def test_a_manager_revision_replans_with_the_previous_violations(fake_llm, api):
    # The API sends the rejected proposal's violations with /replan (camelCase, as the C# client does).
    request = ReplanRequest.model_validate({
        **DEMO_REQUEST, "managerComment": "Please find cheaper hotels",
        "previousViolations": [{"code": "OVER_BUDGET", "message": "Total USD 624.07 is over the budget of USD 400."}]})
    state = initial_state(request)

    update = await planner_node(state)

    assert state["violations"] == [{"code": "OVER_BUDGET",
                                    "message": "Total USD 624.07 is over the budget of USD 400."}]
    assert update["plan"]["constraints"]["hotel_tier"] == "budget"
    prompt = fake_llm.calls_for("planner")[0][1].content
    assert "Please find cheaper hotels" in prompt and "OVER_BUDGET" in prompt
