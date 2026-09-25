from app.tools.models import AgentInfo

_AGENTS = [
    AgentInfo(name="itinerary", responsibility="Pick attractions per day, travel order and road vs train."),
    AgentInfo(name="resources", responsibility="Propose an available guide, vehicle and rooms. Never holds."),
    AgentInfo(name="validation", responsibility="Calculate the quotation and check business rules and budget."),
]


def list_agents() -> list[AgentInfo]:
    """Local tool: the agents the planner may delegate to."""
    return list(_AGENTS)
