"""FastAPI entry point. Internal only: every workflow endpoint needs the X-Internal-Key header."""
import hmac
import logging

from fastapi import BackgroundTasks, Depends, FastAPI, Header, HTTPException, status

from app.config import get_settings
from app.graph import run_workflow
from app.logging_setup import configure_logging
from app.schemas import ReplanRequest, WorkflowRequest

configure_logging()
logger = logging.getLogger("tripcraft.api")

app = FastAPI(title="TripCraft agent service", version="1.0.0")


def require_internal_key(x_internal_key: str | None = Header(default=None)) -> None:
    expected = get_settings().internal_agent_key
    # An empty INTERNAL_AGENT_KEY rejects everything, so a missing setting can never open the service.
    if not expected or not x_internal_key or not hmac.compare_digest(x_internal_key.encode(), expected.encode()):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Missing or invalid X-Internal-Key")


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/run-workflow", status_code=status.HTTP_202_ACCEPTED, dependencies=[Depends(require_internal_key)])
async def start_workflow(request: WorkflowRequest, background: BackgroundTasks) -> dict[str, str]:
    logger.info("workflow accepted", extra={"workflow_id": str(request.workflow_id)})
    background.add_task(run_workflow, request)
    return {"workflow_id": str(request.workflow_id), "status": "Accepted"}


@app.post("/replan", status_code=status.HTTP_202_ACCEPTED, dependencies=[Depends(require_internal_key)])
async def replan(request: ReplanRequest, background: BackgroundTasks) -> dict[str, str]:
    logger.info("replan accepted", extra={"workflow_id": str(request.workflow_id)})
    background.add_task(run_workflow, request)
    return {"workflow_id": str(request.workflow_id), "status": "Accepted"}
