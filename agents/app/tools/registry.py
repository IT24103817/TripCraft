"""
Tool allow-list (PLAN.md section 5) and run_tool, the only way a node may call a tool.
Every call is recorded so the node can put it in its StepReport.
"""
import inspect
import time
from contextvars import ContextVar
from datetime import date
from typing import Any, Callable

from pydantic import ValidationError

from app.errors import ToolError, ToolNotAllowed
from app.schemas import ToolCall
from app.tools.calculate_quotation import calculate_quotation
from app.tools.check_business_rules import check_business_rules
from app.tools.check_guide_availability import check_guide_availability
from app.tools.check_room_availability import check_room_availability
from app.tools.check_vehicle_availability import check_vehicle_availability
from app.tools.get_attractions import get_attractions
from app.tools.get_distance import get_distance
from app.tools.get_fx_rate import get_fx_rate
from app.tools.get_rate_card import get_rate_card
from app.tools.get_weather import get_weather
from app.tools.list_agents import list_agents
from app.tools.parse_dates import parse_dates
from app.tools.validate_schema import validate_schema

ALLOWED_TOOLS: dict[str, list[str]] = {
    "planner": ["parse_dates", "list_agents"],
    "itinerary": ["get_attractions", "get_distance", "get_weather"],
    "resources": ["check_guide_availability", "check_vehicle_availability", "check_room_availability",
                  "get_rate_card"],
    "validation": ["calculate_quotation", "get_fx_rate", "validate_schema", "check_business_rules"],
}

TOOLS: dict[str, Callable[..., Any]] = {
    "parse_dates": parse_dates,
    "list_agents": list_agents,
    "get_attractions": get_attractions,
    "get_distance": get_distance,
    "get_weather": get_weather,
    "check_guide_availability": check_guide_availability,
    "check_vehicle_availability": check_vehicle_availability,
    "check_room_availability": check_room_availability,
    "get_rate_card": get_rate_card,
    "calculate_quotation": calculate_quotation,
    "get_fx_rate": get_fx_rate,
    "validate_schema": validate_schema,
    "check_business_rules": check_business_rules,
}

# The list of calls made by the node that is running now. Each workflow task gets its own list.
_current_calls: ContextVar[list[ToolCall] | None] = ContextVar("current_tool_calls", default=None)


def start_recording() -> list[ToolCall]:
    """Called at the start of each node. Returns the list that run_tool will append to."""
    calls: list[ToolCall] = []
    _current_calls.set(calls)
    return calls


def _summarise(value: Any) -> Any:
    """Small, safe view of an argument for the step report (no large blobs)."""
    if isinstance(value, (str, int, float, bool)) or value is None:
        return value
    if isinstance(value, date):
        return value.isoformat()
    if isinstance(value, (list, tuple)):
        return f"list[{len(value)}]"
    return type(value).__name__


async def run_tool(agent: str, name: str, **kwargs: Any) -> Any:
    if name not in ALLOWED_TOOLS.get(agent, []):
        raise ToolNotAllowed(f"agent '{agent}' may not call tool '{name}'")

    started = time.perf_counter()
    error: str | None = None
    try:
        result = TOOLS[name](**kwargs)
        if inspect.isawaitable(result):
            result = await result
        return result
    except ValidationError as ex:
        error = f"invalid input for {name}: {ex.error_count()} error(s)"
        raise ToolError(error) from ex
    except ToolError as ex:
        error = str(ex)
        raise
    finally:
        calls = _current_calls.get()
        if calls is not None:
            calls.append(ToolCall(
                tool=name, args={k: _summarise(v) for k, v in kwargs.items()}, ok=error is None,
                duration_ms=int((time.perf_counter() - started) * 1000), error=error))
