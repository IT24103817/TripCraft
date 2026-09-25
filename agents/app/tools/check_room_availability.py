from datetime import date

from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import RoomOption


class CheckRoomAvailabilityInput(BaseModel):
    hotel_city: str = Field(min_length=1, max_length=60, pattern=r"^[A-Za-z .'-]+$")
    night: date
    rooms: int = Field(gt=0, le=30)


async def check_room_availability(hotel_city: str, night: date, rooms: int) -> list[RoomOption]:
    """Read-only: lists room types in the city with at least `rooms` rooms free that night. Never creates a hold."""
    args = CheckRoomAvailabilityInput(hotel_city=hotel_city, night=night, rooms=rooms)
    data = await internal_get("/api/internal/availability/rooms", {
        "city": args.hotel_city, "night": args.night.isoformat(), "rooms": args.rooms})
    return [RoomOption.model_validate(item) for item in data]
