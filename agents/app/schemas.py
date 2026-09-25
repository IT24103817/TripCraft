"""
Input and output contracts for the four agents (PLAN.md section 5), the workflow request
from the API, and the step report / proposal posted back to it.
"""
import json
from datetime import date, datetime
from typing import Any, Literal
from uuid import UUID

from pydantic import AliasChoices, BaseModel, Field, field_validator, model_validator

from app.tools.models import ApiModel, Money, RateCard

AgentName = Literal["planner", "itinerary", "resources", "validation"]


# ---------- Workflow request from the ASP.NET Core API ----------

class WorkflowRequest(ApiModel):
    workflow_id: UUID
    objective: str = Field(min_length=1, max_length=2000)
    start_date: date
    end_date: date
    pax: int = Field(gt=0, le=50)
    budget_usd: Money = Field(gt=0)
    preferences: dict[str, Any] = Field(
        default_factory=dict,
        validation_alias=AliasChoices("preferences", "preferencesJson", "preferences_json"))
    manager_comment: str | None = Field(default=None, max_length=1000)
    # Where to POST step reports and the proposal. Defaults to API_BASE_URL when missing.
    callback_base_url: str | None = None

    @field_validator("preferences", mode="before")
    @classmethod
    def parse_preferences_json(cls, value: Any) -> Any:
        # The C# client sends preferences as a JSON string (PreferencesJson).
        if isinstance(value, str):
            return json.loads(value) if value.strip() else {}
        return value

    @model_validator(mode="after")
    def check_dates(self) -> "WorkflowRequest":
        if self.end_date < self.start_date:
            raise ValueError("end_date must be on or after start_date")
        return self


class ReplanRequest(WorkflowRequest):
    manager_comment: str = Field(min_length=1, max_length=1000)


# ---------- Planner / Coordinator ----------

class PlannerInput(BaseModel):
    objective: str
    start_date: date
    end_date: date
    pax: int
    budget_usd: Money
    preferences: dict[str, Any]


class PlanStep(BaseModel):
    step: int = Field(ge=1)
    agent: Literal["itinerary", "resources", "validation"]
    task: str = Field(min_length=1, max_length=200)
    depends_on: list[int] = Field(default_factory=list)


class PlannerConstraints(BaseModel):
    cities: list[str] = Field(min_length=1, max_length=10)
    guide_language: str = Field(default="en", pattern=r"^[a-z]{2}$")
    transport_preference: Literal["train", "road", "any"] = "any"
    hotel_tier: Literal["budget", "standard", "premium"] = "standard"
    max_stops_per_day: int = Field(default=3, ge=1, le=3)


class PlannerOutput(BaseModel):
    plan: list[PlanStep] = Field(min_length=3, max_length=10)
    constraints: PlannerConstraints

    @model_validator(mode="after")
    def check_plan_order(self) -> "PlannerOutput":
        numbers = [s.step for s in self.plan]
        if len(numbers) != len(set(numbers)):
            raise ValueError("step numbers must be unique")
        for s in self.plan:
            if any(d >= s.step or d not in numbers for d in s.depends_on):
                raise ValueError(f"step {s.step} may only depend on earlier existing steps")
        if {s.agent for s in self.plan} != {"itinerary", "resources", "validation"}:
            raise ValueError("plan must delegate to itinerary, resources and validation")
        if max(self.plan, key=lambda s: s.step).agent != "validation":
            raise ValueError("the last step must be validation")
        return self


# ---------- Itinerary Analysis ----------

class ItineraryInput(BaseModel):
    plan: list[PlanStep]
    cities: list[str]
    dates: list[date]
    pax: int
    preferences: dict[str, Any]


class ItineraryDayDraft(BaseModel):
    """One day as the LLM proposes it. `stops` are attraction ids from get_attractions."""
    day: int = Field(ge=1)
    city: str
    stops: list[str]
    transport: Literal["road", "train"]


class ItineraryOutput(BaseModel):
    days: list[ItineraryDayDraft] = Field(min_length=1)


class Stop(BaseModel):
    attraction_id: str
    name: str
    entry_fee_lkr: Money


class ItineraryDay(BaseModel):
    """One day as stored in the workflow state. Dates, distances and weather are filled by code."""
    day: int
    date: date
    city: str
    stops: list[Stop]
    transport: Literal["road", "train"]
    transfer_km: Money = 0
    driving_minutes: int = 0
    weather: str | None = None


# ---------- Resource & Action ----------

class ResourceInput(BaseModel):
    days: list[ItineraryDay]
    pax: int
    language: str
    dates: list[date]


class RoomNight(BaseModel):
    """One room for one night."""
    hotel_id: str
    room_type_id: str
    night: date


class ResourceActionOutput(BaseModel):
    guide_id: str | None = None
    vehicle_id: str | None = None
    rooms: list[RoomNight] = Field(default_factory=list)
    gaps: list[str] = Field(default_factory=list)


class ResourceSelection(ResourceActionOutput):
    """The proposal plus the facts validation needs, copied by code from the availability tools."""
    guide_languages: list[str] = Field(default_factory=list)
    vehicle_seats: int | None = None
    room_capacity: dict[str, int] = Field(default_factory=dict)
    rate_card: RateCard


# ---------- Validation & Safety ----------

class Violation(BaseModel):
    code: str
    message: str


class QuotationLine(BaseModel):
    line_type: Literal["guide", "vehicle", "room", "entry"]
    description: str
    qty: Money
    unit_lkr: Money
    amount_lkr: Money


class Quotation(BaseModel):
    lines: list[QuotationLine]
    subtotal_lkr: Money
    margin_pct: Money
    margin_lkr: Money
    total_lkr: Money
    fx_rate: Money
    fx_as_of: datetime
    fx_stale: bool
    total_usd: Money


class ValidationInput(BaseModel):
    days: list[ItineraryDay]
    resources: ResourceSelection
    quotation_draft: Quotation
    budget_usd: Money


class ValidationSafetyOutput(BaseModel):
    valid: bool
    violations: list[Violation] = Field(default_factory=list)
    quotation_final: Quotation | None = None


# ---------- Reports sent back to the API ----------

class ToolCall(BaseModel):
    tool: str
    args: dict[str, Any]
    ok: bool
    duration_ms: int
    error: str | None = None


class StepReport(BaseModel):
    agent_name: AgentName
    tool_calls: list[ToolCall]
    input_summary: dict[str, Any]
    output_summary: dict[str, Any]
    validation_result: dict[str, Any]
    duration_ms: int
    retries: int
    status: Literal["Succeeded", "Failed"]


class Proposal(BaseModel):
    plan: dict[str, Any] | None
    days: list[dict[str, Any]]
    resources: dict[str, Any] | None
    quotation: dict[str, Any] | None
    violations: list[dict[str, Any]]
    status: str
    replans: int
    error_summary: str | None = None
