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
    groq_api_key: str
    groq_model: str
    node_timeout_seconds: float
    max_retries: int
    max_replans: int


@lru_cache
def get_settings() -> Settings:
    # A local .env file is optional; real environment variables always win.
    load_dotenv(override=False)
    return Settings(
        internal_agent_key=os.getenv("INTERNAL_AGENT_KEY", ""),
        api_base_url=os.getenv("API_BASE_URL", "http://localhost:5080").rstrip("/"),
        llm_provider=os.getenv("LLM_PROVIDER", "ollama").lower(),
        ollama_model=os.getenv("OLLAMA_MODEL", "llama3.1:8b"),
        groq_api_key=os.getenv("GROQ_API_KEY", ""),
        groq_model=os.getenv("GROQ_MODEL", "llama-3.1-8b-instant"),
        node_timeout_seconds=float(os.getenv("NODE_TIMEOUT_SECONDS", "30")),
        max_retries=int(os.getenv("MAX_RETRIES", "2")),
        max_replans=int(os.getenv("MAX_REPLANS", "3")),
    )
