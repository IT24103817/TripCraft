"""Exceptions shared by the tools, the LLM helper and the graph."""


class ToolError(Exception):
    """A tool could not produce a result (API down, non-2xx reply, bad input)."""


class ToolNotAllowed(Exception):
    """An agent tried to use a tool that is not on its allow-list."""


class AgentOutputError(Exception):
    """The LLM did not return valid JSON for the schema after all retries."""
