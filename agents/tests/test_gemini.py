"""Gemini as the hosted provider: the factory, the hosted default, and rate-limit backoff with step warnings."""
import pytest
from google.genai.errors import ClientError
from langchain_core.messages import AIMessage
from langchain_google_genai.chat_models import GoogleRateLimitError
from pydantic import BaseModel

from app import llm
from app.config import get_settings
from app.errors import AgentOutputError
from app.nodes.common import step_report
from app.nodes.planner import SYSTEM_PROMPT as PLANNER_PROMPT
from app.schemas import WorkflowRequest
from tests.conftest import DEMO_REQUEST


class Answer(BaseModel):
    value: int


def rate_limit_error() -> GoogleRateLimitError:
    return GoogleRateLimitError("Error calling model 'gemini-3.8-flash' (RESOURCE_EXHAUSTED): 429 quota exceeded")


@pytest.fixture
def waits(monkeypatch) -> list[float]:
    """Records the backoff waits instead of sleeping."""
    recorded: list[float] = []

    async def no_sleep(seconds: float) -> None:
        recorded.append(seconds)

    monkeypatch.setattr(llm.asyncio, "sleep", no_sleep)
    return recorded


def test_gemini_is_built_in_json_mode_with_the_documented_flash_model(monkeypatch):
    monkeypatch.setenv("LLM_PROVIDER", "gemini")
    monkeypatch.setenv("GEMINI_API_KEY", "placeholder-not-a-real-key")
    get_settings.cache_clear()

    gemini = llm.get_chat_model()

    assert gemini.model.endswith("gemini-3.8-flash")
    assert gemini.response_mime_type == "application/json"
    assert gemini.temperature == 0
    assert gemini.max_retries == 0, "call_json does the rate-limit backoff itself"


def test_gemini_without_a_key_fails_safely_with_a_clear_message(monkeypatch):
    monkeypatch.setenv("GEMINI_API_KEY", "")
    get_settings.cache_clear()

    llm.use_provider_for_run("gemini")
    try:
        with pytest.raises(AgentOutputError, match="GEMINI_API_KEY is not set"):
            llm.get_chat_model()
    finally:
        llm.use_provider_for_run(None)


@pytest.mark.parametrize(("provider", "render", "expected"), [
    ("", "true", "groq"),        # hosted on Render, nothing configured: Groq
    ("", "", "ollama"),          # a laptop: Ollama
    ("ollama", "true", "ollama"),  # an explicit LLM_PROVIDER always wins
    ("gemini", "", "gemini"),
])
def test_the_default_provider_is_groq_when_hosted_and_ollama_locally(monkeypatch, provider, render, expected):
    monkeypatch.setenv("LLM_PROVIDER", provider)
    monkeypatch.setenv("RENDER", render)
    get_settings.cache_clear()

    assert get_settings().llm_provider == expected


def test_a_run_may_ask_for_gemini():
    assert WorkflowRequest.model_validate({**DEMO_REQUEST, "llm_provider": "gemini"}).llm_provider == "gemini"


async def test_a_rate_limited_call_backs_off_retries_and_records_step_warnings(fake_llm, waits):
    async def limited() -> str:
        raise rate_limit_error()

    fake_llm.queue("planner", limited, limited, '{"value": 7}')

    result, retries = await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert result.value == 7
    assert retries == 0, "a rate-limit retry is not a repair retry"
    assert waits == [2, 4]
    report = step_report("planner", [], 0.0, retries, "Succeeded", {}, {}, {"ok": True})
    assert report["validation_result"]["warnings"] == [
        "ollama rate limited (429); retry 1 after 2 s",
        "ollama rate limited (429); retry 2 after 4 s",
    ]
    assert llm.take_step_warnings() == [], "the warnings belong to that one step"


async def test_still_rate_limited_after_three_retries_fails_safely(fake_llm, waits):
    async def limited() -> str:
        raise rate_limit_error()

    fake_llm.queue("planner", limited)

    with pytest.raises(AgentOutputError, match=r"rate limited \(429 RESOURCE_EXHAUSTED\) after 3 retries"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [2, 4, 8]
    assert len(fake_llm.calls_for("planner")) == 4
    assert len(llm.take_step_warnings()) == 3


async def test_an_overloaded_model_is_retried_the_same_way(fake_llm, waits):
    from langchain_google_genai.chat_models import GoogleAPIError

    async def busy() -> str:
        raise GoogleAPIError(503, {"error": {"code": 503, "status": "UNAVAILABLE",
                                             "message": "This model is currently experiencing high demand."}})

    fake_llm.queue("planner", busy, '{"value": 7}')

    result, _ = await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert result.value == 7 and waits == [2]
    assert llm.take_step_warnings() == ["ollama overloaded (503); retry 1 after 2 s"]


async def test_still_overloaded_after_three_retries_fails_safely(fake_llm, waits):
    async def busy() -> str:
        raise ClientError(503, {"error": {"code": 503, "status": "UNAVAILABLE", "message": "high demand"}})

    fake_llm.queue("planner", busy)

    with pytest.raises(AgentOutputError, match=r"overloaded \(503 UNAVAILABLE\) after 3 retries"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [2, 4, 8]
    llm.take_step_warnings()


async def test_a_used_up_daily_quota_fails_at_once_instead_of_waiting(fake_llm, waits):
    # What the Gemini free tier really answers after 20 requests in a day for one model.
    async def daily_quota() -> str:
        raise ClientError(429, {"error": {"code": 429, "status": "RESOURCE_EXHAUSTED", "message": "Quota exceeded",
                                          "details": [{"@type": "type.googleapis.com/google.rpc.RetryInfo",
                                                       "retryDelay": "46650s"}]}})

    fake_llm.queue("planner", daily_quota)

    with pytest.raises(AgentOutputError, match=r"quota used up \(429 RESOURCE_EXHAUSTED\); .* retry in 13\.0 h"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [] and len(fake_llm.calls_for("planner")) == 1


async def test_other_errors_are_not_retried(fake_llm, waits):
    async def broken() -> str:
        raise ConnectionError("model server down")

    fake_llm.queue("planner", broken, '{"value": 7}')

    with pytest.raises(AgentOutputError, match="LLM call failed: ConnectionError"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == []
    assert len(fake_llm.calls_for("planner")) == 1


def test_rate_limits_are_recognised_from_every_provider():
    raw = ClientError(429, {"error": {"code": 429, "status": "RESOURCE_EXHAUSTED", "message": "Quota exceeded"}})
    wrapped = AgentOutputError("LLM call failed")
    wrapped.__cause__ = raw

    assert llm.is_rate_limited(rate_limit_error())
    assert llm.is_rate_limited(raw)
    assert llm.is_rate_limited(wrapped)
    assert not llm.is_rate_limited(ValueError("bad JSON at line 429"))
    assert llm.is_overloaded(ClientError(503, {"error": {"code": 503, "status": "UNAVAILABLE", "message": "busy"}}))
    assert not llm.is_overloaded(rate_limit_error())


def test_a_reply_made_of_content_parts_is_joined_into_one_text():
    reply = AIMessage(content=[{"type": "text", "text": '{"value": '}, {"type": "text", "text": "7}"}])

    assert llm.reply_text(reply) == '{"value": 7}'
