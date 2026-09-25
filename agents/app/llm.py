"""Chat model factory and the call_json helper every agent node uses to talk to the LLM."""
from typing import Callable, TypeVar

from langchain_core.language_models.chat_models import BaseChatModel
from langchain_core.messages import AIMessage, HumanMessage, SystemMessage
from pydantic import BaseModel, ValidationError

from app.config import get_settings
from app.errors import AgentOutputError

T = TypeVar("T", bound=BaseModel)

# Optional extra rules checked in code after the schema passes. Returns a list of problems (empty = OK).
RuleCheck = Callable[[T], list[str]]


def get_chat_model() -> BaseChatModel:
    """Returns the configured chat model, always in JSON output mode and temperature 0."""
    settings = get_settings()
    if settings.llm_provider == "groq":
        from langchain_groq import ChatGroq

        model = ChatGroq(model=settings.groq_model, api_key=settings.groq_api_key, temperature=0)
        return model.bind(response_format={"type": "json_object"})
    if settings.llm_provider == "ollama":
        from langchain_ollama import ChatOllama

        return ChatOllama(model=settings.ollama_model, format="json", temperature=0)
    if settings.llm_provider == "fake":
        # CI sets LLM_PROVIDER=fake: the tests replace get_chat_model with a FakeLLM, so no real model is ever built.
        raise RuntimeError("LLM_PROVIDER=fake is for tests only; they inject a FakeLLM")
    raise ValueError(f"Unknown LLM_PROVIDER '{settings.llm_provider}' (use ollama or groq)")


async def call_json(system: str, user: str, schema: type[T], check: RuleCheck | None = None) -> tuple[T, int]:
    """
    Calls the model and parses its reply into `schema`.
    If parsing (or the optional rule check) fails, sends one repair message with the error and tries again.
    Returns (parsed result, number of retries used). Raises AgentOutputError after MAX_RETRIES repairs.
    """
    max_retries = get_settings().max_retries
    model = get_chat_model()
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
