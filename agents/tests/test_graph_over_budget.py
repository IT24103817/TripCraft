from app.graph import run_workflow
from app.state import PENDING_APPROVAL, REVISION_REQUESTED
from tests.conftest import demo_request, load_fixture


async def test_over_budget_replans_once_with_budget_rooms(fake_llm, api):
    fake_llm.queue("resources", load_fixture("resources"), load_fixture("resources_budget"))

    final = await run_workflow(demo_request(budget_usd=400))

    assert final["replans"] == 1
    assert final["status"] == PENDING_APPROVAL
    assert final["quotation"]["total_usd"] == 378.73
    assert len(api.bodies(api.steps)) == 8  # two full passes of four agents
    # The second planner call received the over-budget violation as revision context.
    second_planner_data = fake_llm.calls_for("planner")[1][1].content
    assert "OVER_BUDGET" in second_planner_data
    proposal = api.bodies(api.proposal)[0]
    assert proposal["replans"] == 1 and proposal["status"] == PENDING_APPROVAL


async def test_replans_stop_at_max_replans(fake_llm, api):
    fake_llm.queue("resources", load_fixture("resources"), load_fixture("resources_budget"))

    final = await run_workflow(demo_request(budget_usd=100))  # even budget rooms cost USD 378.73

    assert final["replans"] == 3
    assert final["status"] == REVISION_REQUESTED
    assert [v["code"] for v in final["violations"]] == ["OVER_BUDGET"]
    assert len(api.bodies(api.proposal)) == 1
