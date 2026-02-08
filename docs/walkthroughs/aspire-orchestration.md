# Orchestration, Observability, and Health Monitoring

<div align="center">

*How Lanka solves the challenges of running a distributed application — and how .NET Aspire helps*

</div>

---

## Table of Contents

1. [The Problem: Running a Distributed Application](#the-problem-running-a-distributed-application)
2. [Observability: Understanding What Your System Is Doing](#observability-understanding-what-your-system-is-doing)
3. [Health Monitoring: Knowing When Things Break](#health-monitoring-knowing-when-things-break)
4. [Configuration Management: Wiring Services Together](#configuration-management-wiring-services-together)
5. [How Aspire Brings It All Together](#how-aspire-brings-it-all-together)
6. [The Aspire Dashboard](#the-aspire-dashboard)
7. [Implementation Reference](#implementation-reference)
8. [Troubleshooting](#troubleshooting)

---

## The Problem: Running a Distributed Application

Lanka depends on 7 infrastructure services: PostgreSQL, MongoDB, Redis, RabbitMQ, Keycloak, Elasticsearch, and Kibana. On top of those, two .NET projects (Lanka.Api, Lanka.Gateway) need to start, connect to the right services, and report their own health.

This creates three categories of problems that every distributed application faces:

**Observability** — When a user request takes 3 seconds, where is the time spent? Is it the database? The cache miss? The identity provider? Without instrumentation, you're debugging blind. You need traces (what happened), metrics (how much), and logs (why) — and they need to be correlated so you can follow a single request across services.

**Health monitoring** — PostgreSQL takes 2 seconds to start, Keycloak takes 15, Elasticsearch takes 30. If the API starts before Keycloak is ready, authentication fails. If Elasticsearch goes down after startup, search requests fail silently. You need health checks at two levels: *before* the app starts (are dependencies ready?) and *while* it runs (are they still reachable?).

**Configuration management** — Each service has a connection string. If the database port changes, every consumer needs updating. If credentials rotate, the same. Hardcoding connection strings in `appsettings.json` means keeping two sources of truth in sync — the infrastructure definition and the application configuration.

Lanka uses .NET Aspire to address all three. But before looking at the tool, it's worth understanding the concepts it builds on.

---

## Observability: Understanding What Your System Is Doing

### The Three Pillars

Observability in distributed systems rests on three complementary types of telemetry:

**Traces** record the journey of a single request through the system. When a user calls `GET /api/bloggers/profile`, a trace captures every step: the Gateway proxies the request → the API authenticates via Keycloak → a Dapper query runs against PostgreSQL → the response is returned. Each step is a *span* with a start time, duration, and attributes. Spans are linked by parent-child relationships to form a tree.

**Metrics** are numeric measurements aggregated over time. How many requests per second? What's the 95th percentile response time? How much memory is the GC reclaiming? Metrics answer "how is the system performing overall?" while traces answer "what happened to this specific request?"

**Logs** are discrete events with structured data. `User registered: {UserId: 42, Email: "test@example.com"}` — not a flat string, but queryable fields. When logs carry a `TraceId`, you can jump from a log entry to the full trace it belongs to.

### OpenTelemetry: The Standard

OpenTelemetry (OTel) is a vendor-neutral framework that standardizes how applications generate and export telemetry. The key design decision: OTel separates **instrumentation** (generating data) from **export** (sending it somewhere).

You instrument your code once. Where the data goes — the Aspire Dashboard in development, Datadog in staging, Grafana in production — is a configuration choice, not a code change. The transport protocol is OTLP (OpenTelemetry Protocol), a gRPC-based standard for transmitting traces, metrics, and logs.

### How Lanka Instruments

All instrumentation is configured in a single shared project — **ServiceDefaults** (`src/Api/Lanka.ServiceDefaults/Extensions.cs`). Both Lanka.Api and Lanka.Gateway reference it:

```csharp
// In Program.cs of both projects
builder.AddServiceDefaults();
```

This registers listeners for every library in Lanka's stack:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
        metrics.AddAspNetCoreInstrumentation()       // HTTP request metrics
            .AddHttpClientInstrumentation()           // Outbound HTTP call metrics
            .AddRuntimeInstrumentation())             // GC, thread pool, memory
    .WithTracing(tracing =>
        tracing.AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation()           // Incoming HTTP request spans
            .AddHttpClientInstrumentation()           // Outbound HTTP call spans
            .AddEntityFrameworkCoreInstrumentation()   // EF Core query spans
            .AddRedisInstrumentation()                // Redis command spans
            .AddNpgsql()                              // PostgreSQL command spans
            .AddSource("MassTransit")                 // RabbitMQ message spans
            .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
            .AddSource("Yarp.ReverseProxy"));         // YARP proxy spans
```

Each `.Add*Instrumentation()` hooks into a library's `DiagnosticSource` or `ActivitySource`. For example, `AddEntityFrameworkCoreInstrumentation()` listens to EF Core's diagnostic events and creates a trace span for every database operation — you see the SQL, the duration, and whether it succeeded or failed.

These registrations are safe for all consumers. The Gateway doesn't use EF Core, but registering `AddEntityFrameworkCoreInstrumentation()` is a no-op there — no spans are created, no errors are thrown.

### How Telemetry Reaches the Dashboard

```mermaid
sequenceDiagram
    participant App as Lanka.Api / Gateway
    participant OTel as OpenTelemetry SDK
    participant OTLP as OTLP Exporter
    participant Dash as Aspire Dashboard

    App->>OTel: Library emits diagnostic event<br/>(e.g., EF Core executes a query)
    OTel->>OTel: Instrumentation creates a Span<br/>with timing, attributes, parent ID
    OTel->>OTLP: Span batched for export
    OTLP->>Dash: gRPC push to<br/>OTEL_EXPORTER_OTLP_ENDPOINT
    Dash->>Dash: Stores and correlates<br/>spans into traces
```

Aspire automatically sets `OTEL_EXPORTER_OTLP_ENDPOINT` in each application project to point at the Dashboard's receiver. ServiceDefaults checks for this variable and enables export if it's present:

```csharp
bool useOtlpExporter = !string.IsNullOrWhiteSpace(
    builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
if (useOtlpExporter)
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
```

No manual URL configuration. If you run without Aspire, the variable isn't set, and no telemetry is exported — the code is silent.

### The Serilog Bridge

Lanka uses Serilog for structured logging. To make logs appear in the Dashboard alongside traces and metrics, Serilog output is bridged into the OpenTelemetry pipeline:

```csharp
// Serilog writes to its sinks AND to ASP.NET Core's ILoggerProvider pipeline
builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration),
    writeToProviders: true);

// ServiceDefaults registers an OTel log provider in that pipeline
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

The result: Serilog handles formatting and the console sink. OpenTelemetry captures the same entries and exports them via OTLP. In the Dashboard, logs carry their parent `TraceId` — clicking a log entry reveals the full distributed trace it belongs to.

---

## Health Monitoring: Knowing When Things Break

### Why Health Checks Matter

A distributed application has many failure modes. The database might be unreachable. The cache might be full. The identity provider might be restarting. Without health checks, failures surface as cryptic errors in user-facing requests. With them, you get a structured answer to "is everything okay?" — and infrastructure can act on it (load balancers stop routing, orchestrators restart containers, dashboards show alerts).

### Two Levels of Health Checks

Lanka uses health checks at two different levels because they answer different questions at different times.

**Level 1: Startup ordering** — These run *before* the application starts. When the AppHost calls `WaitFor(postgres)`, Aspire polls PostgreSQL's health check until it passes, only then launching Lanka.Api. This prevents the API from crashing on startup because a dependency isn't ready yet.

| Resource       | What Aspire Checks                                                                 |
|----------------|------------------------------------------------------------------------------------|
| PostgreSQL     | TCP connection to port 5432                                                        |
| Redis          | PING command returns PONG                                                          |
| RabbitMQ       | Management API responds                                                            |
| MongoDB        | Admin database ping                                                                |
| Elasticsearch  | `GET /` returns HTTP 200 (custom — built-in check assumes security is enabled)     |
| Keycloak       | `GET /realms/lanka` returns HTTP 200 (custom — built-in check uses HTTPS management port) |

**Level 2: Runtime monitoring** — These run *while* the application is running, exposed at `GET /healthz`. They verify that dependencies remain reachable after startup.

ServiceDefaults registers these checks conditionally — only for services whose connection strings are present:

```csharp
string? databaseCs = builder.Configuration.GetConnectionString("Database");
if (!string.IsNullOrEmpty(databaseCs))
    healthChecks.AddNpgSql(databaseCs);
```

Lanka.Api has connection strings for all services, so it gets health checks for all of them. Lanka.Gateway has none of these connection strings, so it only gets the "self" check. Same code, different behavior based on configuration.

#### Example: `GET /healthz` on Lanka.Api

```json
{
    "status": "Healthy",
    "totalDuration": "00:00:00.2104209",
    "entries": {
        "self": {
            "data": {},
            "duration": "00:00:00.0012900",
            "status": "Healthy",
            "tags": [
                "live"
            ]
        },
        "npgsql": {
            "data": {},
            "duration": "00:00:00.0104221",
            "status": "Healthy",
            "tags": []
        },
        "redis": {
            "data": {},
            "duration": "00:00:00.0263132",
            "status": "Healthy",
            "tags": []
        },
        "rabbitmq": {
            "data": {},
            "duration": "00:00:00.0343505",
            "status": "Healthy",
            "tags": []
        },
        "mongodb": {
            "data": {},
            "duration": "00:00:00.1269383",
            "status": "Healthy",
            "tags": []
        },
        "elasticsearch": {
            "data": {},
            "duration": "00:00:00.0678815",
            "status": "Healthy",
            "tags": []
        },
        "keycloak": {
            "data": {},
            "duration": "00:00:00.0679847",
            "status": "Healthy",
            "tags": []
        },
        "masstransit-bus": {
            "data": {
                "Endpoints": {
                    "rabbitmq://localhost/blogger-updated-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-data-fetched-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-linking-failure-cleaned-up-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-linking-failed-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-linking-completed-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-data-renewed-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-renewal-access-failure-cleaned-up-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-renewal-failed-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-access-renewal-completed-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/user-registered-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/user-deleted-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-linking-started-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-access-renewal-started-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/blogger-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/campaign-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/review-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/offer-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/pact-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/instagram-account-search-sync-integration-event-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/link-instagram-state-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/renew-instagram-access-state-lanka-api": {
                        "status": "Healthy",
                        "description": "ready"
                    },
                    "rabbitmq://localhost/MacBookProIllia_LankaApi_bus_4c4yyyn3jqffte4nbdxgqenunq?temporary=true": {
                        "status": "Healthy",
                        "description": "ready (not started)"
                    }
                }
            },
            "description": "Ready",
            "duration": "00:00:00.0242337",
            "status": "Healthy",
            "tags": [
                "ready",
                "masstransit"
            ]
        }
    }
}
```

Each entry reports status, duration, and service-specific data. The `masstransit-bus` check verifies that every RabbitMQ consumer endpoint is reachable — one entry per integration event consumer and saga state machine. If any dependency is down, the overall status changes to `Unhealthy` and the entry shows the failure reason.

### Why Both Levels

```mermaid
graph LR
    subgraph "Level 1 — Startup"
        AH[AppHost] -->|WaitFor| PG[PostgreSQL healthy?]
        AH -->|WaitFor| RD[Redis healthy?]
        AH -->|WaitFor| RMQ[RabbitMQ healthy?]
        PG -->|yes| START[Start Lanka.Api]
        RD -->|yes| START
        RMQ -->|yes| START
    end

    subgraph "Level 2 — Runtime"
        START --> HC["/healthz endpoint"]
        HC --> PG2[PostgreSQL reachable?]
        HC --> RD2[Redis reachable?]
        HC --> RMQ2[RabbitMQ reachable?]
        HC --> ES[Elasticsearch reachable?]
        HC --> KC[Keycloak reachable?]
    end

    style START fill:#2d6,stroke:#174,color:#fff
```

Level 1 prevents startup against unavailable infrastructure. Level 2 monitors ongoing health for load balancers, alerting systems, and the Aspire Dashboard.

---

## Configuration Management: Wiring Services Together

### The Connection String Problem

Every service has connection parameters: host, port, credentials, database name. In a traditional setup, these live in `appsettings.json` and must match whatever the infrastructure actually runs. Change a port in Docker Compose, forget to update the config file — and you get a runtime error with no compile-time warning.

### Convention-Based Injection

Aspire solves this with a naming convention. When you declare a resource in the AppHost, the **resource name** becomes the **connection string key**:

```mermaid
graph LR
    A["AppHost declares:<br/>postgres.AddDatabase(&quot;Database&quot;)"] --> B["Aspire starts PostgreSQL,<br/>discovers its address"]
    B --> C["Sets env var:<br/>ConnectionStrings__Database=<br/>Host=localhost;Port=5432;..."]
    C --> D["Lanka.Api reads:<br/>builder.Configuration<br/>.GetConnectionString(&quot;Database&quot;)"]
```

`AddDatabase("Database")` → `ConnectionStrings:Database`. `AddRedis("Cache")` → `ConnectionStrings:Cache`. The application reads them through `IConfiguration` as always — the only difference is the values come from environment variables instead of a config file.

| Resource Name    | Config Key                     | Format                                           |
|------------------|--------------------------------|--------------------------------------------------|
| `"Database"`     | `ConnectionStrings:Database`   | `Host=localhost;Port=5432;Database=lanka;...`     |
| `"Cache"`        | `ConnectionStrings:Cache`      | `localhost:6379`                                  |
| `"Queue"`        | `ConnectionStrings:Queue`      | `amqp://guest:password@localhost:5672`            |
| `"Mongo"`        | `ConnectionStrings:Mongo`      | `mongodb://root:password@localhost:27017/Mongo?...` |
| `"lanka-search"` | `ConnectionStrings:lanka-search` | `http://localhost:59123`                        |

The application code doesn't change at all:

```csharp
string databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
string redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;
```

### Format Adaptation: RabbitMQ

Not all formats match what libraries expect. Aspire injects RabbitMQ as an AMQP URI (`amqp://guest:lanka-dev@localhost:5672`), but Lanka's `RabbitMqSettings` originally expected separate `host`, `username`, and `password` fields.

The class was rewritten to parse URIs:

```csharp
// File: src/Common/Lanka.Common.Infrastructure/EventBus/RabbitMqSettings.cs
public RabbitMqSettings(string connectionString)
{
    Host = connectionString;
    var uri = new Uri(connectionString);
    string[] userInfo = uri.UserInfo.Split(':', 2);
    Username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0])
        ? Uri.UnescapeDataString(userInfo[0]) : "guest";
    Password = userInfo.Length > 1
        ? Uri.UnescapeDataString(userInfo[1]) : "guest";
}
```

---

## How Aspire Brings It All Together

.NET Aspire addresses the three problems above through two projects:

**Lanka.AppHost** (`src/Api/Lanka.AppHost/Program.cs`) — A C# program that declares all resources and their relationships. It starts containers, runs health checks, injects connection strings, and launches application projects in the correct order.

```bash
dotnet run --project src/Api/Lanka.AppHost
```

This single command starts 7 infrastructure containers and 2 application projects with health-checked startup ordering.

**Lanka.ServiceDefaults** (`src/Api/Lanka.ServiceDefaults/Extensions.cs`) — A shared project that configures all cross-cutting concerns. Both Lanka.Api and Lanka.Gateway call `builder.AddServiceDefaults()`, which sets up OTel instrumentation, health checks, HTTP resilience, and service discovery.

Aspire is not limited to local development. The same AppHost definition can generate deployment manifests for Azure Container Apps, Kubernetes, or other targets. Lanka currently uses it for development orchestration, but the model extends to production.

**Why I adopted it:** See [ADR 015](../architecure-decision-log/015-aspire-adoption.md) for the full decision rationale.

---

## The Aspire Dashboard

The Aspire Dashboard launches automatically with the AppHost. It's the single UI for all three pillars of observability.

### Resources

The graph view visualizes resource dependencies — clicking a resource reveals its references, endpoints, and health checks.

![Aspire Dashboard — Resource graph with project details](../images/aspire-resource-graph.png)

### Structured Logs

All logs from all resources in one view, with log level filtering, full-text search, and trace correlation. Clicking a log entry reveals its details — message, category, structured properties, and a link to the parent trace.

![Aspire Dashboard — Structured logs with log entry details](../images/aspire-structured-logs.png)

### Distributed Traces

Request flows across services as a waterfall timeline. Each span is an operation: an HTTP request, a database query, a cache lookup. Spans nest to show parent-child relationships — a Gateway proxy request triggering API endpoint execution, which runs a query against PostgreSQL.

![Aspire Dashboard — Trace waterfall showing Gateway to API flow](../images/aspire-spans-waterfall.png)

### Metrics

Runtime and HTTP metrics: request duration percentiles, active connections, GC collections, thread pool queue length.

![Aspire Dashboard — Metrics view with authentication duration chart](../images/aspire-metrics.png)

---

## Implementation Reference

This section covers the AppHost configuration details for those who want to understand or modify the orchestration setup.

### AppHost: Resource Declarations

**File:** `src/Api/Lanka.AppHost/Program.cs`

#### Parameters and Secrets

```csharp
IResourceBuilder<ParameterResource> pgPassword = builder.AddParameter("pg-password", secret: true);
```

Parameters marked `secret: true` resolve from `appsettings.Development.json` (under `Parameters`) or user secrets. Fixed passwords are used instead of auto-generation because Docker volumes persist data — a new password would break authentication against existing data.

**File:** `src/Api/Lanka.AppHost/appsettings.Development.json`

```json
{
  "Parameters": {
    "pg-password": "lanka-dev",
    "rabbitmq-password": "lanka-dev",
    "keycloak-password": "admin",
    "mongo-password": "lanka-dev"
  }
}
```

For sensitive environments, use user secrets instead:

```bash
cd src/Api/Lanka.AppHost
dotnet user-secrets set "Parameters:pg-password" "your-password"
```

#### Infrastructure Resources

```csharp
IResourceBuilder<PostgresDatabaseResource> lankaDb = postgres.AddDatabase("Database", databaseName: "lanka");
IResourceBuilder<RedisResource> redis = builder.AddRedis("Cache", port: 6379);
IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("Queue", port: 5672, password: rabbitPassword);
IResourceBuilder<MongoDBDatabaseResource> mongoDb = mongo.AddDatabase("Mongo");
```

Elasticsearch requires the most configuration — ICU analysis plugin, JVM memory, security disabled:

```csharp
IResourceBuilder<ElasticsearchResource> elasticsearch = builder.AddElasticsearch("lanka-search")
    .WithImageTag("9.1.10")
    .WithDataVolume()
    .WithVolume("lanka-search-plugins", "/usr/share/elasticsearch/plugins")
    .WithBindMount("elasticsearch-plugins.yml",
        "/usr/share/elasticsearch/config/elasticsearch-plugins.yml")
    .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
    .WithEnvironment("xpack.security.enabled", "false");
```

#### Health Check Overrides

Two built-in health checks required replacing because their assumptions didn't match Lanka's configuration:

```csharp
// Elasticsearch: built-in check uses credentials, but security is disabled
elasticsearch.WithHttpHealthCheck("/");

// Keycloak: built-in check uses HTTPS management port with self-signed cert
keycloak.WithHttpHealthCheck("/realms/lanka");
```

#### Application Projects

```csharp
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Lanka_Api>("lanka-api")
    .WithReference(lankaDb)       // Injects ConnectionStrings:Database
    .WithReference(redis)         // Injects ConnectionStrings:Cache
    .WithReference(rabbitMq)      // Injects ConnectionStrings:Queue
    .WithReference(mongoDb)       // Injects ConnectionStrings:Mongo
    .WithReference(elasticsearch) // Injects ConnectionStrings:lanka-search
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WaitFor(mongo)
    .WaitFor(elasticsearch)
    .WaitFor(keycloak);
```

Kibana has no Aspire package, so it's added as a generic container with `isProxied: false` to prevent Aspire's port proxy from breaking its websocket connections.

### Data Persistence

Every stateful resource uses `WithDataVolume()` — Docker named volumes that survive container restarts. To reset all data:

```bash
docker volume rm $(docker volume ls -q --filter name=lanka)
```

---

## Troubleshooting

### Stale Docker Volumes with Old Credentials

**Symptom**: PostgreSQL or RabbitMQ fails to authenticate after changing passwords.

**Cause**: Docker volumes contain data initialized with old passwords.

**Fix**:

```bash
docker volume rm $(docker volume ls -q --filter name=lanka)
dotnet run --project src/Api/Lanka.AppHost
```

### Elasticsearch Connection Refused on Startup

**Symptom**: `Cannot connect to Elasticsearch` error during API startup.

**Cause**: Elasticsearch takes longer to become ready than other containers. If the AppHost health check is missing, `WaitFor(elasticsearch)` doesn't wait for actual readiness.

**Fix**: Ensure the AppHost has `.WithHttpHealthCheck("/")` on the Elasticsearch resource.

### Kibana Version Mismatch

**Symptom**: Kibana shows "version mismatch" error.

**Fix**: Both Kibana and Elasticsearch must use the exact same version — pinned to `9.1.10` in the AppHost.

### Gateway Certificate Errors

**Symptom**: Gateway fails to start with certificate errors.

**Fix**: Remove `Kestrel` section from Gateway's `appsettings.json`. Aspire uses the standard dev certificate (`dotnet dev-certs https --trust`).

### Connection String Returns Null

**Symptom**: `GetConnectionString("X")` returns null at runtime.

**Fix**: Check the resource name passed to `AddDatabase()`, `AddRedis()`, etc. — that name becomes the connection string key.

---

## Files Reference

| Component                               | File                                                                  |
|-----------------------------------------|-----------------------------------------------------------------------|
| AppHost orchestration                   | `src/Api/Lanka.AppHost/Program.cs`                                    |
| AppHost secrets                         | `src/Api/Lanka.AppHost/appsettings.Development.json`                  |
| ES plugin config                        | `src/Api/Lanka.AppHost/elasticsearch-plugins.yml`                     |
| ServiceDefaults (OTel + health checks)  | `src/Api/Lanka.ServiceDefaults/Extensions.cs`                         |
| API startup                             | `src/Api/Lanka.Api/Program.cs`                                        |
| Gateway startup                         | `src/Api/Lanka.Gateway/Program.cs`                                    |
| RabbitMQ URI parser                     | `src/Common/Lanka.Common.Infrastructure/EventBus/RabbitMqSettings.cs` |

---

<div align="center">

*The best infrastructure is the kind you don't have to think about. Aspire gets closer to that ideal by turning orchestration, observability, and health monitoring into code that lives alongside the application it serves.*

</div>
