from app.nodes.itinerary import itinerary_node
from app.nodes.planner import planner_node
from app.state import FAILED_SAFELY
from tests.conftest import apply, load_fixture


async def planned(demo_state):
    return apply(demo_state, await planner_node(demo_state))


async def test_itinerary_golden_days_follow_rules(fake_llm, api, demo_state):
    state = await planned(demo_state)

    update = await itinerary_node(state)

    days = update["days"]
    assert [d["day"] for d in days] == [1, 2, 3, 4, 5]
    assert [d["city"] for d in days] == ["Kandy", "Kandy", "Ella", "Ella", "Ella"]
    assert days[0]["date"] == "2026-10-10" and days[4]["date"] == "2026-10-14"
    assert all(1 <= len(d["stops"]) <= 3 for d in days)
    assert days[2]["transport"] == "train"
    assert days[2]["transfer_km"] == 140 and days[2]["driving_minutes"] == 0  # train day: no driving
    assert days[0]["stops"][0] == {"attraction_id": "a-temple", "name": "Temple of the Tooth", "entry_fee_lkr": 2000}
    assert all(d["weather"] == "light rain" for d in days)
    tools = [c["tool"] for c in update["steps"][0]["tool_calls"]]
    assert tools.count("get_attractions") == 2 and tools.count("get_distance") == 1 and tools.count("get_weather") == 5


async def test_itinerary_repairs_four_stops_then_succeeds(fake_llm, api, demo_state):
    state = await planned(demo_state)
    bad = load_fixture("itinerary")
    bad["days"][0]["stops"] = ["a-temple", "a-kandy-lake", "a-peradeniya", "a-bahirawakanda"]
    fake_llm.queue("itinerary", bad, load_fixture("itinerary"))

    update = await itinerary_node(state)

    assert update["steps"][0]["retries"] == 1
    assert "day 1: has 4 stops" in fake_llm.calls_for("itinerary")[1][-1].content
    assert all(len(d["stops"]) <= 3 for d in update["days"])


async def test_itinerary_long_road_transfer_fails_safely(fake_llm, api, demo_state):
    state = await planned(demo_state)
    bad = load_fixture("itinerary")
    bad["days"][2]["transport"] = "road"  # Kandy -> Ella by road is 270 min, over the 240 min limit
    fake_llm.queue("itinerary", bad)

    update = await itinerary_node(state)

    assert update["status"] == FAILED_SAFELY
    assert "270 min" in update["error_summary"]
    assert "days" not in update
