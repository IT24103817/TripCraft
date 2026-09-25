"""POSTs step reports and the final proposal back to the ASP.NET Core API. Failures are logged, never raised."""
import logging
from typing import Any

import httpx

from app.config import get_settings
from app.schemas import WorkflowRequest

logger = logging.getLogger("tripcraft.callbacks")
TIMEOUT_SECONDS = 10.0


def _base_url(request: WorkflowRequest) -> str:
    return (request.callback_base_url or get_settings().api_base_url).rstrip("/")


async def _post(request: WorkflowRequest, suffix: str, body: dict[str, Any]) -> None:
    url = f"{_base_url(request)}/api/internal/workflows/{request.workflow_id}/{suffix}"
    headers = {"X-Internal-Key": get_settings().internal_agent_key}
    try:
        async with httpx.AsyncClient(timeout=TIMEOUT_SECONDS) as client:
            response = await client.post(url, json=body, headers=headers)
        if not response.is_success:
            logger.warning(f"callback {suffix} returned {response.status_code}",
                           extra={"workflow_id": str(request.workflow_id)})
    except httpx.HTTPError as ex:
        logger.warning(f"callback {suffix} failed: {type(ex).__name__}", extra={"workflow_id": str(request.workflow_id)})


async def post_step(request: WorkflowRequest, report: dict[str, Any]) -> None:
    await _post(request, "steps", report)


async def post_proposal(request: WorkflowRequest, proposal: dict[str, Any]) -> None:
    await _post(request, "proposal", proposal)
