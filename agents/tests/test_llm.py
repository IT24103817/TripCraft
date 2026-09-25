import pytest
from pydantic import BaseModel

from app import llm
from app.config import get_settings
from app.errors import AgentOutputError
from app.nodes.planner import SYSTEM_PROMPT as PLANNER_PROMPT


class Answer(BaseModel):
    value: int


async def test_call_json_repairs_then_raises_after_max_retries(fake_llm):
    fake_llm.queue("planner", "not json at all")

    with pytest.raises(AgentOutputError, match="after 2 retries"):
        await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    messages = fake_llm.calls_for("planner")[-1]
    assert len(fake_llm.calls_for("planner")) == 3
    assert messages[-1].content.startswith("Your JSON did not pass validation.")


async def test_call_json_returns_parsed_model_and_retry_count(fake_llm):
    fake_llm.queue("planner", '{"value": "x"}', '{"value": 7}')

    result, retries = await llm.call_json(PLANNER_PROMPT, "<DATA>{}</DATA>", Answer)

    assert result.value == 7 and retries == 1


def test_factory_uses_json_mode_for_each_provider(monkeypatch):
    ollama = llm.get_chat_model()
    assert ollama.format == "json" and ollama.model == "llama3.1:8b"

    monkeypatch.setenv("LLM_PROVIDER", "groq")
    monkeypatch.setenv("GROQ_API_KEY", "placeholder-not-a-real-key")
    get_settings.cache_clear()
    groq = llm.get_chat_model()
    assert groq.kwargs["response_format"] == {"type": "json_object"}
    assert groq.bound.model_name == "llama-3.1-8b-instant"
