"""Typed results returned by the tools. They mirror the JSON the ASP.NET Core internal API sends."""
from datetime import date, datetime
from decimal import Decimal
from typing import Annotated

from pydantic import BaseModel, ConfigDict, PlainSerializer
from pydantic.alias_generators import to_camel

# Money is exact (Decimal) inside Python and a plain JSON number on the wire.
Money = Annotated[Decimal, PlainSerializer(float, return_type=float, when_used="json")]


class ApiModel(BaseModel):
    """Accepts both snake_case and camelCase keys, so the C# side can use its default naming."""
    model_config = ConfigDict(alias_generator=to_camel, populate_by_name=True)


class Attraction(ApiModel):
    id: str
    name: str
    city: str
    category: str
    duration_minutes: int
    entry_fee_lkr: Money


class Distance(ApiModel):
    from_city: str
    to_city: str
    distance_km: Money
    duration_minutes: int


class Weather(ApiModel):
    city: str
    date: date
    summary: str
    rain_probability: float


class GuideOption(ApiModel):
    id: str
    name: str
    languages: list[str]
    max_pax: int


class VehicleOption(ApiModel):
    id: str
    registration_no: str
    type: str
    seats: int


class RoomOption(ApiModel):
    hotel_id: str
    hotel_name: str
    room_type_id: str
    room_type_name: str
    capacity: int
    available_rooms: int


class RateCard(ApiModel):
    """Prices in LKR keyed by resource id, plus the operator's margin."""
    margin_pct: Money
    guide_day_rates: dict[str, Money]
    vehicle_km_rates: dict[str, Money]
    room_night_rates: dict[str, Money]


class FxRate(ApiModel):
    """How many LKR one USD buys. `stale` is true when the API fell back to a cached rate."""
    base: str = "USD"
    quote: str = "LKR"
    rate: Money
    as_of: datetime
    stale: bool = False


class DateRange(BaseModel):
    """Result of the local parse_dates tool."""
    start_date: date
    end_date: date
    days: int
    dates: list[date]
    nights: list[date]


class AgentInfo(BaseModel):
    """Result item of the local list_agents tool."""
    name: str
    responsibility: str


class SchemaCheck(BaseModel):
    """Result of the local validate_schema tool."""
    ok: bool
    errors: list[str]
