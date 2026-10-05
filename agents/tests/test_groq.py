"""Groq as the hosted provider: 429 / 503 backoff with Groq's own errors and its retry-after header."""
import httpx
import pytest
from groq import InternalServerError, RateLimitError
from pydantic import BaseModel

from app import llm
from app.errors import AgentOutputError
from app.nodes.planner import SYSTEM_PROMPT as PLANNER_PROMPT

GROQ_URL = "https://api.groq.com/openai/v1/chat/completions"


class Answer(BaseModel):
    value: int


def groq_error(cls: type, status: int, headers: dict[str, str] | None = None) -> Exception:
    response = httpx.Response(status, headers=headers or {}, request=httpx.Request("POST", GROQ_URL))
    return cls(f"Error code: {status}", response=response, body=None)


@pytest.fixture
def waits(monkeypatch) -> list[float]:
    recorded: list[float] = []

    async def no_sleep(seconds: float) -> None:
        recorded.append(seconds)

    monkeypatch.setattr(llm.asyncio, "sleep", no_sleep)
    return recorded


async def test_a_groq_429_waits_for_its_retry_after_then_succeeds(fake_llm, waits):
    async def limited() -> str:
        raise groq_error(RateLimitError, 429, {"retry-after": "3"})

    fake_llm.queue("planner", limited, '{"value": 7}')

    result, retries = await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert result.value == 7 and retries == 0
    assert waits == [3], "Groq's own retry-after is used instead of the fixed backoff"
    assert llm.take_step_warnings() == ["ollama rate limited (429); retry 1 after 3 s"]


async def test_a_groq_429_without_retry_after_uses_the_backoff(fake_llm, waits):
    async def limited() -> str:
        raise groq_error(RateLimitError, 429)

    fake_llm.queue("planner", limited, limited, '{"value": 7}')

    await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [2, 4]
    llm.take_step_warnings()


async def test_a_long_groq_retry_after_fails_at_once(fake_llm, waits):
    async def daily_limit() -> str:
        raise groq_error(RateLimitError, 429, {"retry-after": "90"})

    fake_llm.queue("planner", daily_limit)

    with pytest.raises(AgentOutputError, match=r"quota used up \(429 RESOURCE_EXHAUSTED\); .* retry in 90 s"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [] and len(fake_llm.calls_for("planner")) == 1


async def test_a_groq_503_is_retried_then_fails_safely(fake_llm, waits):
    async def busy() -> str:
        raise groq_error(InternalServerError, 503)

    fake_llm.queue("planner", busy)

    with pytest.raises(AgentOutputError, match=r"overloaded \(503 UNAVAILABLE\) after 3 retries"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert waits == [2, 4, 8]
    assert len(llm.take_step_warnings()) == 3


def test_groq_errors_are_classified_by_status():
    assert llm.is_rate_limited(groq_error(RateLimitError, 429))
    assert llm.is_overloaded(groq_error(InternalServerError, 503))
    assert not llm.is_rate_limited(groq_error(InternalServerError, 500))
    assert not llm.is_overloaded(groq_error(InternalServerError, 500))
    assert llm.retry_delay_seconds(groq_error(RateLimitError, 429, {"retry-after": "7"})) == 7
