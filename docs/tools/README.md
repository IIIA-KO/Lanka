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
- [Telemetry](telemetry/) — OpenTelemetry, Serilog, Aspire Dashboard

</td>
</tr>
</table>

---

## Local Development URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Aspire Dashboard | *(shown in console output)* | Logs, traces, metrics |
| Lanka API | http://localhost:4307 | Main application |
| Health Check | http://localhost:4307/healthz | System status |
| Gateway | https://localhost:4308 | YARP reverse proxy |
| RabbitMQ | http://localhost:15672 | Message queue admin |
| Keycloak | http://localhost:18080/admin | Identity management |
| Kibana | http://localhost:5601 | Elasticsearch UI |

---

## Database Connections

Connection strings are **auto-injected by Aspire** when running via the AppHost. The values below are for manual tool access (e.g., DBeaver, pgAdmin, MongoDB Compass):

| Database | Connection | Usage |
|----------|------------|-------|
| PostgreSQL | `Host=localhost;Port=5432;Database=lanka;Username=postgres;Password=lanka-dev` | Primary data |
| MongoDB | `mongodb://root:lanka-dev@localhost:27017` | Analytics storage |
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

### Aspire Dashboard
The **Aspire Dashboard** is the single observability UI for Lanka. It provides structured logs, distributed traces, and metrics in one place. Its URL is shown in the console when running the AppHost.

### Logging
- **Serilog** — Structured logging with console sink
- Serilog output is bridged to OpenTelemetry via `writeToProviders: true`
- Logs appear in the Aspire Dashboard with trace correlation

### Tracing
- **OpenTelemetry** — Distributed tracing configured in ServiceDefaults
- Traces cover: HTTP requests, EF Core queries, Redis, PostgreSQL, MassTransit, MongoDB, YARP
- Aspire Dashboard shows trace waterfalls with nested spans

### Metrics
- Health checks at `/healthz` (JSON report of all dependency statuses)
- Runtime metrics via OpenTelemetry (GC, thread pool, HTTP request durations)

---

## Docker Services

Infrastructure containers are managed by **.NET Aspire** via the AppHost project (`src/Api/Lanka.AppHost/Program.cs`). Aspire handles container lifecycle, health checks, and connection string injection automatically.

Start everything with:
```bash
dotnet run --project src/Api/Lanka.AppHost
```

For details, see [Development Setup](../development/development-setup.md#-container-infrastructure-setup) and the [Aspire Orchestration Walkthrough](../walkthroughs/aspire-orchestration.md).

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
