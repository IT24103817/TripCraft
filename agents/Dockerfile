# TripCraft agent service (FastAPI + LangGraph). Build context is agents/:  docker build -t tripcraft-agents agents
FROM python:3.11-slim

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PORT=8001

WORKDIR /app
COPY requirements.txt .
RUN pip install -r requirements.txt

COPY app/ app/

# Non-root user.
RUN useradd --create-home --uid 10001 agent
USER agent

EXPOSE 8001
# PORT is 8001 locally; Render sets its own PORT.
CMD ["sh", "-c", "uvicorn app.main:app --host 0.0.0.0 --port ${PORT}"]
