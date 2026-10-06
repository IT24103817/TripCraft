"""Chat model factory and the call_json helper every agent node uses to talk to the LLM."""
import asyncio
import logging
from collections.abc import Callable
from contextvars import ContextVar
from typing import Literal, TypeVar

from langchain_core.language_models.chat_models import BaseChatModel
from langchain_core.messages import AIMessage, HumanMessage, SystemMessage
from pydantic import BaseModel, ValidationError

from app.config import get_settings
from app.errors import AgentOutputError

T = TypeVar("T", bound=BaseModel)
log = logging.getLogger(__name__)

# Optional extra rules checked in code after the schema passes. Returns a list of problems (empty = OK).
RuleCheck = Callable[[T], list[str]]

# v1.1: the operator picks Ollama, Gemini or Groq in the web Settings page; the API sends the choice with each
# workflow and run_workflow puts it here for that run only. None = the LLM_PROVIDER environment setting.
_provider_for_run: ContextVar[str | None] = ContextVar("llm_provider_for_run", default=None)

# Warnings for the current agent step (e.g. "rate limited, retried"). step_report() takes them into the step's
# validation_result, so the operator sees them in the workflow monitor.
_step_warnings: ContextVar[list[str] | None] = ContextVar("llm_step_warnings", default=None)


def add_step_warning(message: str) -> None:
    warnings = _step_warnings.get()
    if warnings is None:
        warnings = []
        _step_warnings.set(warnings)
    warnings.append(message)


def take_step_warnings() -> list[str]:
    """Returns the warnings collected for this step and clears them."""
    warnings = _step_warnings.get() or []
    _step_warnings.set(None)
    return warnings


def use_provider_for_run(provider: Literal["ollama", "gemini", "groq"] | None) -> None:
    """Sets the LLM provider for the current workflow run (its own asyncio task, so runs never mix)."""
    _provider_for_run.set(provider)


def current_provider() -> str:
    return _provider_for_run.get() or get_settings().llm_provider


def get_chat_model() -> BaseChatModel:
    """Returns the chat model for this run, always in JSON output mode and temperature 0."""
    settings = get_settings()
    provider = current_provider()
    if provider == "groq":
        if not settings.groq_api_key:
            # A clear safe failure instead of a confusing auth error from Groq.
            raise AgentOutputError("Groq is selected in Settings but GROQ_API_KEY is not set on the agent service")
        from langchain_groq import ChatGroq

        # max_retries=0: call_json does the 429 / 503 backoff itself, visible as step warnings.
        model = ChatGroq(model=settings.groq_model, api_key=settings.groq_api_key, temperature=0, max_retries=0)
        return model.bind(response_format={"type": "json_object"})
    if provider == "gemini":
        if not settings.gemini_api_key:
            raise AgentOutputError("Gemini is selected but GEMINI_API_KEY is not set on the agent service")
        from langchain_google_genai import ChatGoogleGenerativeAI

        # JSON output mode like the other providers. max_retries=0: call_json does the rate-limit backoff itself,
        # so every retry is visible as a step warning and bounded by the node timeout.
        return ChatGoogleGenerativeAI(model=settings.gemini_model, google_api_key=settings.gemini_api_key,
                                      temperature=0, response_mime_type="application/json", max_retries=0)
    if provider == "ollama":
        from langchain_ollama import ChatOllama

        return ChatOllama(model=settings.ollama_model, base_url=settings.ollama_base_url, format="json", temperature=0,
                          num_predict=settings.ollama_num_predict)
    if provider == "fake":
        # CI sets LLM_PROVIDER=fake: the tests replace get_chat_model with a FakeLLM, so no real model is ever built.
        raise RuntimeError("LLM_PROVIDER=fake is for tests only; they inject a FakeLLM")
    raise ValueError(f"Unknown LLM provider '{provider}' (use ollama, gemini or groq)")


def _status_of(seen: BaseException) -> int | None:
    code = getattr(seen, "code", None)
    return code if isinstance(code, int) else getattr(seen, "status_code", None)


def is_rate_limited(ex: BaseException) -> bool:
    """True for a provider's "too many requests" answer: HTTP 429 or gRPC RESOURCE_EXHAUSTED, also when wrapped."""
    from langchain_core.exceptions import ModelRateLimitError

    seen: BaseException | None = ex
    while seen is not None:
        if isinstance(seen, ModelRateLimitError) or _status_of(seen) == 429 or "RESOURCE_EXHAUSTED" in str(seen):
            return True
        seen = seen.__cause__
    return False


def retry_delay_seconds(ex: BaseException) -> float | None:
    """The wait the provider asks for (Groq's retry-after header, Gemini's RetryInfo.retryDelay e.g. "46650s"), or
    None if it gave none."""
    seen: BaseException | None = ex
    while seen is not None:
        response = getattr(seen, "response", None)
        retry_after = getattr(getattr(response, "headers", None), "get", lambda _: None)("retry-after")
        if retry_after:
            try:
                return float(retry_after)
            except ValueError:
                return None
        details = getattr(seen, "details", None)
        if isinstance(details, dict):
            for item in details.get("error", {}).get("details", []):
                delay = str(item.get("retryDelay", "")) if isinstance(item, dict) else ""
                if delay.endswith("s"):
                    try:
                        return float(delay[:-1])
                    except ValueError:
                        return None
        seen = seen.__cause__
    return None


def is_overloaded(ex: BaseException) -> bool:
    """True for "the model is busy, try again": HTTP 503 or gRPC UNAVAILABLE (Gemini under high demand)."""
    seen: BaseException | None = ex
    while seen is not None:
        if _status_of(seen) == 503 or "UNAVAILABLE" in str(seen):
            return True
        seen = seen.__cause__
    return False


async def _invoke(model: BaseChatModel, messages: list) -> AIMessage:
    """
    One model call. A rate-limit answer (429 RESOURCE_EXHAUSTED) or an overloaded model (503 UNAVAILABLE) is retried
    after 2 s, 4 s, 8 s (RATE_LIMIT_RETRIES, default 3), each retry recorded as a step warning; after that the node
    fails safely. Any other error fails at once.
    """
    settings = get_settings()
    for attempt in range(settings.rate_limit_retries + 1):
        try:
            return await model.ainvoke(messages)
        except Exception as ex:  # model server down, network error, bad key, quota...
            delay = retry_delay_seconds(ex)
            if is_rate_limited(ex):
                reason, label = "rate limited (429)", "429 RESOURCE_EXHAUSTED"
                if delay is not None and delay > settings.rate_limit_max_wait_seconds:
                    # E.g. a daily quota: waiting inside the node cannot help, so fail safely at once.
                    when = f"{delay / 3600:.1f} h" if delay >= 3600 else f"{delay:.0f} s"
                    raise AgentOutputError(f"LLM quota used up ({label}); the provider asks to retry in {when}") \
                        from ex
            elif is_overloaded(ex):
                reason, label = "overloaded (503)", "503 UNAVAILABLE"
            else:
                raise AgentOutputError(f"LLM call failed: {type(ex).__name__}") from ex
            if attempt == settings.rate_limit_retries:
                raise AgentOutputError(f"LLM {reason.split(' (')[0]} ({label}) after {attempt} retries") from ex
            wait = min(delay, settings.rate_limit_max_wait_seconds) if delay is not None \
                else settings.rate_limit_backoff_seconds * 2 ** attempt
            add_step_warning(f"{current_provider()} {reason}; retry {attempt + 1} after {wait:g} s")
            log.warning("LLM %s; retry %s after %s s", reason, attempt + 1, wait)
            await asyncio.sleep(wait)
    raise AssertionError("unreachable")


def reply_text(reply: AIMessage) -> str:
    """The reply's text. Gemini may return a list of content parts instead of one string."""
    if isinstance(reply.content, str):
        return reply.content
    return "".join(part.get("text", "") if isinstance(part, dict) else str(part) for part in reply.content)


async def call_json(system: str, user: str, schema: type[T], check: RuleCheck | None = None,
                    normalise: Callable[[T], T] | None = None) -> tuple[T, int]:
    """
    Calls the model and parses its reply into `schema`.
    `normalise` (optional) may remove entries that are clearly outside the task, or expand a reference the model
    made to data code gave it (the Resource agent's "use_suggested_rooms"), before the rule check; it never
    invents anything. If parsing (or the optional rule check) fails, sends one repair message with the error
    and tries again. Returns (parsed result, number of retries used). Raises AgentOutputError after MAX_RETRIES.
    """
    max_retries = get_settings().max_retries
    model = get_chat_model()  # an AgentOutputError here (e.g. Gemini without a key) fails the node safely
    messages = [SystemMessage(content=system), HumanMessage(content=user)]
    retries = 0

    while True:
        reply = await _invoke(model, messages)
        text = reply_text(reply)
        usage = getattr(reply, "usage_metadata", None) or {}
        # Token counts only (never the prompt or the reply): hosted free tiers limit tokens per minute and per day.
        log.info("LLM reply for %s: %s input tokens, %s output tokens", schema.__name__,
                 usage.get("input_tokens"), usage.get("output_tokens"))
        try:
            result = schema.model_validate_json(text)
            if normalise:
                result = normalise(result)
            problems = check(result) if check else []
        except ValidationError as ex:
            problems = [f"{'.'.join(str(p) for p in e['loc'])}: {e['msg']}" for e in ex.errors()]

        if not problems:
            return result, retries

        if retries >= max_retries:
            raise AgentOutputError(f"{schema.__name__} invalid after {retries} retries: {'; '.join(problems)[:500]}")

        retries += 1
        log.info("%s needs a repair (%s): %s", schema.__name__, retries, "; ".join(problems)[:300])
        messages.append(AIMessage(content=text))
        messages.append(HumanMessage(content=(
            "Your JSON did not pass validation. Problems: " + "; ".join(problems)
            + ". Reply again with corrected JSON only, matching the schema exactly."
        )))
