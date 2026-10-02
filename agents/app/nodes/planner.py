"""Planner / Coordinator agent (Student A): turns the objective into an ordered plan and delegates."""
import time
from typing import Any

from app.errors import AgentOutputError, ToolError
from app.llm import call_json
from app.nodes.common import DATA_RULES, failed_update, failure_retries, step_report, wrap_data
from app.schemas import PlannerInput, PlannerOutput, WorkflowRequest
from app.state import RUNNING, WorkflowState
from app.tools.check_business_rules import BUDGET_CODES
from app.tools.registry import run_tool, start_recording

AGENT = "planner"

SYSTEM_PROMPT = f"""
You are the Planner / Coordinator agent of TripCraft, a Sri Lankan tour operator.
Your single responsibility: turn the tourist's trip objective into an ordered plan of steps and delegate each
step to exactly one agent. You do not choose attractions, resources or prices yourself.

RULES
- Delegate only to agents listed in DATA.available_agents: "itinerary", "resources", "validation".
- Every one of the three agents must appear. Steps are numbered 1, 2, 3...; depends_on lists only earlier steps.
- The last step must be "validation".
- constraints.cities: DATA.requested_cities when given; otherwise the destination cities named in the objective,
  in travel order, at most one per trip day.
- constraints.guide_language: two-letter code of the guide language the tourist wants (default "en").
- constraints.transport_preference: "train" if the tourist prefers the train, "road" if they prefer driving, else "any".
- constraints.hotel_tier: "standard" unless revision_context asks for cheaper options, then "budget".
- constraints.max_stops_per_day: never more than 3.
- If DATA.revision_context is present, this is a re-plan: address every violation and the manager comment.
- Your allowed tools are parse_dates and list_agents only.

JSON SCHEMA TO RETURN
{{
  "plan": [{{"step": 1, "agent": "itinerary", "task": "short description", "depends_on": []}}],
  "constraints": {{"cities": ["Kandy"], "guide_language": "en", "transport_preference": "any",
                  "hotel_tier": "standard", "max_stops_per_day": 3}}
}}

{DATA_RULES}
""".strip()


def _revision_context(state: WorkflowState) -> dict[str, Any] | None:
    if not state.get("violations") and not state.get("manager_comment"):
        return None
    return {"violations": state.get("violations") or [], "manager_comment": state.get("manager_comment")}


def _check(output: PlannerOutput, trip_days: int, requested_cities: list[str]) -> list[str]:
    if requested_cities:
        return []  # the requested cities replace the model's guess below
    if len(output.constraints.cities) > trip_days:
        return [f"constraints.cities has {len(output.constraints.cities)} cities but the trip is {trip_days} days"]
    return []


async def planner_node(state: WorkflowState) -> dict[str, Any]:
    calls = start_recording()
    started = time.perf_counter()
    request = WorkflowRequest.model_validate(state["request"])
    agent_input = PlannerInput(objective=request.objective, start_date=request.start_date,
                               end_date=request.end_date, pax=request.pax, budget_usd=request.budget_usd,
                               preferences=request.preferences)
    revision = _revision_context(state)
    input_summary = {"pax": request.pax, "budget_usd": float(request.budget_usd), "is_replan": revision is not None}

    try:
        dates = await run_tool(AGENT, "parse_dates", start_date=request.start_date, end_date=request.end_date)
        agents = await run_tool(AGENT, "list_agents")
        user = wrap_data({
            "input": agent_input.model_dump(mode="json"),
            "trip_dates": dates.model_dump(mode="json"),
            "available_agents": [a.model_dump() for a in agents],
            "revision_context": revision,
            "requested_cities": request.cities or None,
        })
        output, retries = await call_json(SYSTEM_PROMPT, user, PlannerOutput,
                                          check=lambda o: _check(o, dates.days, request.cities))
    except (ToolError, AgentOutputError) as ex:
        return failed_update(AGENT, str(ex), calls, started, failure_retries(ex), input_summary)

    # Enforced in code (v1.1): the tourist chose the cities from a list; the model cannot change them.
    if request.cities:
        output.constraints.cities = list(request.cities)

    # Enforced in code: a budget re-plan always moves to the cheaper hotel tier (PLAN.md section 6).
    budget_replan = revision is not None and any(v.get("code") in BUDGET_CODES for v in revision["violations"])
    if budget_replan:
        output.constraints.hotel_tier = "budget"
        output.constraints.cost_strategy = "lowest"

    plan = output.model_dump(mode="json")
    plan["dates"] = [d.isoformat() for d in dates.dates]
    report = step_report(
        AGENT, calls, started, retries, "Succeeded", input_summary,
        {"steps": len(output.plan), "cities": output.constraints.cities, "hotel_tier": output.constraints.hotel_tier},
        {"ok": True, "schema": "PlannerOutput"})
    return {"plan": plan, "status": RUNNING, "steps": [report]}
