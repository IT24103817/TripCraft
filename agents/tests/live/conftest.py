"""
Fixtures for the live suite: the same graph, tools and mocked internal API as the golden suite, but with a real
model (LIVE_LLM_PROVIDER=ollama, groq or gemini). A recorder keeps every raw model reply, so the providers' outputs
can be compared (field names, markdown fences, extra text) without changing any prompt.
"""
import json
import os
import re
import time
from pathlib import Path
from typing import Any

import pytest
from langchain_core.messages import AIMessage

from app import llm
from app.config import get_settings
from tests.conftest import FakeLLM

PROVIDER = os.getenv("LIVE_LLM_PROVIDER", "").lower()
OUTPUT_DIR = Path(os.getenv("LIVE_OUTPUT_DIR", Path(__file__).parent / "output"))


def agent_of(messages: list[Any]) -> str:
    """Which agent is calling, from its system prompt (the same markers the FakeLLM uses)."""
    return next((a for a, marker in FakeLLM.MARKERS.items() if marker in messages[0].content), "unknown")


class Recorder:
    """Wraps the real chat model: records each reply's agent, latency and raw text, optionally replacing replies."""

    def __init__(self, inner: Any, log: list[dict[str, Any]], replace: dict[str, list[str]]) -> None:
        self.inner, self.log, self.replace = inner, log, replace

    async def ainvoke(self, messages: list[Any]) -> AIMessage:
        agent = agent_of(messages)
        if self.replace.get(agent):
            text = self.replace[agent].pop(0)
            self.log.append({"agent": agent, "ms": 0, "text": text, "injected": True})
            return AIMessage(content=text)
        started = time.perf_counter()
        reply = await self.inner.ainvoke(messages)
        text = llm.reply_text(reply)
        usage = getattr(reply, "usage_metadata", None) or {}
        self.log.append({"agent": agent, "ms": int((time.perf_counter() - started) * 1000), "text": text,
                         "repair": messages[-1].content.startswith("Your JSON did not pass validation."),
                         "tokens_in": usage.get("input_tokens"), "tokens_out": usage.get("output_tokens"),
                         "at": time.time()})
        return reply


def shape(text: str) -> dict[str, Any]:
    """What a raw reply looks like: fenced, extra text around the JSON, and its top-level keys."""
    stripped = text.strip()
    keys: list[str] | None
    try:
        parsed = json.loads(stripped)
        keys = sorted(parsed) if isinstance(parsed, dict) else None
    except ValueError:
        keys = None
    return {"fenced": stripped.startswith("```"), "pure_json": keys is not None,
            "extra_text": keys is None and not stripped.startswith("```")
            and bool(re.search(r"[A-Za-z]{4,}", stripped.split("{", 1)[0])), "keys": keys}


# The model's own HTTP calls must reach the real model; everything else stays on the mocked internal API.
MODEL_HOSTS = ("localhost", "127.0.0.1", "api.groq.com", "generativelanguage.googleapis.com")


@pytest.fixture
def live(monkeypatch, request, api):
    """Switches the run to the real provider and records every reply. Skips when no provider is configured."""
    if PROVIDER not in ("ollama", "groq", "gemini"):
        pytest.skip("set LIVE_LLM_PROVIDER=ollama, groq or gemini to run the live suite")
    key = {"groq": "GROQ_API_KEY", "gemini": "GEMINI_API_KEY"}.get(PROVIDER)
    if key and not os.getenv(key):
        pytest.skip(f"LIVE_LLM_PROVIDER={PROVIDER} needs {key}")
    for host in MODEL_HOSTS:
        api.router.route(host=host).pass_through()
    monkeypatch.setenv("LLM_PROVIDER", PROVIDER)
    monkeypatch.setenv("NODE_TIMEOUT_SECONDS", os.getenv("LIVE_NODE_TIMEOUT_SECONDS", "30"))
    get_settings.cache_clear()

    real_factory = llm.get_chat_model
    log: list[dict[str, Any]] = []
    replace: dict[str, list[str]] = {}
    monkeypatch.setattr(llm, "get_chat_model", lambda: Recorder(real_factory(), log, replace))
    yield {"log": log, "replace": replace, "provider": PROVIDER}

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    out = OUTPUT_DIR / f"{PROVIDER}-{request.node.name}.json"
    out.write_text(json.dumps([{**entry, "shape": shape(entry["text"])} for entry in log], indent=2,
                              ensure_ascii=False))
