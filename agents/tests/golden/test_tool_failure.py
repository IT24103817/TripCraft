import asyncio

import httpx

from app.config import get_settings
from app.graph import run_workflow
from app.state import FAILED_SAFELY
from tests.conftest import demo_request


async def test_get_distance_failure_ends_failed_safely(fake_llm, api):
    api.distance.mock(side_effect=lambda req: httpx.Response(503))

    final = await run_workflow(demo_request())  # must not raise

    assert final["status"] == FAILED_SAFELY
    assert final["error_summary"].startswith("itinerary: GET /api/internal/distance returned 503")
    steps = api.bodies(api.steps)
    assert [(s["agent_name"], s["status"]) for s in steps] == [("planner", "Succeeded"), ("itinerary", "Failed")]
    failed_call = steps[1]["tool_calls"][-1]
    assert failed_call["tool"] == "get_distance" and failed_call["ok"] is False

    proposal = api.bodies(api.proposal)[0]
    assert proposal["status"] == FAILED_SAFELY
    assert proposal["error_summary"] == final["error_summary"]
    assert proposal["resources"] is None and proposal["quotation"] is None  # nothing proposed, nothing held


async def test_slow_node_times_out_and_fails_safely(fake_llm, api, monkeypatch):
    monkeypatch.setenv("NODE_TIMEOUT_SECONDS", "0.05")
    get_settings.cache_clear()

    async def slow_reply():
        await asyncio.sleep(1)
        return {}

    fake_llm.queue("planner", slow_reply)

    final = await run_workflow(demo_request())

    assert final["status"] == FAILED_SAFELY
    assert final["error_summary"] == "planner: timed out after 0.05 s"
    assert len(api.bodies(api.proposal)) == 1
