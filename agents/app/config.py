"""Settings for the agent service, read from environment variables (never hard-coded)."""
import os
from dataclasses import dataclass
from functools import lru_cache

from dotenv import load_dotenv


@dataclass(frozen=True)
class Settings:
    internal_agent_key: str
    api_base_url: str
    llm_provider: str
    ollama_model: str
    ollama_base_url: str
    ollama_num_predict: int
    groq_api_key: str
    groq_model: str
    gemini_api_key: str
    gemini_model: str
    rate_limit_retries: int
    rate_limit_backoff_seconds: float
    rate_limit_max_wait_seconds: float
    node_timeout_seconds: float
    max_retries: int
    max_replans: int


# Default Gemini model: "Gemini 3.8 Flash", the stable Flash model Google recommends
# (https://ai.google.dev/gemini-api/docs/models, checked 5 Oct 2026). Optional; free tier: 20 requests per day.
DEFAULT_GEMINI_MODEL = "gemini-3.8-flash"

# Default Groq model. llama-3.1-8b-instant is listed in Groq's docs but our key cannot use it (404
# model_not_found); qwen/qwen3.8-27b answers in JSON mode fastest and with the fewest tokens, which matters under the
# free tier's 8,000 tokens per minute (measured 5 Oct 2026, see agents/README.md).
DEFAULT_GROQ_MODEL = "qwen/qwen3.8-27b"


def default_provider() -> str:
    """LLM_PROVIDER when set. Otherwise Groq on a hosted service (Render sets RENDER=true on every service, and
    Ollama cannot run there) and Ollama on a laptop."""
    configured = os.getenv("LLM_PROVIDER", "").strip().lower()
    if configured:
        return configured
    return "groq" if os.getenv("RENDER", "").lower() == "true" else "ollama"


@lru_cache
def get_settings() -> Settings:
    # A local .env file is optional; real environment variables always win.
    load_dotenv(override=False)
    return Settings(
        internal_agent_key=os.getenv("INTERNAL_AGENT_KEY", ""),
        api_base_url=os.getenv("API_BASE_URL", "http://localhost:5080").rstrip("/"),
        llm_provider=default_provider(),
        ollama_model=os.getenv("OLLAMA_MODEL", "llama3.1:8b"),
        # In Docker, Ollama on the host is e.g. http://host.docker.internal:11434.
        ollama_base_url=os.getenv("OLLAMA_BASE_URL", "http://localhost:11434"),
        # Most answers are under 1,000 tokens; in JSON mode the local model sometimes never stops. The cap ends such
        # an answer early, so the repair retry still fits inside the node timeout.
        ollama_num_predict=int(os.getenv("OLLAMA_NUM_PREDICT", "1536")),
        groq_api_key=os.getenv("GROQ_API_KEY", ""),
        groq_model=os.getenv("GROQ_MODEL", DEFAULT_GROQ_MODEL),
        gemini_api_key=os.getenv("GEMINI_API_KEY", ""),
        gemini_model=os.getenv("GEMINI_MODEL", DEFAULT_GEMINI_MODEL),
        # A hosted model answers 429 (rate limit) or 503 (overloaded): wait and try again, then fail the node
        # safely. The wait is the provider's own retry hint when it gives one (Groq's retry-after, Gemini's
        # retryDelay), else 2 s, 4 s, 8 s. A hint longer than the max wait (a daily quota) fails at once.
        rate_limit_retries=int(os.getenv("RATE_LIMIT_RETRIES", "3")),
        rate_limit_backoff_seconds=float(os.getenv("RATE_LIMIT_BACKOFF_SECONDS", "2")),
        rate_limit_max_wait_seconds=float(os.getenv("RATE_LIMIT_MAX_WAIT_SECONDS", "20")),
        node_timeout_seconds=float(os.getenv("NODE_TIMEOUT_SECONDS", "30")),
        max_retries=int(os.getenv("MAX_RETRIES", "2")),
        max_replans=int(os.getenv("MAX_REPLANS", "3")),
    )
