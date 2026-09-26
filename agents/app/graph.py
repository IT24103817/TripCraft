"""
The LangGraph workflow: planner -> itinerary -> resources -> validation -> END.
A budget-only failure loops back to the planner (at most MAX_REPLANS times). Any safe failure ends the run.
"""
import asyncio
import logging
import time
from collections.abc import Awaitable, Callable
from typing import Any

from langgraph.graph import END, START, StateGraph

from app.callbacks import post_proposal, post_step
from app.config import get_settings
from app.errors import ToolNotAllowed
from app.nodes.common import failed_update
from app.nodes.itinerary import itinerary_node
from app.nodes.planner import planner_node
from app.nodes.resources import resources_node
from app.nodes.validation import validation_node
from app.schemas import Proposal, WorkflowRequest
from app.state import FAILED_SAFELY, PENDING_APPROVAL, RUNNING, WorkflowState
from app.tools.check_business_rules import BUDGET_CODES

logger = logging.getLogger("tripcraft.graph")

NodeFn = Callable[[WorkflowState], Awaitable[dict[str, Any]]]


def guarded(name: str, node: NodeFn) -> NodeFn:
    """Adds a timeout and a last-resort error guard to a node, then POSTs its step report to the API."""
    async def run(state: WorkflowState) -> dict[str, Any]:
        timeout = get_settings().node_timeout_seconds
        started = time.perf_counter()
        try:
            update = await asyncio.wait_for(node(state), timeout=timeout)
        except TimeoutError:
            update = failed_update(name, f"timed out after {timeout:g} s", [], started, 0, {})
        except ToolNotAllowed as ex:  # a node asked for a tool outside its allow-list
            logger.warning("tool not allowed", extra={"workflow_id": state["workflow_id"], "node": name})
            update = failed_update(name, f"tool not allowed: {ex}", [], started, 0, {})
        except Exception as ex:  # a bug must still end the workflow safely, never crash it
            logger.exception("node crashed", extra={"workflow_id": state["workflow_id"], "node": name})
            update = failed_update(name, f"unexpected {type(ex).__name__}", [], started, 0, {})

        report = update["steps"][-1]
        logger.info("node finished", extra={"workflow_id": state["workflow_id"], "node": name,
                                            "duration_ms": report["duration_ms"], "status": report["status"]})
        await post_step(WorkflowRequest.model_validate(state["request"]), report)
        return update
    return run


async def prepare_replan(state: WorkflowState) -> dict[str, Any]:
    """Not an agent: counts the re-plan. The planner reads state['violations'] as its revision context."""
    return {"replans": state["replans"] + 1, "status": RUNNING}


def next_unless_failed(next_node: str) -> Callable[[WorkflowState], str]:
    return lambda state: END if state["status"] == FAILED_SAFELY else next_node


def after_validation(state: WorkflowState) -> str:
    if state["status"] in (FAILED_SAFELY, PENDING_APPROVAL):
        return END
    codes = {v["code"] for v in state["violations"]}
    if codes and codes <= BUDGET_CODES and state["replans"] < get_settings().max_replans:
        return "prepare_replan"
    return END


def build_graph():
    graph = StateGraph(WorkflowState)
    graph.add_node("planner", guarded("planner", planner_node))
    graph.add_node("itinerary", guarded("itinerary", itinerary_node))
    graph.add_node("resources", guarded("resources", resources_node))
    graph.add_node("validation", guarded("validation", validation_node))
    graph.add_node("prepare_replan", prepare_replan)

    graph.add_edge(START, "planner")
    graph.add_conditional_edges("planner", next_unless_failed("itinerary"), ["itinerary", END])
    graph.add_conditional_edges("itinerary", next_unless_failed("resources"), ["resources", END])
    graph.add_conditional_edges("resources", next_unless_failed("validation"), ["validation", END])
    graph.add_conditional_edges("validation", after_validation, ["prepare_replan", END])
    graph.add_edge("prepare_replan", "planner")
    return graph.compile()


GRAPH = build_graph()


def initial_state(request: WorkflowRequest) -> WorkflowState:
    return WorkflowState(
        workflow_id=str(request.workflow_id), objective=request.objective, request=request.model_dump(mode="json"),
        plan=None, days=[], resources=None, quotation=None, violations=[], status=RUNNING, steps=[], replans=0,
        manager_comment=request.manager_comment, error_summary=None)


async def run_workflow(request: WorkflowRequest) -> WorkflowState:
    """Runs the graph to the end and POSTs the proposal. Never raises (it runs as a background task)."""
    state = initial_state(request)
    try:
        state = await GRAPH.ainvoke(state, config={"recursion_limit": 50})
    except Exception as ex:
        logger.exception("workflow crashed", extra={"workflow_id": state["workflow_id"]})
        state = {**state, "status": FAILED_SAFELY, "error_summary": f"workflow: unexpected {type(ex).__name__}"}

    proposal = Proposal(plan=state["plan"], days=state["days"], resources=state["resources"],
                        quotation=state["quotation"], violations=state["violations"], status=state["status"],
                        replans=state["replans"], error_summary=state.get("error_summary"))
    await post_proposal(request, proposal.model_dump(mode="json"))
    logger.info("workflow finished", extra={"workflow_id": state["workflow_id"], "status": state["status"]})
    return state
