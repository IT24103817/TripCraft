from datetime import date

from pydantic import BaseModel, Field

from app.tools.http_client import internal_get
from app.tools.models import VehicleOption


class CheckVehicleAvailabilityInput(BaseModel):
    from_date: date
    to_date: date
    seats: int = Field(gt=0, le=60)


async def check_vehicle_availability(from_date: date, to_date: date, seats: int) -> list[VehicleOption]:
    """Read-only: lists vehicles free for the whole range with at least `seats` seats. Never creates a hold."""
    args = CheckVehicleAvailabilityInput(from_date=from_date, to_date=to_date, seats=seats)
    data = await internal_get("/api/internal/availability/vehicles", {
        "from": args.from_date.isoformat(), "to": args.to_date.isoformat(), "seats": args.seats})
    return [VehicleOption.model_validate(item) for item in data]
