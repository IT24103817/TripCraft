from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import Attraction


class GetAttractionsInput(BaseModel):
    city: str = Field(min_length=1, max_length=60, pattern=r"^[A-Za-z .'-]+$")


async def get_attractions(city: str) -> list[Attraction]:
    args = GetAttractionsInput(city=city)
    data = await internal_get("/api/internal/attractions", {"city": args.city})
    return [Attraction.model_validate(item) for item in data]
