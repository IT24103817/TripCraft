"""Golden case: a tool outside an agent's allow-list is refused and the workflow ends safely."""
import pytest

from app.errors import ToolNotAllowed
from app.graph import run_workflow
from app.state import FAILED_SAFELY
from app.tools.registry import ALLOWED_TOOLS, TOOLS, run_tool
from tests.conftest import demo_request


@pytest.mark.parametrize("agent", sorted(ALLOWED_TOOLS))
async def test_every_other_tool_is_refused_for_each_agent(agent):
    for tool in sorted(set(TOOLS) - set(ALLOWED_TOOLS[agent])):
        with pytest.raises(ToolNotAllowed):
            await run_tool(agent, tool)


async def test_a_node_asking_for_a_disallowed_tool_fails_safely_without_calling_it(fake_llm, api, monkeypatch):
    # Simulate an itinerary node that reaches for a tool it is not allowed to use (get_weather removed).
    monkeypatch.setitem(ALLOWED_TOOLS, "itinerary", ["get_attractions", "get_distance"])

    final = await run_workflow(demo_request())  # must not raise

    assert final["status"] == FAILED_SAFELY
    assert final["error_summary"] == "itinerary: tool not allowed: agent 'itinerary' may not call tool 'get_weather'"
    assert not api.weather.called  # the refused tool never reached the API
    assert api.bodies(api.proposal)[0]["status"] == FAILED_SAFELY
