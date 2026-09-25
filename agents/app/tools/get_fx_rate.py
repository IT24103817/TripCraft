from app.tools.http_client import internal_get
from app.tools.models import FxRate


async def get_fx_rate() -> FxRate:
    """No input: returns LKR per 1 USD. The API falls back to a cached rate and sets stale=true."""
    data = await internal_get("/api/internal/fx-rate")
    return FxRate.model_validate(data)
