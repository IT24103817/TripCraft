from datetime import date
from decimal import Decimal

from app.nodes.itinerary import fewer_paid_entries
from app.nodes.resources import cheapest_vehicle
from app.schemas import ItineraryDay, Stop
from app.tools.models import Attraction, RateCard, VehicleOption


def attraction(id_: str, fee: int) -> Attraction:
    return Attraction.model_validate({"id": id_, "name": id_, "city": "Kandy", "category": "Sight",
                                      "durationMinutes": 60, "entryFeeLkr": fee, "latitude": 7.29, "longitude": 80.63})


def test_paid_stops_are_swapped_for_unused_free_ones_of_the_same_city():
    kandy = [attraction("temple", 2000), attraction("lake", 0), attraction("buddha", 0), attraction("gardens", 3000)]
    day = ItineraryDay(day=1, date=date(2026, 11, 1), city="Kandy", transport="road", transfer_km=0, driving_minutes=0,
                       stops=[Stop(attraction_id="temple", name="temple", entry_fee_lkr=2000),
                              Stop(attraction_id="lake", name="lake", entry_fee_lkr=0),
                              Stop(attraction_id="gardens", name="gardens", entry_fee_lkr=3000)])

    [result] = fewer_paid_entries([day], {"kandy": kandy})

    # One free alternative left ("buddha"): the first paid stop is swapped, the second stays (nothing free remains).
    assert [s.attraction_id for s in result.stops] == ["buddha", "lake", "gardens"]


def test_the_cheapest_vehicle_by_km_rate_is_chosen():
    van = VehicleOption.model_validate({"id": "van", "registrationNo": "CAB-1", "type": "Van", "seats": 6})
    coach = VehicleOption.model_validate({"id": "coach", "registrationNo": "NC-1", "type": "Coach", "seats": 15})
    card = RateCard.model_validate({"marginPct": 15, "guideDayRates": {}, "roomNightRates": {},
                                    "vehicleKmRates": {"van": 120, "coach": 90}})

    assert cheapest_vehicle([van, coach], card).id == "coach"
    assert cheapest_vehicle([], card) is None
    assert card.vehicle_km_rates["coach"] == Decimal("90")
