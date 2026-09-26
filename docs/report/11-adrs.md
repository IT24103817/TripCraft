# 11. Architecture Decision Records

Six decisions from PLAN.md section 13, one page each (Context → Options considered → Decision → Consequences →
where it shows in the code). Each author defends their own at the viva.

| # | Decision | Outcome | Author |
|---|----------|---------|--------|
| ADR-001 | React state management | Zustand + TanStack Query | Student C |
| ADR-002 | Flutter state management | Riverpod 3 | Student A |
| ADR-003 | Agentic AI framework and orchestration | LangGraph in FastAPI; rules in code and C# | Student A |
| ADR-004 | Agent workflow state schema | Hybrid tables + jsonb | Student C |
| ADR-005 | Cloud deployment platform | Neon + Render + Vercel | Student B |
| ADR-006 | LLM provider | Ollama llama3.1:8b, Groq fallback | Student B |

The six ADRs follow (from `docs/adr/`; `build.sh` inserts them here).
