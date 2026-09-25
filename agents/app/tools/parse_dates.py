from datetime import date, timedelta

from pydantic import BaseModel, model_validator

from app.tools.models import DateRange

MAX_TRIP_DAYS = 30


class ParseDatesInput(BaseModel):
    start_date: date
    end_date: date

    @model_validator(mode="after")
    def check_range(self) -> "ParseDatesInput":
        if self.end_date < self.start_date:
            raise ValueError("end_date must be on or after start_date")
        if (self.end_date - self.start_date).days + 1 > MAX_TRIP_DAYS:
            raise ValueError(f"trips longer than {MAX_TRIP_DAYS} days are not supported")
        return self


def parse_dates(start_date: date, end_date: date) -> DateRange:
    """Local tool: every trip date, and the nights (every date except the last)."""
    args = ParseDatesInput(start_date=start_date, end_date=end_date)
    count = (args.end_date - args.start_date).days + 1
    dates = [args.start_date + timedelta(days=i) for i in range(count)]
    return DateRange(start_date=args.start_date, end_date=args.end_date, days=count, dates=dates, nights=dates[:-1])
