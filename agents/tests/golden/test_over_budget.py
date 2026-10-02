from app.graph import run_workflow
from app.state import PENDING_APPROVAL, REVISION_REQUESTED
from tests.conftest import demo_request, load_fixture


async def test_over_budget_replans_once_at_lowest_cost(fake_llm, api):
    fake_llm.queue("resources", load_fixture("resources"), load_fixture("resources_budget"))

    final = await run_workflow(demo_request(budget_usd=400))

    assert final["replans"] == 1
    assert final["status"] == PENDING_APPROVAL
    # Lowest cost on the re-plan (v1.1): budget rooms, cheapest vehicle and free stops instead of the two paid
    # Kandy entries (Temple of the Tooth LKR 2,000, Peradeniya LKR 3,000), all enforced in code.
    assert final["plan"]["constraints"]["cost_strategy"] == "lowest"
    assert all(stop["entry_fee_lkr"] == 0 for day in final["days"] for stop in day["stops"])
    assert final["quotation"]["total_usd"] == 302.07
    assert len(api.bodies(api.steps)) == 8  # two full passes of four agents
    # The second planner call received the over-budget violation as revision context.
    second_planner_data = fake_llm.calls_for("planner")[1][1].content
    assert "OVER_BUDGET" in second_planner_data
    proposal = api.bodies(api.proposal)[0]
    assert proposal["replans"] == 1 and proposal["status"] == PENDING_APPROVAL
    assert proposal["best_available_price"] is False  # within budget after the re-plan


async def test_replans_stop_at_max_replans(fake_llm, api):
    fake_llm.queue("resources", load_fixture("resources"), load_fixture("resources_budget"))

    final = await run_workflow(demo_request(budget_usd=100))  # even budget rooms cost USD 378.73

    assert final["replans"] == 3
    assert final["status"] == REVISION_REQUESTED
    assert [v["code"] for v in final["violations"]] == ["OVER_BUDGET"]
    # Still over budget after the lowest-cost re-plans: the proposal goes to the API flagged as the best price
    # available, and the API sends it to the client with that note (a Hard rule would never be sent).
    proposals = api.bodies(api.proposal)
    assert len(proposals) == 1 and proposals[0]["best_available_price"] is True
