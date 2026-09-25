from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import Distance

CITY = r"^[A-Za-z .'-]+$"


class GetDistanceInput(BaseModel):
    from_city: str = Field(min_length=1, max_length=60, pattern=CITY)
    to_city: str = Field(min_length=1, max_length=60, pattern=CITY)


async def get_distance(from_city: str, to_city: str) -> Distance:
    args = GetDistanceInput(from_city=from_city, to_city=to_city)
    data = await internal_get("/api/internal/distance", {"from": args.from_city, "to": args.to_city})
    return Distance.model_validate(data)
