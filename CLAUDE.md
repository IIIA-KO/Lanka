# CLAUDE.md - Lanka Project Context

## Project Overview

**Lanka** is a diploma project exploring modern .NET architecture patterns through building a social media campaign management platform. The goal is learning, not shipping a product.

### What This Project Is About
- Learning modular monolith architecture
- Applying Domain-Driven Design (DDD) patterns
- Implementing CQRS with MediatR
- Understanding event-driven architecture
- Integrating with external APIs (Instagram)

## Technology Stack

- **.NET 10** with **C# 14**
- **ASP.NET Core** — Web API
- **Entity Framework Core 9** — ORM
- **PostgreSQL 15+** — Primary database
- **MongoDB 7+** — Analytics document storage
- **Redis 7+** — Caching
- **RabbitMQ** — Message broker
- **Keycloak** — Identity provider
- **Docker** — Development infrastructure

## Project Structure

```
Lanka/
├── src/
│   ├── Api/
│   │   ├── Lanka.Api/                    # Main API host
│   │   └── Lanka.Gateway/                # YARP reverse proxy
│   ├── Common/                           # Shared components
│   │   ├── Lanka.Common.Application/
│   │   ├── Lanka.Common.Contracts/
│   │   ├── Lanka.Common.Domain/
│   │   ├── Lanka.Common.Infrastructure/
│   │   └── Lanka.Common.Presentation/
│   └── Modules/                          # Business modules
│       ├── Analytics/                    # Instagram data
│       ├── Campaigns/                    # Campaign management
│       ├── Matching/                     # Search (Elasticsearch)
│       └── Users/                        # Identity & profiles
├── docs/                                 # Documentation
└── docker-compose.yml                    # Development environment
```

### Module Architecture

Each module follows Clean Architecture:
- **Domain** — Entities, value objects, domain events
- **Application** — Commands, queries, handlers
- **Infrastructure** — Repositories, external services
- **Presentation** — API endpoints

## Quick Start

```bash
# Start infrastructure
docker compose up -d

# Run the API (migrations apply automatically)
cd src/Api/Lanka.Api && dotnet run

# Access points
# API: http://localhost:4307
# Gateway: http://localhost:4308
# Health: http://localhost:4307/healthz
# Seq logs: http://localhost:8081
```

## Development Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Add migration (from module Infrastructure directory)
dotnet ef migrations add <Name>

# View logs
docker compose logs -f <service>
```

## Module Status

| Module | Status | Notes |
|--------|--------|-------|
| Users | Complete | Authentication, OAuth2, Instagram linking |
| Analytics | In Progress | MongoDB, Instagram API integration |
| Campaigns | In Progress | Complex domain modeling |
| Matching | Basic | Elasticsearch search |
| Gateway | Complete | YARP configuration |

## Key Patterns

- **Modular Monolith** — Modules communicate via events, not direct calls
- **CQRS** — Commands and queries separated with MediatR
- **Domain Events** — Internal module communication
- **Integration Events** — Cross-module communication via RabbitMQ
- **Outbox/Inbox** — Reliable message delivery
- **Saga** — Orchestrated multi-step workflows

## Database

- PostgreSQL for relational data (one schema per module)
- MongoDB for analytics time-series data
- Redis for caching

Migrations run automatically on startup.

## Configuration

Main settings in `appsettings.json`:
- `ConnectionStrings.Database` — PostgreSQL
- `ConnectionStrings.Mongo` — MongoDB
- `ConnectionStrings.Cache` — Redis
- `ConnectionStrings.Queue` — RabbitMQ
- `Authentication` — Keycloak OIDC settings

## Documentation

- [Quick Start](docs/development/quick-start.md)
- [Architecture](docs/architecture/README.md)
- [Architecture Decisions](docs/architecure-decision-log/README.md)
- [Modules](docs/modules/README.md)
- [Catalog of Terms](docs/catalog-of-terms/README.md)

## Notes for Development

- Start with Users module as reference — it's the most complete
- Each module has its own database schema
- Use integration events for cross-module communication
- Result pattern for error handling (no exceptions for business errors)
- Bogus for fake data generation in development
