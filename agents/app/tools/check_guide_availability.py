from datetime import date

from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import GuideOption


class CheckGuideAvailabilityInput(BaseModel):
    from_date: date
    to_date: date
    language: str = Field(pattern=r"^[a-z]{2}$")
    pax: int = Field(gt=0, le=50)


async def check_guide_availability(from_date: date, to_date: date, language: str, pax: int) -> list[GuideOption]:
    """Read-only: lists guides free for the whole range. Never creates a hold."""
    args = CheckGuideAvailabilityInput(from_date=from_date, to_date=to_date, language=language, pax=pax)
    data = await internal_get("/api/internal/availability/guides", {
        "from": args.from_date.isoformat(), "to": args.to_date.isoformat(),
        "language": args.language, "pax": args.pax})
    return [GuideOption.model_validate(item) for item in data]
