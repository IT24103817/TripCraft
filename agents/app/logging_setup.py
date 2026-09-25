"""Structured JSON logging. Only ids, node names, durations and statuses are logged — never prompts."""
import json
import logging

_EXTRA_FIELDS = ("workflow_id", "node", "duration_ms", "status")


class JsonFormatter(logging.Formatter):
    def format(self, record: logging.LogRecord) -> str:
        entry = {"level": record.levelname, "logger": record.name, "message": record.getMessage()}
        for field in _EXTRA_FIELDS:
            if hasattr(record, field):
                entry[field] = getattr(record, field)
        return json.dumps(entry)


def configure_logging() -> None:
    handler = logging.StreamHandler()
    handler.setFormatter(JsonFormatter())
    root = logging.getLogger()
    root.handlers = [handler]
    root.setLevel(logging.INFO)
