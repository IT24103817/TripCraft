"""The only way tools reach the ASP.NET Core API: read-only GET calls with the internal key and a 10 s timeout."""
from typing import Any

import httpx

from app.config import get_settings
from app.errors import ToolError

TIMEOUT_SECONDS = 10.0


async def internal_get(path: str, params: dict[str, Any] | None = None) -> Any:
    settings = get_settings()
    headers = {"X-Internal-Key": settings.internal_agent_key}
    try:
        async with httpx.AsyncClient(base_url=settings.api_base_url, timeout=TIMEOUT_SECONDS) as client:
            response = await client.get(path, params=params, headers=headers)
    except httpx.HTTPError as ex:
        raise ToolError(f"GET {path} failed: {type(ex).__name__}") from ex

    if not response.is_success:
        raise ToolError(f"GET {path} returned {response.status_code}")
    return response.json()
