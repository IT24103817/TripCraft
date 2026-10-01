"""Chat model factory and the call_json helper every agent node uses to talk to the LLM."""
from collections.abc import Callable
from contextvars import ContextVar
from typing import Literal, TypeVar

from langchain_core.language_models.chat_models import BaseChatModel
from langchain_core.messages import AIMessage, HumanMessage, SystemMessage
from pydantic import BaseModel, ValidationError

from app.config import get_settings
from app.errors import AgentOutputError

T = TypeVar("T", bound=BaseModel)

# Optional extra rules checked in code after the schema passes. Returns a list of problems (empty = OK).
RuleCheck = Callable[[T], list[str]]

# v1.1: the operator picks Ollama or Groq in the web Settings page; the API sends the choice with each workflow and
# run_workflow puts it here for that run only. None = the LLM_PROVIDER environment setting.
_provider_for_run: ContextVar[str | None] = ContextVar("llm_provider_for_run", default=None)


def use_provider_for_run(provider: Literal["ollama", "groq"] | None) -> None:
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

        model = ChatGroq(model=settings.groq_model, api_key=settings.groq_api_key, temperature=0)
        return model.bind(response_format={"type": "json_object"})
    if provider == "ollama":
        from langchain_ollama import ChatOllama

        return ChatOllama(model=settings.ollama_model, base_url=settings.ollama_base_url, format="json", temperature=0)
    if provider == "fake":
        # CI sets LLM_PROVIDER=fake: the tests replace get_chat_model with a FakeLLM, so no real model is ever built.
        raise RuntimeError("LLM_PROVIDER=fake is for tests only; they inject a FakeLLM")
    raise ValueError(f"Unknown LLM provider '{provider}' (use ollama or groq)")


async def call_json(system: str, user: str, schema: type[T], check: RuleCheck | None = None,
                    normalise: Callable[[T], T] | None = None) -> tuple[T, int]:
    """
    Calls the model and parses its reply into `schema`.
    `normalise` (optional) may remove entries that are clearly outside the task before the rule check; it must
    never add anything. If parsing (or the optional rule check) fails, sends one repair message with the error
    and tries again. Returns (parsed result, number of retries used). Raises AgentOutputError after MAX_RETRIES.
    """
    max_retries = get_settings().max_retries
    model = get_chat_model()  # an AgentOutputError here (e.g. Groq without a key) fails the node safely
    messages = [SystemMessage(content=system), HumanMessage(content=user)]
    retries = 0

    while True:
        try:
            reply = await model.ainvoke(messages)
        except Exception as ex:  # model server down, network error, bad key...
            raise AgentOutputError(f"LLM call failed: {type(ex).__name__}") from ex

        text = reply.content if isinstance(reply.content, str) else str(reply.content)
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
        messages.append(AIMessage(content=text))
        messages.append(HumanMessage(content=(
            "Your JSON did not pass validation. Problems: " + "; ".join(problems)
            + ". Reply again with corrected JSON only, matching the schema exactly."
        )))
