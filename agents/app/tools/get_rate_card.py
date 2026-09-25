from app.tools.http_client import internal_get
from app.tools.models import RateCard


async def get_rate_card() -> RateCard:
    """No input: returns the current LKR rates for guides, vehicles and room types, and the margin."""
    data = await internal_get("/api/internal/rate-card")
    return RateCard.model_validate(data)
