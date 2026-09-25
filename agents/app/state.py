"""The shared state every LangGraph node reads and updates. Plain JSON-friendly dicts only."""
import operator
from typing import Annotated, Any, TypedDict

RUNNING = "Running"
PENDING_APPROVAL = "PendingApproval"
REVISION_REQUESTED = "RevisionRequested"
FAILED_SAFELY = "FailedSafely"


class WorkflowState(TypedDict):
    workflow_id: str
    objective: str
    request: dict[str, Any]
    plan: dict[str, Any] | None
    days: list[dict[str, Any]]
    resources: dict[str, Any] | None
    quotation: dict[str, Any] | None
    violations: list[dict[str, Any]]
    status: str
    # operator.add means a node returns only its new step(s) and LangGraph appends them.
    steps: Annotated[list[dict[str, Any]], operator.add]
    replans: int
    manager_comment: str | None
    error_summary: str | None
