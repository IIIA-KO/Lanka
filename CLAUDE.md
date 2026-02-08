# CLAUDE.md - Lanka Project Context

## Project Overview

**Lanka** is a diploma project exploring modern .NET architecture patterns through building a social media campaign management platform. The goal is learning, not shipping a product.

### What This Project Is About
- Learning modular monolith architecture
- Applying Domain-Driven Design (DDD) patterns
- Implementing CQRS with MediatR
- Understanding event-driven architecture
- Integrating with external APIs (Instagram)

---

## Current State

This is a learning project. Some parts work well, others are incomplete or simplified.

| Component | Status | Honest Assessment |
|-----------|--------|-------------------|
| Users Module | Working | Authentication, profiles, Instagram linking all functional |
| Analytics Module | Partial | Data fetching works; MongoDB caching implemented but TTL strategy needs tuning |
| Campaigns Module | In Progress | Domain model exists, but limited API coverage |
| Matching Module | Basic | Elasticsearch indexing works; search features need refinement |
| Gateway | Working | YARP routing configured correctly |
| Frontend | Partial | Core flows work; some features incomplete, low test coverage |
| Test Coverage | Low | Unit tests exist for domain logic; integration tests sparse |

---

## Technology Stack

- **.NET 10** with **C# 14**
- **ASP.NET Core** — Web API
- **Entity Framework Core 9** — ORM
- **PostgreSQL 15+** — Primary database
- **MongoDB 7+** — Analytics document storage
- **Redis 7+** — Caching
- **RabbitMQ** — Message broker
- **Keycloak** — Identity provider
- **Elasticsearch 9** — Full-text search
- **.NET Aspire 13** — Local development orchestration
- **Docker** — Container runtime (managed by Aspire)
- **Angular 20** — Frontend SPA

## Project Structure

```
Lanka/
├── src/
│   ├── Api/
│   │   ├── Lanka.AppHost/                # Aspire orchestration (containers + projects)
│   │   ├── Lanka.ServiceDefaults/        # Shared OTel, health checks, resilience
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
├── client/
│   └── lanka-client/                     # Angular frontend
└── docs/                                 # Documentation
```

### Module Architecture

Each module follows Clean Architecture:
- **Domain** — Entities, value objects, domain events
- **Application** — Commands, queries, handlers
- **Infrastructure** — Repositories, external services
- **Presentation** — API endpoints

---

## Quick Start

```bash
# Install Aspire workload (one-time)
dotnet workload install aspire

# Start everything (infrastructure + API + Gateway)
dotnet run --project src/Api/Lanka.AppHost

# Run the frontend (separate terminal)
cd client/lanka-client && npm install && npm start

# Access points
# Aspire Dashboard: (URL shown in console output)
# API: http://localhost:4307
# Gateway: https://localhost:4308
# Frontend: https://localhost:4200
# Health: http://localhost:4307/healthz
```

## Development Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Start all services via Aspire
dotnet run --project src/Api/Lanka.AppHost

# Add migration (from module Infrastructure directory)
dotnet ef migrations add <Name>

# View logs — use the Aspire Dashboard (URL in console output)

# Frontend lint
cd client/lanka-client && npm run lint
```

---

## Key Patterns

- **Modular Monolith** — Modules communicate via events, not direct calls
- **CQRS** — Commands and queries separated with MediatR
- **Domain Events** — Internal module communication
- **Integration Events** — Cross-module communication via RabbitMQ
- **Outbox/Inbox** — Reliable message delivery
- **Saga** — Orchestrated multi-step workflows (Instagram linking)
- **Result Pattern** — Error handling without exceptions for business errors

---

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

---

## Documentation

- [Quick Start](docs/development/quick-start.md)
- [Architecture](docs/architecture/README.md)
- [Architecture Decisions](docs/architecure-decision-log/README.md)
- [Modules](docs/modules/README.md)
- [Frontend](docs/frontend/README.md)
- [Catalog of Terms](docs/catalog-of-terms/README.md)

---

## Notes for Development

- Start with Users module as reference — it's the most complete
- Each module has its own database schema
- Use integration events for cross-module communication
- Result pattern for error handling (no exceptions for business errors)
- Bogus for fake data generation in development

---

## Instagram Service Factory Pattern (Analytics Module)

In Development environment, the Analytics module uses a **Service Factory Pattern** to dispatch real vs mock Instagram API services based on user email.

### How It Works

```
IInstagramServiceFactory<TService>
├── Development Environment
│   ├── User email in AllowedUserEmails → Real Instagram API
│   └── User email NOT in AllowedUserEmails → Mock service (fake data)
└── Production Environment → Always Real Instagram API
```

### Key Components

- **`IInstagramServiceFactory<TService>`** — Factory interface for runtime service resolution
- **`InstagramServiceFactory<TService>`** — Implementation that checks `AllowedUserEmails` config
- **`IInstagramUserContext`** — Provides current user's email (from HTTP context or InstagramAccount entity)
- **`FakeInstagramDataGenerator`** — Shared utility for generating fake Instagram data (used by both mock services and database seeders)

### Configuration

```json
{
  "Analytics": {
    "Instagram": {
      "Development": {
        "AllowedUserEmails": ["real.user@example.com"]
      }
    }
  }
}
```

### Background Jobs

Background jobs (like `UpdateInstagramAccountsJob`) don't have HTTP context. They:
1. Inject `IInstagramServiceFactory<IInstagramAccountsService>`
2. Get user's email from `InstagramAccount.Email` property
3. Call `factory.GetService(email)` per account to get the appropriate service

### Mock Services

All mock services use `FakeInstagramDataGenerator` for consistent fake data:
- `MockFacebookService`
- `MockInstagramAccountsService`
- `MockInstagramStatisticsService`
- `MockInstagramAudienceService`
- `MockInstagramPostService`

---

## Module Business Rules

### Users Module

**Core Responsibilities:**
- Authentication via Keycloak (JWT tokens)
- User profiles (FirstName, LastName, Email, BirthDate)
- Instagram account linking via OAuth2 saga

**Instagram Linking Flow:**
1. User initiates linking → `LinkInstagramAccountCommand`
2. Saga starts → `InstagramAccountLinkingStartedIntegrationEvent` to Analytics
3. Analytics fetches Instagram data → `InstagramAccountDataFetchedIntegrationEvent` back
4. Saga completes → User's `InstagramAccountLinkedOnUtc` is set

**Key Entities:** `User`, `Role` (Member)

### Analytics Module

**Core Responsibilities:**
- Instagram account data storage (PostgreSQL)
- Statistics caching (MongoDB with TTL based on user activity level)
- Token management (encrypted access tokens)

**Background Jobs:**
- `UpdateInstagramAccountsJob` — Periodic account metadata refresh
- `CheckTokensJob` — Token expiration monitoring
- `CleanupExpiredAnalyticsJob` — TTL-based data cleanup

**Data Storage Split:**
- PostgreSQL: `InstagramAccount`, `Token` (transactional)
- MongoDB: Statistics, Audience distributions, Posts (cached with TTL)

**Key Entities:** `InstagramAccount`, `Token`, `UserActivity`

### Campaigns Module

**Core Responsibilities:**
- Campaign lifecycle management
- Blogger (influencer) profiles
- Offers and Pacts (contracts)
- Review system

**Campaign Status Flow:**
```
Pending → Confirmed → Done → Completed
   ↓         ↓
Rejected   Cancelled
```

**Key Business Rules:**
- Campaign has `Client` (who requests) and `Creator` (who executes)
- Offers belong to Pacts, Pacts belong to Bloggers
- Reviews can only be created for completed campaigns
- Blogger profile created automatically when user registers (via integration event)

**Key Entities:**
- `Campaign` — Aggregate root with status state machine
- `Blogger` — Influencer profile with Instagram metadata
- `Offer` — Service offering with price
- `Pact` — Contract containing multiple offers
- `Review` — Rating (1-5) and comment for completed campaigns

### Matching Module

**Core Responsibilities:**
- Full-text search via Elasticsearch
- Document indexing for all searchable entities
- Real-time sync via integration events

**Searchable Item Types:**
- `Blogger` — Influencer profiles
- `Campaign` — Marketing campaigns
- `Offer` — Service offerings
- `Review` — Campaign reviews
- `Pact` — Contracts
- `InstagramAccount` — Instagram profiles

**Search Features:**
- Fuzzy search (typo tolerance)
- Faceted filtering
- Synonym support
- Pagination with caching (3 min)

**Event-Driven Sync:**
- Consumes `*SearchSyncIntegrationEvent` from other modules
- Updates Elasticsearch index in real-time
- Idempotent processing (prevents duplicates)

**Seeding:**
- `ElasticsearchSeeder` checks existing documents before seeding
- Uses `GetExistingSourceEntityIdsAsync()` to avoid duplicates

---

## Cross-Module Communication

Modules communicate **only** via integration events through RabbitMQ:

| From | Event | To | Purpose |
|------|-------|-----|---------|
| Users | `UserRegisteredIntegrationEvent` | Campaigns | Create Blogger profile |
| Users | `InstagramAccountLinkingStartedIntegrationEvent` | Analytics | Fetch Instagram data |
| Analytics | `InstagramAccountDataFetchedIntegrationEvent` | Users | Complete linking saga |
| Campaigns | `*SearchSyncIntegrationEvent` | Matching | Index for search |
| Analytics | `InstagramAccountSearchSyncIntegrationEvent` | Matching | Index Instagram data |

**Pattern:** Outbox/Inbox ensures reliable delivery even if RabbitMQ is temporarily unavailable.

---

## Known Limitations

1. **Test coverage is incomplete** — Domain logic has unit tests, but integration tests are sparse
2. **No production deployment** — Only local Docker environment exists
3. **Instagram API limits** — Mock services exist for development without real API access
4. **Campaign workflow** — Domain model is solid, but API endpoints need more work
5. **Search refinement** — Basic fuzzy search works, but relevance tuning needed
6. **Basic monitoring** — Aspire Dashboard for logs/traces/metrics in dev, but no production alerting
7. **Single-tenant** — No multi-tenancy considerations

---

## What's Missing (Future Work)

- Comprehensive integration tests
- Production deployment configuration
- API documentation (OpenAPI/Swagger improvements)
- Performance testing and optimization
- Security audit
- Rate limiting and API throttling
- Better error messages in some areas
