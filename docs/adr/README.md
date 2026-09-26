# Architecture Decision Records

One page per decision (PLAN.md section 13): Context → Options considered → Decision → Consequences → where it
shows in the code. Each author must be able to defend theirs at the viva.

| # | Decision | Outcome | Author |
|---|----------|---------|--------|
| [ADR-001](ADR-001-react-state-management.md) | React state management | Zustand (auth/UI) + TanStack Query (server state) | Student C |
| [ADR-002](ADR-002-flutter-state-management.md) | Flutter state management | Riverpod 3 with code generation | Student A |
| [ADR-003](ADR-003-agentic-ai-framework.md) | Agentic AI framework and orchestration | LangGraph in a FastAPI service; deterministic rules in code and C# | Student A |
| [ADR-004](ADR-004-agent-workflow-state-schema.md) | Agent workflow state schema | Hybrid: `agent_workflows` + `agent_steps` with jsonb summaries | Student C |
| [ADR-005](ADR-005-cloud-deployment-platform.md) | Cloud deployment platform | Neon + Render (Docker) + Vercel, APK on GitHub Releases | Student B |
| [ADR-006](ADR-006-llm-provider.md) | LLM provider | Ollama llama3.1:8b, Groq as fallback | Student B |

ADR-001, 004, 005 and 006 were drafted during the build for their authors; each author reviews, edits and
signs off their own.
