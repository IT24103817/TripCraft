# ER diagram

Generated from the EF Core model (`backend/src/TripCraft.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
and the configurations in `Trips/`, `Identity/`, `Persistence/Auditing/` and `Workflows/`). Every table has `id uuid` (PK),
`created_at` and `updated_at timestamptz`; money is `numeric(12,2)`; column names are snake_case.

Tables of Resource Management (guides, guide_languages, vehicles, hotels, room_types, resource_holds, rate card)
and of Quotations (quotations, quotation_lines, approval_decisions) are designed in PLAN.md section 4 but are
**not in the database yet** — they belong to Students B and C.

```mermaid
erDiagram
    users ||--o| tourists : "has profile"
    tourists ||--o{ trip_requests : submits
    trip_requests ||--o| itineraries : "has (unique trip_request_id)"
    itineraries ||--o{ itinerary_days : contains
    itinerary_days ||--o{ itinerary_stops : contains
    attractions ||--o{ itinerary_stops : "visited in"
    trip_requests ||--o{ agent_workflows : "planned by"
    agent_workflows ||--o{ agent_steps : records

    users {
        uuid id PK
        varchar(256) email UK
        text password_hash
        varchar(32) role "Tourist | Guide | OperationsManager | Admin"
        varchar(200) full_name
        boolean is_active
    }
    tourists {
        uuid id PK
        uuid user_id FK,UK
        varchar(100) nationality
        varchar(20) passport_number_masked "last 4 only"
        varchar(500) passport_photo_url "private storage key"
    }
    trip_requests {
        uuid id PK
        uuid tourist_id FK
        text objective
        date start_date "check end_date >= start_date"
        date end_date
        int pax "check pax > 0"
        numeric budget_usd "numeric(12,2)"
        jsonb preferences
        varchar(32) status "index (status, start_date)"
    }
    attractions {
        uuid id PK
        varchar(200) name
        varchar(100) city "indexed"
        varchar(50) category
        int duration_minutes
        numeric entry_fee_lkr "numeric(12,2)"
        double latitude
        double longitude
        boolean is_deleted "soft delete"
    }
    itineraries {
        uuid id PK
        uuid trip_request_id FK,UK
        int version
        varchar(16) generated_by "Agent | Manual"
    }
    itinerary_days {
        uuid id PK
        uuid itinerary_id FK "unique (itinerary_id, day_number)"
        int day_number
        varchar(100) city
        uuid hotel_id "FK to hotels once Student B merges"
        text notes
    }
    itinerary_stops {
        uuid id PK
        uuid itinerary_day_id FK "unique (itinerary_day_id, sequence)"
        uuid attraction_id FK
        int sequence
        time arrival_time
    }
    agent_workflows {
        uuid id PK
        uuid trip_request_id FK "indexed"
        text objective
        jsonb plan
        varchar(32) status "index (status, started_at)"
        varchar(100) current_step
        timestamptz started_at
        timestamptz finished_at
        jsonb final_outcome "proposal, then decision"
        jsonb validation_result "C# ProposalValidator"
        text error_summary
    }
    agent_steps {
        uuid id PK
        uuid workflow_id FK "unique (workflow_id, step_no)"
        int step_no
        varchar(32) agent_name
        varchar(200) tool_name
        jsonb input_summary
        jsonb output_summary
        jsonb validation_result
        int duration_ms
        int retries
        varchar(32) status
    }
    audit_logs {
        uuid id PK
        uuid actor_id "null for system actions"
        varchar(100) action
        varchar(100) entity
        uuid entity_id
        jsonb before
        jsonb after
        timestamptz at
    }
    city_distances {
        uuid id PK
        varchar(60) from_city "unique (from_city, to_city)"
        varchar(60) to_city
        numeric distance_km "check > 0"
        int duration_minutes "check > 0"
    }
```

**Delete behaviour:** itinerary days and stops cascade with their parent; agent steps cascade with their
workflow; every other foreign key is `RESTRICT`. `audit_logs` has no foreign keys on purpose (it must outlive
the rows it describes).
