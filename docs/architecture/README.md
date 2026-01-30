# Lanka Architecture Documentation

<div align="center">

*Exploring how modular monolith architecture works in practice*

![Architecture Overview](../images/overall-architecture.png)

</div>

---

## About This Section

This is where I document the architectural patterns I'm learning and applying in Lanka. If you're also exploring .NET architecture, I hope these docs help explain not just *what* patterns are used, but *why* they make sense (or sometimes don't).

---

## Architecture Guide Index

### Core Concepts
| Document | Description | What I Learned |
|----------|-------------|----------------|
| [Modular Monolith](../architecure-decision-log/003-modular-monolith-architecture.md) | Architecture decision and rationale | Module boundaries, why not microservices |
| [Domain-Driven Design](../architecure-decision-log/004-adoption-of-ddd.md) | DDD adoption decision | Aggregates, entities, value objects |
| [Event-Driven Architecture](../architecure-decision-log/008-event-driven-architecture.md) | Event-driven patterns | Domain events, integration events |
| [CQRS & MediatR](../architecure-decision-log/005-cqrs-implementation.md) | CQRS implementation | Commands, queries, handlers |

### Implementation Patterns
| Document | Description | Status |
|----------|-------------|--------|
| Data Architecture | Database design and data flow | Planned |
| API Design | RESTful API patterns | Planned |
| Security Architecture | Authentication and authorization | Partial |
| Performance Patterns | Caching, optimization | Planned |

### Infrastructure
| Document | Description | Where to Look |
|----------|-------------|---------------|
| Containerization | Docker setup | See `docker-compose.yml` |
| Observability | Monitoring, logging, tracing | [Tools/Telemetry](../tools/telemetry/) |
| Message Bus | Async messaging with RabbitMQ | [Tools/Messaging](../tools/messaging/) |
| Deployment | Production strategies | Not implemented yet |

---

## Architectural Principles

### 1. Modular Design

```
┌─────────────────────────────────────────────────────────────┐
│                    Lanka.Api (Host)                         │
├─────────────────────────────────────────────────────────────┤
│  Users Module      │  Analytics Module  │ Campaigns Module  │
│  ┌───────────────┐ │  ┌───────────────┐ │ ┌───────────────┐ │
│  │ Application   │ │  │ Application   │ │ │ Application   │ │
│  │ Domain        │ │  │ Domain        │ │ │ Domain        │ │
│  │ Infrastructure│ │  │ Infrastructure│ │ │ Infrastructure│ │
│  └───────────────┘ │  └───────────────┘ │ └───────────────┘ │
├─────────────────────────────────────────────────────────────┤
│                     Common Infrastructure                   │
└─────────────────────────────────────────────────────────────┘
```

**Why this matters:**

- **Clear Separation** — Each module owns its data and business logic
- **Independent Evolution** — Modules can change at different speeds
- **Easier Testing** — Isolated modules are simpler to test
- **Future Flexibility** — Modules can be extracted to microservices if needed

### 2. Command Query Responsibility Segregation (CQRS)

![CQRS](../images/cqrs.jpg)

Separating reads from writes allows optimizing each path independently. In Lanka, commands go through validation pipelines while queries can use optimized read models.

### 3. Event-Driven Communication

![Event Bus](../images/event-bus.jpg)

Modules communicate through events rather than direct calls. This keeps them loosely coupled — the Users module doesn't need to know what Analytics does with user data.

### 4. Clean Architecture Layers

```
┌──────────────────────────────────────────┐
│           Presentation Layer             │
│          (Endpoints, DTOs)               │
├──────────────────────────────────────────┤
│           Application Layer              │
│     (Use Cases, Command/Query)           │
├──────────────────────────────────────────┤
│             Domain Layer                 │
│      (Entities, Business Rules)          │
├──────────────────────────────────────────┤
│          Infrastructure Layer            │
│     (Database, External Services)        │
└──────────────────────────────────────────┘
```

Dependencies point inward — outer layers depend on inner layers, never the reverse.

---

## Module Overview

### Users Module

**What it handles:**
- Authentication & Authorization via Keycloak
- User profile management
- Instagram account linking (via OAuth2)
- User activity tracking

### Analytics Module

**What it handles:**
- Instagram analytics data collection
- Audience demographics processing
- Performance metrics storage (MongoDB)
- Mock services for development without real Instagram access

### Campaigns Module

**What it handles:**
- Campaign creation and management
- Blogger profiles and applications
- Offer negotiations and contracts
- Campaign tracking

---

## Data Flow

### Cross-Module Communication

![Data-Flow](../images/data-flow.png)

### Request Processing Pipeline

![Request-Processing](../images/request-processing.png)

---

## Environment & Settings

- **API** (`Lanka.Api`) listens on `http://localhost:4307` in dev; health checks at `/healthz`
- **Gateway** (`Lanka.Gateway`) listens on `https://localhost:4308` with YARP reverse proxy
- **Logging** uses Serilog → Seq sink; UI at `http://localhost:8081`
- **Migrations** are applied automatically on startup (no manual step needed)
- **Keycloak** realm imported from `test/Lanka.IntegrationTests/lanka-realm-export.json`
- **Data stores**: PostgreSQL (5432), MongoDB (27017), Redis (6379), RabbitMQ (5672/15672)

---

## Quality Attributes

These are the non-functional requirements I'm trying to address:

### Scalability
- Stateless design for horizontal scaling
- Database-per-module prevents bottlenecks
- Asynchronous processing for heavy operations
- Caching for frequently accessed data

### Maintainability
- Clear module boundaries reduce cognitive load
- Consistent patterns across all modules
- Architecture tests enforce boundaries

### Reliability
- Circuit breaker patterns for external services
- Retry policies with exponential backoff
- Health checks for all critical components

### Security
- OAuth2/JWT authentication via Keycloak
- Role-based access control
- Input validation at all entry points

---

## What's Next?

Dive deeper into specific patterns:

1. **Modular Monolith** — see [ADR 003](../architecure-decision-log/003-modular-monolith-architecture.md)
2. **Domain-Driven Design** — see [ADR 004](../architecure-decision-log/004-adoption-of-ddd.md)
3. **Event-Driven Patterns** — see [ADR 008](../architecure-decision-log/008-event-driven-architecture.md)
4. **Module Details** — see [Modules Documentation](../modules/README.md)

---

<div align="center">

*"Good architecture makes the system easy to understand, develop, maintain, and deploy."*
— Uncle Bob Martin

</div>
