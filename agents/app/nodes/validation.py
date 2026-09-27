"""Validation & Safety agent (Student C): deterministic checks, then a compliance verdict. Read-only."""
import time
from typing import Any

from app.errors import AgentOutputError, ToolError
from app.llm import call_json
from app.nodes.common import DATA_RULES, failed_update, failure_retries, step_report, wrap_data
from app.schemas import (
    ItineraryDay,
    PlannerConstraints,
    ResourceSelection,
    ValidationInput,
    ValidationSafetyOutput,
    Violation,
    WorkflowRequest,
)
from app.state import PENDING_APPROVAL, REVISION_REQUESTED, WorkflowState
from app.tools.registry import run_tool, start_recording

AGENT = "validation"
CONCERN = "COMPLIANCE_CONCERN"

SYSTEM_PROMPT = f"""
You are the Validation & Safety agent of TripCraft, a Sri Lankan tour operator.
Your single responsibility: give a compliance verdict on the proposed itinerary, resources and quotation.
You never change the itinerary, the resources or any price, and you never approve anything — a human manager does.

RULES
- DATA.deterministic_violations were found by code. They always stand; copy them into violations.
- valid is true only when there are no violations at all.
- Also flag anything unsafe or non-compliant you notice (for example a missing guide or rooms), as extra violations.
- A trip is over budget when quotation_draft.total_usd > budget_usd.
- A day must have 1 to 3 stops; vehicle seats must be at least pax; the guide must speak the requested language.
- Set quotation_final to null: the server always keeps the calculated quotation_draft, so do not copy it.
- Your allowed tools are calculate_quotation, get_fx_rate, validate_schema and check_business_rules only.

JSON SCHEMA TO RETURN
{{"valid": false, "violations": [{{"code": "OVER_BUDGET", "message": "<text>"}}], "quotation_final": null}}

{DATA_RULES}
""".strip()


def merge_verdict(rule_violations: list[Violation], verdict: ValidationSafetyOutput) -> list[Violation]:
    """Code violations always stand. The LLM may add concerns but can never remove a violation or approve."""
    codes = {v.code for v in rule_violations}
    extra = [Violation(code=CONCERN, message=v.message[:300]) for v in verdict.violations if v.code not in codes]
    if not verdict.valid and not extra and not rule_violations:
        extra.append(Violation(code=CONCERN, message="Validation agent marked the proposal invalid."))
    return rule_violations + extra


async def validation_node(state: WorkflowState) -> dict[str, Any]:
    calls = start_recording()
    started = time.perf_counter()
    request = WorkflowRequest.model_validate(state["request"])
    language = PlannerConstraints.model_validate((state["plan"] or {})["constraints"]).guide_language
    input_summary = {"days": len(state["days"]), "budget_usd": float(request.budget_usd)}

    try:
        schema = await run_tool(AGENT, "validate_schema", days=state["days"], resources=state["resources"] or {})
        if not schema.ok:
            raise ToolError("proposal failed schema validation: " + "; ".join(schema.errors[:3]))
        days = [ItineraryDay.model_validate(d) for d in state["days"]]
        resources = ResourceSelection.model_validate(state["resources"])

        fx = await run_tool(AGENT, "get_fx_rate")
        quotation = await run_tool(AGENT, "calculate_quotation", days=days, resources=resources, pax=request.pax, fx=fx)
        rule_violations = await run_tool(AGENT, "check_business_rules", days=days, resources=resources,
                                         pax=request.pax, language=language, total_usd=quotation.total_usd,
                                         budget_usd=request.budget_usd)

        agent_input = ValidationInput(days=days, resources=resources, quotation_draft=quotation,
                                      budget_usd=request.budget_usd)
        user = wrap_data({
            "input": agent_input.model_dump(mode="json"),
            "pax": request.pax,
            "guide_language": language,
            "deterministic_violations": [v.model_dump() for v in rule_violations],
        })
        verdict, retries = await call_json(SYSTEM_PROMPT, user, ValidationSafetyOutput)
    except (ToolError, AgentOutputError) as ex:
        return failed_update(AGENT, str(ex), calls, started, failure_retries(ex), input_summary)

    violations = merge_verdict(rule_violations, verdict)
    valid = not violations
    report = step_report(
        AGENT, calls, started, retries, "Succeeded", input_summary,
        {"total_lkr": float(quotation.total_lkr), "total_usd": float(quotation.total_usd),
         "fx_rate": float(quotation.fx_rate), "fx_stale": quotation.fx_stale},
        {"ok": True, "valid": valid, "violations": [v.code for v in violations], "schema_ok": schema.ok})

    # Only quotation, violations and status are written. days and resources are never touched here.
    return {
        "quotation": quotation.model_dump(mode="json"),  # always the calculated one, never the LLM's copy
        "violations": [v.model_dump() for v in violations],
        "status": PENDING_APPROVAL if valid else REVISION_REQUESTED,
        "steps": [report],
    }
