# Development Tools

<div align="center">

*Infrastructure and tooling for Lanka development*

</div>

---

## Overview

This section documents the tools and infrastructure used in Lanka. Whether you're setting up your development environment or trying to understand how the pieces fit together, you'll find the details here.

---

## Tool Categories

<table>
<tr>
<td width="50%">

### Core Development

- [Testing Tools](../development/faq.md#-testing-issues) — Unit, integration, architecture tests
- [Debugging](../development/faq.md#-debugging-problems) — Debugging and profiling
- [Database Tools](../development/faq.md#-database--migrations) — Migrations and data management

### Frontend

- [Angular Guide](../frontend/README.md) — Build, lint, and test commands

</td>
<td width="50%">

### Infrastructure

- [Docker Setup](../development/development-setup.md#-container-infrastructure-setup) — Container orchestration
- [Messaging](messaging/) — RabbitMQ and MassTransit
- [Gateway](gateway/) — YARP reverse proxy
- [Telemetry](telemetry/) — OpenTelemetry, Serilog, Seq

</td>
</tr>
</table>

---

## Local Development URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Lanka API | http://localhost:4307 | Main application |
| Health Check | http://localhost:4307/healthz | System status |
| Gateway | http://localhost:4308 | YARP reverse proxy |
| RabbitMQ | http://localhost:15672 | Message queue admin |
| Keycloak | http://localhost:18080/admin | Identity management |
| Seq | http://localhost:8081 | Centralized logs |
| Jaeger | http://localhost:16686 | Distributed tracing |
| Kibana | http://localhost:5601 | Elasticsearch UI |

---

## Database Connections

| Database | Connection | Usage |
|----------|------------|-------|
| PostgreSQL | `Host=localhost;Port=5432;Database=lanka_dev;Username=postgres;Password=postgres` | Primary data |
| MongoDB | `mongodb://localhost:27017/lanka_analytics` | Analytics storage |
| Redis | `localhost:6379` | Caching |

---

## IDE Setup

### VS Code Extensions

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-vscode-remote.remote-containers",
    "ms-azuretools.vscode-docker",
    "humao.rest-client",
    "redhat.vscode-yaml"
  ]
}
```

### JetBrains Rider Plugins

- Docker — Container management
- Database Tools — PostgreSQL/MongoDB support
- HTTP Client — API testing
- GitToolBox — Enhanced Git integration

---

## Testing Stack

### Unit Tests
- **xUnit** — Test framework
- **FluentAssertions** — Readable assertions
- **NSubstitute** — Mocking
- **Bogus** — Fake data generation

### Integration Tests
- **TestContainers** — Docker-based database tests
- **WebApplicationFactory** — API integration tests

### Performance Tests
- **BenchmarkDotNet** — Micro-benchmarks
- **NBomber** — Load testing

---

## Observability

### Logging
- **Serilog** — Structured logging
- **Seq** — Log aggregation and search (http://localhost:8081)

### Tracing
- **OpenTelemetry** — Distributed tracing
- **Jaeger** — Trace visualization (http://localhost:16686)

### Metrics
- Health checks at `/healthz`
- Custom metrics via OpenTelemetry

---

## Docker Services

The `docker-compose.yml` includes:

```yaml
services:
  postgres:
    image: postgres:17.6
    ports: ["5432:5432"]

  mongodb:
    image: mongo:8.0
    ports: ["27017:27017"]

  redis:
    image: redis:8.2
    ports: ["6379:6379"]

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    ports: ["5672:5672", "15672:15672"]

  keycloak:
    image: quay.io/keycloak/keycloak:26.4
    ports: ["18080:8080"]

  seq:
    image: datalust/seq:2024
    ports: ["8081:80"]

  elasticsearch:
    image: elasticsearch:9.1.3
    ports: ["9200:9200"]

  jaeger:
    image: jaegertracing/all-in-one:1.74.0
    ports: ["16686:16686"]
```

---

## Code Quality

### Analysis Configuration

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <AnalysisLevel>latest</AnalysisLevel>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

### Formatting
- **EditorConfig** — Consistent code style
- **StyleCop** — C# style rules

---

## Tool Documentation

- [Development Setup](../development/development-setup.md) — Full environment configuration
- [FAQ](../development/faq.md) — Common issues and solutions
- [Messaging](messaging/README.md) — RabbitMQ + MassTransit
- [Gateway](gateway/README.md) — YARP configuration
- [Telemetry](telemetry/README.md) — Logging and tracing

---

<div align="center">

*Tools should get out of your way and let you focus on the code.*

</div>
