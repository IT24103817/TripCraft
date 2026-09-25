from app.graph import run_workflow
from app.state import PENDING_APPROVAL
from tests.conftest import demo_request


async def test_demo_request_reaches_pending_approval(fake_llm, api):
    final = await run_workflow(demo_request())

    assert final["status"] == PENDING_APPROVAL
    assert final["violations"] == [] and final["replans"] == 0
    assert [s["agent_name"] for s in final["steps"]] == ["planner", "itinerary", "resources", "validation"]

    steps = api.bodies(api.steps)
    assert len(steps) == 4
    assert all(s["status"] == "Succeeded" for s in steps)
    assert all(call.request.headers["X-Internal-Key"] == "test-internal-key" for call in api.steps.calls)

    proposals = api.bodies(api.proposal)
    assert len(proposals) == 1
    proposal = proposals[0]
    assert proposal["status"] == PENDING_APPROVAL
    assert proposal["quotation"]["total_usd"] == 624.07
    assert len(proposal["days"]) == 5 and proposal["resources"]["guide_id"] == "g-1"
    assert proposal["error_summary"] is None
    assert str(api.proposal.calls[0].request.url).endswith(
        "/api/internal/workflows/3f1c9a52-7d4e-4b8a-9c11-2a5e6f7b8c90/proposal")
