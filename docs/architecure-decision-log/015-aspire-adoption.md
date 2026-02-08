# ADL 015 - Adoption of .NET Aspire

**Date:** 2025-08-02
**Status:** Accepted

## Context

Lanka's development environment relied on Docker Compose to manage 8+ infrastructure containers (PostgreSQL, MongoDB, Redis, RabbitMQ, Keycloak, Elasticsearch, Kibana, Seq, Jaeger). This approach had several pain points:

1. **Manual connection string management**: Connection strings were hardcoded in `appsettings.json` and had to be kept in sync with Docker Compose port mappings and credentials.
2. **No health-check orchestration**: Docker Compose `depends_on` only checks if a container started — it doesn't wait for actual readiness. The API would crash on startup if Keycloak or Elasticsearch wasn't ready yet.
3. **Duplicated telemetry setup**: Each project (API, Gateway) independently configured OpenTelemetry instrumentation, OTLP exporters, and health check endpoints with duplicate code.
4. **Two-step startup**: Developers had to run `docker compose up -d` first, then `dotnet run` — two separate terminal commands with a wait in between.
5. **Separate observability tools**: Seq for logs, Jaeger for traces — two different UIs to check during development, neither integrated into the .NET tooling.

## Decision

Adopt **.NET Aspire 13** as the application orchestrator. This involves:

- A new **Lanka.AppHost** project that declaratively defines all infrastructure containers and application projects
- A new **Lanka.ServiceDefaults** project that centralizes OpenTelemetry, health checks, HTTP resilience, and service discovery
- Both API and Gateway reference ServiceDefaults and are launched by the AppHost

Aspire is not limited to local development. The same AppHost definition serves as the source of truth for the application's resource topology and can generate deployment manifests for Azure Container Apps, Kubernetes, or other targets. Lanka currently uses it for development orchestration, but the model extends to production.

## Alternatives Considered

### Keep Docker Compose
- **Pros**: Familiar, no .NET-specific tooling required, works for any stack
- **Cons**: Doesn't solve connection string wiring, health-check orchestration, or telemetry duplication. Two-step startup remains.
- **Why rejected**: The pain points were real and growing. Docker Compose solves container orchestration but not .NET application orchestration.

### Microsoft Tye
- **Pros**: Similar concept to Aspire, was Microsoft's earlier attempt
- **Cons**: Archived and no longer maintained. No production path.
- **Why rejected**: Dead project with no future support.

### Custom shell scripts
- **Pros**: Simple, no new dependencies
- **Cons**: Fragile, platform-specific, doesn't solve the connection string or telemetry problems
- **Why rejected**: Treats symptoms, not root causes.

## Consequences

### Positive

- **Single command startup**: `dotnet run --project src/Api/Lanka.AppHost` starts all containers and both application projects
- **Auto-wired connection strings**: Aspire injects `ConnectionStrings:*` into application projects based on resource names — no more manual sync
- **Health-check orchestration**: `WaitFor()` ensures the API doesn't start until dependencies are actually ready (healthy, not just running)
- **Aspire Dashboard**: A single UI for structured logs, distributed traces, and metrics — replaces both Seq and Jaeger
- **ServiceDefaults centralizes cross-cutting concerns**: All OpenTelemetry instrumentation (base + domain-specific), all health checks, OTLP export, and HTTP resilience configured once
- **Named Docker volumes**: Data persists across container restarts via `WithDataVolume()`
- **Deployment path**: The AppHost resource model can generate manifests for production orchestrators

### Negative

- **Requires .NET Aspire workload**: Developers must install `dotnet workload install aspire`
- **Docker still required**: Aspire uses Docker under the hood for container resources
- **Learning curve**: Aspire's abstractions (resource naming → connection string keys, port proxying, parameter secrets) aren't immediately obvious
- **Connection string format changes**: Aspire generates AMQP URIs for RabbitMQ instead of separate host/user/pass — required rewriting `RabbitMqSettings`

### Neutral

- **Docker Compose file kept**: Retained in the repository for CI pipelines and as a reference
- **Serilog still used**: Aspire's OTel logging is additive; Serilog handles structured logging with `writeToProviders: true` bridging to OTel

## Implementation Notes

### Key Files

| File | Purpose |
|------|---------|
| `src/Api/Lanka.AppHost/Program.cs` | Orchestration definition — all resources and their relationships |
| `src/Api/Lanka.ServiceDefaults/Extensions.cs` | Shared OTel instrumentation, health checks, resilience, service discovery |
| `src/Api/Lanka.AppHost/appsettings.Development.json` | Local dev passwords for Aspire parameters |

### ServiceDefaults Consolidation

All OpenTelemetry instrumentation is configured in `ServiceDefaults/Extensions.cs` — both base instrumentation (ASP.NET Core, HttpClient, Runtime) and domain-specific instrumentation (EF Core, Redis, Npgsql, MassTransit, MongoDB, YARP). This means adding `AddSource()` for a library that isn't used in a given project is a no-op — the Gateway gets EF Core instrumentation registered but never produces EF Core spans.

All health checks are also in ServiceDefaults, registered conditionally based on connection string presence. Lanka.Api gets checks for all 7 services; Lanka.Gateway only gets the "self" check. A single `/healthz` endpoint exposes the results.

### RabbitMQ URI Parsing

Aspire injects RabbitMQ connection strings as AMQP URIs (e.g., `amqp://guest:guest@localhost:5672`). The `RabbitMqSettings` class was rewritten to parse URIs:

```csharp
public RabbitMqSettings(string connectionString)
{
    Host = connectionString;
    var uri = new Uri(connectionString);
    string[] userInfo = uri.UserInfo.Split(':', 2);
    Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "guest";
    Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "guest";
}
```

### Health Check Overrides

Two Aspire built-in health checks required overriding:

- **Elasticsearch**: Built-in check uses `ElasticsearchClient` with security credentials, but Lanka runs ES with `xpack.security.enabled=false`. Replaced with `.WithHttpHealthCheck("/")` — a simple HTTP GET on the root endpoint.
- **Keycloak**: Built-in check targets the management HTTPS port, which fails due to self-signed certificates. Replaced with `.WithHttpHealthCheck("/realms/lanka")`.

## Related Decisions

- [002 - Technology Stack](002-technology-stack.md): Aspire is a new addition to the tech stack
- [009 - Configuration Management](009-configuration-management.md): Aspire changes how connection strings are managed — auto-injected rather than manually configured

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Orchestration Walkthrough](../walkthroughs/aspire-orchestration.md)
