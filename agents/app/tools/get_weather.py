from datetime import date as Date

from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import Weather


class GetWeatherInput(BaseModel):
    city: str = Field(min_length=1, max_length=60, pattern=r"^[A-Za-z .'-]+$")
    date: Date


async def get_weather(city: str, date: Date) -> Weather:
    args = GetWeatherInput(city=city, date=date)
    data = await internal_get("/api/internal/weather", {"city": args.city, "date": args.date.isoformat()})
    return Weather.model_validate(data)
