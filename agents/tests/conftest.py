"""Shared fixtures: environment, a FakeLLM with canned JSON per agent, and respx mocks of the internal API."""
import copy
import json
from pathlib import Path
from typing import Any, ClassVar

import httpx
import pytest
import respx
from langchain_core.messages import AIMessage

from app import llm
from app.config import get_settings
from app.graph import initial_state
from app.schemas import WorkflowRequest

FIXTURES = Path(__file__).parent / "fixtures"
API = "http://api.test"
INTERNAL_KEY = "test-internal-key"

DEMO_REQUEST: dict[str, Any] = {
    "workflow_id": "3f1c9a52-7d4e-4b8a-9c11-2a5e6f7b8c90",
    "objective": "5 days for 4 people, 10-14 October, Kandy and Ella, budget USD 1,500, "
                 "prefer the hill-country train, English-speaking guide.",
    "start_date": "2026-10-10",
    "end_date": "2026-10-14",
    "pax": 4,
    "budget_usd": 1500,
    "preferences": {"transport": "train", "language": "en"},
    "callback_base_url": API,
}


def load_fixture(name: str) -> Any:
    return json.loads((FIXTURES / f"{name}.json").read_text())


def demo_request(**changes: Any) -> WorkflowRequest:
    return WorkflowRequest.model_validate({**DEMO_REQUEST, **changes})


def apply(state: dict[str, Any], update: dict[str, Any]) -> dict[str, Any]:
    """Merges a node's update into the state the same way LangGraph does (steps are appended)."""
    merged = {**state, **update}
    merged["steps"] = state["steps"] + update.get("steps", [])
    return merged


@pytest.fixture(autouse=True)
def settings_env(monkeypatch):
    monkeypatch.setenv("INTERNAL_AGENT_KEY", INTERNAL_KEY)
    monkeypatch.setenv("API_BASE_URL", API)
    monkeypatch.setenv("LLM_PROVIDER", "ollama")
    monkeypatch.setenv("MAX_RETRIES", "2")
    monkeypatch.setenv("MAX_REPLANS", "3")
    monkeypatch.setenv("NODE_TIMEOUT_SECONDS", "30")
    get_settings.cache_clear()
    yield
    get_settings.cache_clear()


class FakeLLM:
    """Stands in for the chat model. Picks the agent from the system prompt and replies with canned JSON."""

    MARKERS: ClassVar[dict[str, str]] = {
        "planner": "Planner / Coordinator agent",
        "itinerary": "Itinerary Analysis agent",
        "resources": "Resource & Action agent",
        "validation": "Validation & Safety agent",
    }

    def __init__(self) -> None:
        self.responses: dict[str, list[Any]] = {agent: [load_fixture(agent)] for agent in self.MARKERS}
        self.calls: list[tuple[str, list[Any]]] = []

    def queue(self, agent: str, *responses: Any) -> None:
        """Replies used in order; the last one repeats once the queue is down to one."""
        self.responses[agent] = list(responses)

    def calls_for(self, agent: str) -> list[list[Any]]:
        return [messages for name, messages in self.calls if name == agent]

    async def ainvoke(self, messages: list[Any]) -> AIMessage:
        agent = next(a for a, marker in self.MARKERS.items() if marker in messages[0].content)
        self.calls.append((agent, list(messages)))
        queue = self.responses[agent]
        response = queue.pop(0) if len(queue) > 1 else queue[0]
        if callable(response):
            response = await response()
        return AIMessage(content=response if isinstance(response, str) else json.dumps(response))


@pytest.fixture
def fake_llm(monkeypatch) -> FakeLLM:
    fake = FakeLLM()
    monkeypatch.setattr(llm, "get_chat_model", lambda: fake)
    return fake


class ApiMock:
    """respx routes for every internal API endpoint the tools and callbacks use."""

    def __init__(self, router: respx.MockRouter) -> None:
        self.data = copy.deepcopy(load_fixture("api_data"))
        self.router = router
        r = router
        self.attractions = r.get(f"{API}/api/internal/attractions").mock(side_effect=self._attractions)
        self.distance = r.get(f"{API}/api/internal/distance").mock(side_effect=lambda req: httpx.Response(
            200, json=self.data["distance"]))
        self.weather = r.get(f"{API}/api/internal/weather").mock(side_effect=self._weather)
        self.guides = r.get(f"{API}/api/internal/availability/guides").mock(side_effect=lambda req: httpx.Response(
            200, json=self.data["guides"]))
        self.vehicles = r.get(f"{API}/api/internal/availability/vehicles").mock(
            side_effect=lambda req: httpx.Response(200, json=self.data["vehicles"]))
        self.rooms = r.get(f"{API}/api/internal/availability/rooms").mock(side_effect=self._rooms)
        self.rate_card = r.get(f"{API}/api/internal/rate-card").mock(side_effect=lambda req: httpx.Response(
            200, json=self.data["rate_card"]))
        self.fx = r.get(f"{API}/api/internal/fx-rate").mock(side_effect=lambda req: httpx.Response(
            200, json=self.data["fx"]))
        self.steps = r.post(url__regex=rf"{API}/api/internal/workflows/[^/]+/steps").mock(
            return_value=httpx.Response(200))
        self.proposal = r.post(url__regex=rf"{API}/api/internal/workflows/[^/]+/proposal").mock(
            return_value=httpx.Response(200))

    def _attractions(self, request: httpx.Request) -> httpx.Response:
        return httpx.Response(200, json=self.data["attractions"].get(request.url.params["city"].lower(), []))

    def _rooms(self, request: httpx.Request) -> httpx.Response:
        return httpx.Response(200, json=self.data["rooms"].get(request.url.params["city"].lower(), []))

    def _weather(self, request: httpx.Request) -> httpx.Response:
        params = request.url.params
        return httpx.Response(200, json={"city": params["city"], "date": params["date"], "summary": "light rain",
                                         "rain_probability": 0.4})

    @staticmethod
    def bodies(route: respx.Route) -> list[dict[str, Any]]:
        return [json.loads(call.request.content) for call in route.calls]


@pytest.fixture
def api() -> ApiMock:
    with respx.mock(assert_all_called=False) as router:
        yield ApiMock(router)


@pytest.fixture
def demo_state() -> dict[str, Any]:
    return dict(initial_state(demo_request()))
