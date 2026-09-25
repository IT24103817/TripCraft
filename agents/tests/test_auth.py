from fastapi.testclient import TestClient

from app.main import app
from tests.conftest import DEMO_REQUEST, INTERNAL_KEY

client = TestClient(app)


def test_run_workflow_without_key_returns_401():
    response = client.post("/run-workflow", json=DEMO_REQUEST)
    assert response.status_code == 401


def test_run_workflow_with_wrong_key_returns_401():
    response = client.post("/run-workflow", json=DEMO_REQUEST, headers={"X-Internal-Key": "wrong"})
    assert response.status_code == 401


def test_replan_without_key_returns_401():
    response = client.post("/replan", json={**DEMO_REQUEST, "manager_comment": "cheaper please"})
    assert response.status_code == 401


def test_run_workflow_with_key_returns_202_and_runs_in_background(fake_llm, api):
    response = client.post("/run-workflow", json=DEMO_REQUEST, headers={"X-Internal-Key": INTERNAL_KEY})

    assert response.status_code == 202
    assert response.json() == {"workflow_id": DEMO_REQUEST["workflow_id"], "status": "Accepted"}
    assert len(api.bodies(api.proposal)) == 1  # TestClient runs background tasks before returning


def test_accepts_camel_case_request_from_csharp(fake_llm, api):
    body = {"workflowId": DEMO_REQUEST["workflow_id"], "objective": DEMO_REQUEST["objective"],
            "startDate": "2026-10-10", "endDate": "2026-10-14", "pax": 4, "budgetUsd": 1500,
            "preferencesJson": "{\"transport\": \"train\"}", "tripRequestId": "ignored", "skeleton": []}
    response = client.post("/run-workflow", json=body, headers={"X-Internal-Key": INTERNAL_KEY})
    assert response.status_code == 202


def test_health_is_open():
    assert client.get("/health").json() == {"status": "ok"}
