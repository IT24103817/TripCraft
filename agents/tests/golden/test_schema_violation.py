"""Golden case: the LLM returns JSON with the wrong field names (PLAN.md section 11, "schema violation rejected")."""
from app.graph import run_workflow
from app.state import FAILED_SAFELY, PENDING_APPROVAL
from tests.conftest import demo_request, load_fixture

# Right shape of data, wrong names: "steps"/"rules" instead of "plan"/"constraints".
WRONG_FIELD_NAMES = {
    "steps": [{"number": 1, "who": "itinerary"}],
    "rules": {"towns": ["Kandy", "Ella"]},
}


async def test_wrong_field_names_are_repaired_then_fail_safely(fake_llm, api):
    fake_llm.queue("planner", WRONG_FIELD_NAMES)

    final = await run_workflow(demo_request())

    planner_calls = fake_llm.calls_for("planner")
    assert len(planner_calls) == 3  # first answer + 2 repair attempts (MAX_RETRIES)
    repair = planner_calls[1][-1].content
    assert repair.startswith("Your JSON did not pass validation.")
    assert "plan: Field required" in repair and "constraints: Field required" in repair
    assert final["status"] == FAILED_SAFELY
    assert "PlannerOutput invalid after 2 retries" in final["error_summary"]
    # Nothing downstream ran and nothing was proposed.
    assert [s["agent_name"] for s in final["steps"]] == ["planner"]
    assert final["steps"][0]["retries"] == 2
    proposal = api.bodies(api.proposal)[0]
    assert proposal["status"] == FAILED_SAFELY and proposal["days"] == [] and proposal["resources"] is None


async def test_one_repair_message_fixes_the_answer(fake_llm, api):
    fake_llm.queue("planner", WRONG_FIELD_NAMES, load_fixture("planner"))

    final = await run_workflow(demo_request())

    assert final["status"] == PENDING_APPROVAL
    assert final["steps"][0]["retries"] == 1
