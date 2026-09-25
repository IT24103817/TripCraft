"""Local tool: deterministic operator rules. The LLM can never switch these off."""
from decimal import Decimal

from app.schemas import ItineraryDay, ResourceSelection, Violation

MAX_STOPS_PER_DAY = 3
MAX_DRIVING_MINUTES = 4 * 60

OVER_BUDGET = "OVER_BUDGET"
BUDGET_CODES = {OVER_BUDGET}


def check_business_rules(days: list[ItineraryDay], resources: ResourceSelection, pax: int, language: str,
                         total_usd: Decimal, budget_usd: Decimal) -> list[Violation]:
    violations: list[Violation] = []

    for day in days:
        if not 1 <= len(day.stops) <= MAX_STOPS_PER_DAY:
            violations.append(Violation(code="DAY_STOPS",
                                        message=f"Day {day.day} has {len(day.stops)} stops (allowed 1-3)."))
        if day.driving_minutes > MAX_DRIVING_MINUTES:
            violations.append(Violation(code="DRIVING_LIMIT",
                                        message=f"Day {day.day} needs {day.driving_minutes} min driving (max 240)."))

    if resources.vehicle_id is None or (resources.vehicle_seats or 0) < pax:
        violations.append(Violation(code="VEHICLE_SEATS",
                                    message=f"Vehicle seats {resources.vehicle_seats or 0} < pax {pax}."))

    if resources.guide_id is None or language not in resources.guide_languages:
        violations.append(Violation(code="GUIDE_LANGUAGE", message=f"No guide speaking '{language}' is assigned."))

    for day in days[:-1]:  # every date except the last is a night
        capacity = sum(resources.room_capacity.get(r.room_type_id, 0) for r in resources.rooms if r.night == day.date)
        if capacity < pax:
            violations.append(Violation(code="ROOM_CAPACITY",
                                        message=f"Night {day.date} sleeps {capacity} but pax is {pax}."))

    if total_usd > budget_usd:
        violations.append(Violation(code=OVER_BUDGET,
                                    message=f"Total USD {total_usd} is over the budget of USD {budget_usd}."))
    return violations
