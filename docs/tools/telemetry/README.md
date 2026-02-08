# Telemetry in Lanka

Telemetry is the process of collecting and transmitting data from remote sources to a central location for monitoring and analysis. In Lanka, telemetry provides insight into the application's behavior, performance, and health across all modules and infrastructure services.

This document provides an overview of the telemetry stack and how the components work together.

## Telemetry Stack

The Lanka project uses the following tools for telemetry:

* **OpenTelemetry (OTel):** A vendor-neutral, open-source observability framework for generating, collecting, and exporting telemetry data — traces, metrics, and logs. OTel defines a standard protocol (OTLP) for transmitting this data.
* **Serilog:** A structured logging library for .NET applications. Logs are bridged to OpenTelemetry via `writeToProviders: true`, so they appear in the Aspire Dashboard alongside traces and metrics.
* **Aspire Dashboard:** A unified UI for structured logs, distributed traces, and metrics. Launched automatically by the AppHost. Replaces the previous Seq (log aggregation) and Jaeger (trace visualization) setup.

## How They Work Together

All telemetry configuration is centralized in **ServiceDefaults** (`src/Api/Lanka.ServiceDefaults/Extensions.cs`). Both Lanka.Api and Lanka.Gateway call `builder.AddServiceDefaults()`, which registers:

**Tracing** — spans are created for every significant operation:
- ASP.NET Core incoming HTTP requests
- HttpClient outbound calls
- Entity Framework Core database queries
- Redis cache operations
- PostgreSQL commands (via Npgsql)
- MassTransit message publish/consume
- MongoDB operations
- YARP reverse proxy requests

**Metrics** — numeric measurements collected over time:
- HTTP request durations and status codes
- Outbound HTTP call performance
- Runtime metrics (GC, thread pool, memory)

**Logs** — Serilog captures structured log entries and bridges them into the OTel pipeline. The Aspire Dashboard correlates logs with their parent trace — clicking a log entry reveals the full distributed trace.

**Export** — all telemetry is exported via OTLP (gRPC) to the Aspire Dashboard. The `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable is injected automatically by Aspire; no manual URL configuration is needed.

Instrumentation registrations are safe for all consumers. `AddSource()` for a library that isn't used in a given project is a no-op — the Gateway gets EF Core instrumentation registered but never produces EF Core spans.

![CQRS](/docs/images/telemetry.jpg)

## Detailed Documentation

* [OpenTelemetry Integration](./open-telemetry/README.md)
* [Serilog for Structured Logging](./serilog/README.md)
* [Distributed Tracing](./tracing/README.md)
* [Orchestration & Observability Walkthrough](../../walkthroughs/aspire-orchestration.md#observability-understanding-what-your-system-is-doing)
