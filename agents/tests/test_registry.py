from datetime import date

import pytest

from app.errors import ToolError, ToolNotAllowed
from app.tools.registry import ALLOWED_TOOLS, run_tool, start_recording


async def test_disallowed_tool_raises_tool_not_allowed():
    with pytest.raises(ToolNotAllowed):
        await run_tool("resources", "get_distance", from_city="Kandy", to_city="Ella")


async def test_unknown_agent_and_unknown_tool_are_rejected():
    with pytest.raises(ToolNotAllowed):
        await run_tool("hacker", "parse_dates", start_date=date(2026, 10, 10), end_date=date(2026, 10, 14))
    with pytest.raises(ToolNotAllowed):
        await run_tool("resources", "create_resource_hold")


def test_allow_list_matches_plan_and_has_no_hold_tool():
    assert ALLOWED_TOOLS["planner"] == ["parse_dates", "list_agents"]
    assert ALLOWED_TOOLS["itinerary"] == ["get_attractions", "get_distance", "get_weather"]
    assert ALLOWED_TOOLS["resources"] == [
        "check_guide_availability", "check_vehicle_availability", "check_room_availability", "get_rate_card"]
    assert ALLOWED_TOOLS["validation"] == [
        "calculate_quotation", "get_fx_rate", "validate_schema", "check_business_rules"]
    assert not any("hold" in tool for tools in ALLOWED_TOOLS.values() for tool in tools)


async def test_allowed_calls_and_bad_input_are_recorded():
    calls = start_recording()

    result = await run_tool("planner", "parse_dates", start_date=date(2026, 10, 10), end_date=date(2026, 10, 14))
    with pytest.raises(ToolError):
        await run_tool("planner", "parse_dates", start_date=date(2026, 10, 14), end_date=date(2026, 10, 10))

    assert result.days == 5 and len(result.nights) == 4
    assert [(c.tool, c.ok) for c in calls] == [("parse_dates", True), ("parse_dates", False)]
    assert calls[0].args == {"start_date": "2026-10-10", "end_date": "2026-10-14"}
