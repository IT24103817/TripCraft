from typing import Any

from pydantic import TypeAdapter, ValidationError

from app.schemas import ItineraryDay, ResourceSelection
from app.tools.models import SchemaCheck

_DAYS = TypeAdapter(list[ItineraryDay])


def validate_schema(days: list[dict[str, Any]], resources: dict[str, Any]) -> SchemaCheck:
    """Local tool: re-checks the proposal in the shared state against the Pydantic contracts."""
    errors: list[str] = []
    for label, check in (("days", lambda: _DAYS.validate_python(days)),
                         ("resources", lambda: ResourceSelection.model_validate(resources))):
        try:
            check()
        except ValidationError as ex:
            errors += [f"{label}.{'.'.join(str(p) for p in e['loc'])}: {e['msg']}" for e in ex.errors()]
    return SchemaCheck(ok=not errors, errors=errors)
