# Agent graph (LangGraph)

`agents/app/graph.py`. Every node is wrapped by `guarded()`: a `NODE_TIMEOUT_SECONDS` timeout (30 s default),
a last-resort error guard, a structured log line and a `POST …/steps` report to the API.

```mermaid
flowchart TD
    START([POST /run-workflow or /replan]) --> P

    P["Planner / Coordinator<br/>tools: parse_dates, list_agents<br/>→ plan + constraints"]
    I["Itinerary Analysis<br/>tools: get_attractions, get_distance, get_weather<br/>rules: 1–3 stops/day, ≤ 240 min road driving"]
    R["Resource & Action<br/>tools: check_guide/vehicle/room_availability, get_rate_card<br/>proposes, never holds; lists gaps"]
    V["Validation & Safety<br/>tools: validate_schema, get_fx_rate, calculate_quotation, check_business_rules<br/>LLM may add concerns, never remove violations"]
    RP["prepare_replan<br/>replans + 1"]
    FAIL([failed safely<br/>error_summary → trip NeedsOperator])
    PA([valid → API sends the quotation])
    RR([violations remain → API: best price if only over budget, else NeedsOperator])
    POST[["POST …/proposal<br/>{plan, days, resources, quotation, violations, status, replans}"]]

    P -- ok --> I
    P -- "tool error / invalid JSON after 2 repairs / timeout" --> FAIL
    I -- ok --> R
    I -- failure --> FAIL
    R -- ok --> V
    R -- failure --> FAIL
    V -- "no violations" --> PA
    V -- "only OVER_BUDGET and replans < 3" --> RP
    RP -- "violations as revision context;<br/>hotel tier forced to budget" --> P
    V -- "other violations, or 3 re-plans used" --> RR
    V -- failure --> FAIL

    PA --> POST
    RR --> POST
    FAIL --> POST
```

| Guard | Where |
|-------|-------|
| Tool allow-list (`ToolNotAllowed`) | `agents/app/tools/registry.py` |
| JSON output + repair loop (`MAX_RETRIES` = 2) | `agents/app/llm.py` (`call_json`) |
| Prompt-injection: inputs as escaped JSON inside one `<DATA>` block | `agents/app/nodes/common.py` (`wrap_data`, `DATA_RULES`) |
| Rules enforced in code, not only prompts | `agents/app/nodes/itinerary.py` (`check_days`), `resources.py` (`check_selection`, `missing_resource_gaps`), `validation.py` (`merge_verdict`), `tools/check_business_rules.py` |
| Re-plan limit (`MAX_REPLANS` = 3) | `agents/app/graph.py` (`after_validation`) |
