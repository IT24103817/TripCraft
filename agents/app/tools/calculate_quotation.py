"""
Local tool: the quotation formula from PLAN.md section 3, Component C. The C# QuotationCalculator must
give the same numbers:
  guide    = day_rate_lkr  x trip days
  vehicle  = rate_per_km   x total transfer km
  rooms    = rate_per_night x room-nights (one line per room type)
  entry    = entry_fee_lkr x pax for every stop with a fee
  subtotal = sum of lines;  margin = subtotal x margin_pct / 100;  total_lkr = subtotal + margin
  total_usd = total_lkr / fx_rate (LKR per USD)
Every amount is rounded to 2 decimals, half away from zero (C#: MidpointRounding.AwayFromZero).
"""
from collections import Counter
from decimal import ROUND_HALF_UP, Decimal

from pydantic import BaseModel, Field

from app.errors import ToolError
from app.schemas import ItineraryDay, Quotation, QuotationLine, ResourceSelection
from app.tools.models import FxRate

TWO_PLACES = Decimal("0.01")


class CalculateQuotationInput(BaseModel):
    days: list[ItineraryDay] = Field(min_length=1)
    resources: ResourceSelection
    pax: int = Field(gt=0)
    fx: FxRate


def round_money(value: Decimal) -> Decimal:
    return value.quantize(TWO_PLACES, rounding=ROUND_HALF_UP)


def _line(line_type: str, description: str, qty: Decimal, unit: Decimal) -> QuotationLine:
    return QuotationLine(line_type=line_type, description=description, qty=qty, unit_lkr=unit,
                         amount_lkr=round_money(qty * unit))


def _rate(rates: dict[str, Decimal], resource_id: str) -> Decimal:
    if resource_id not in rates:
        raise ToolError(f"rate card has no rate for {resource_id}")
    return rates[resource_id]


def calculate_quotation(days: list[ItineraryDay], resources: ResourceSelection, pax: int, fx: FxRate) -> Quotation:
    args = CalculateQuotationInput(days=days, resources=resources, pax=pax, fx=fx)
    card = args.resources.rate_card
    lines: list[QuotationLine] = []

    if args.resources.guide_id:
        rate = _rate(card.guide_day_rates, args.resources.guide_id)
        lines.append(_line("guide", f"Guide {args.resources.guide_id}", Decimal(len(args.days)), rate))

    if args.resources.vehicle_id:
        rate = _rate(card.vehicle_km_rates, args.resources.vehicle_id)
        total_km = sum((d.transfer_km for d in args.days), Decimal(0))
        lines.append(_line("vehicle", f"Vehicle {args.resources.vehicle_id}", total_km, rate))

    room_nights = Counter(r.room_type_id for r in args.resources.rooms)
    for room_type_id, count in sorted(room_nights.items()):
        rate = _rate(card.room_night_rates, room_type_id)
        lines.append(_line("room", f"Room type {room_type_id}", Decimal(count), rate))

    for day in args.days:
        for stop in day.stops:
            if stop.entry_fee_lkr > 0:
                lines.append(_line("entry", f"{stop.name} (day {day.day})", Decimal(args.pax), stop.entry_fee_lkr))

    subtotal = sum((line.amount_lkr for line in lines), Decimal(0))
    margin = round_money(subtotal * card.margin_pct / 100)
    total_lkr = subtotal + margin
    return Quotation(
        lines=lines, subtotal_lkr=subtotal, margin_pct=card.margin_pct, margin_lkr=margin, total_lkr=total_lkr,
        fx_rate=args.fx.rate, fx_as_of=args.fx.as_of, fx_stale=args.fx.stale,
        total_usd=round_money(total_lkr / args.fx.rate))
