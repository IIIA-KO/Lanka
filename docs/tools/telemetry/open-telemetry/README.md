# OpenTelemetry Integration

OpenTelemetry (OTel) is a vendor-neutral, open-source observability framework for generating, collecting, and exporting telemetry data — traces, metrics, and logs. It provides a unified standard for instrumenting applications, making it possible to monitor distributed systems without vendor lock-in.

## Usage in Lanka

Lanka uses OpenTelemetry for distributed tracing and runtime metrics. Tracing records the path a request takes through the system (HTTP request → database query → cache lookup → response), helping identify performance bottlenecks and failure points. Metrics provide quantitative measurements of system health over time.

## Configuration

All OpenTelemetry instrumentation is configured in a single place: **ServiceDefaults** (`src/Api/Lanka.ServiceDefaults/Extensions.cs`). Both Lanka.Api and Lanka.Gateway call `builder.AddServiceDefaults()`, which registers all instrumentation.

### Tracing

```csharp
builder.Services.AddOpenTelemetry()
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

Each `.Add*Instrumentation()` call registers a listener that hooks into the corresponding library's `DiagnosticSource` or `ActivitySource`. For example, `AddEntityFrameworkCoreInstrumentation()` listens to EF Core's diagnostic events and creates trace spans for every database operation.

These registrations are safe for all consumers. `AddSource()` for a library that isn't present in a project is a no-op — no spans are created, no errors are thrown. The Gateway gets EF Core instrumentation registered but never produces EF Core spans because it doesn't use EF Core.

The service name (`builder.Environment.ApplicationName`) is set automatically by Aspire based on the resource name in the AppHost (e.g., `lanka-api`, `lanka-gateway`), so traces are correctly attributed to their source.

### Metrics

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
        metrics.AddAspNetCoreInstrumentation()    // HTTP request duration, status codes
            .AddHttpClientInstrumentation()        // Outbound HTTP performance
            .AddRuntimeInstrumentation());         // GC, thread pool, memory
```

### OTLP Export

Telemetry data is exported via OTLP (OpenTelemetry Protocol) — a gRPC-based protocol for transmitting traces, metrics, and logs. Aspire automatically sets the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable in each application project to point at the Dashboard's receiver:

```csharp
bool useOtlpExporter = !string.IsNullOrWhiteSpace(
    builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
if (useOtlpExporter)
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
```

No manual URL configuration needed. If the environment variable is set, telemetry is exported. If not (e.g., running without Aspire), the code silently does nothing.

### Serilog Bridge

Lanka uses Serilog for structured logging. Serilog output is bridged into the OTel pipeline via `writeToProviders: true`:

```csharp
builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration),
    writeToProviders: true);
```

ServiceDefaults registers an OTel log provider that captures these logs and exports them via OTLP:

```csharp
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

The result: logs appear in the Aspire Dashboard correlated with their parent trace. Clicking a log entry reveals the full distributed trace it belongs to.

## Code Examples

### Adding Custom Spans and Events

You can add custom spans and events to provide more detailed information about application behavior:

```csharp
using System.Diagnostics;

// Start a new activity (span)
using (Activity activity = activitySource.StartActivity("MyCustomOperation"))
{
    // Add attributes to the activity
    activity?.SetTag("attribute1", "value1");
    activity?.SetTag("attribute2", 123);

    // Add an event to the activity
    activity?.AddEvent(new ActivityEvent("MyCustomEvent", tags: new ActivityTagsCollection {
        { "eventAttribute1", "eventValue1" }
    }));

    // Perform the operation
    DoSomething();

    // Set the status of the activity
    activity?.SetStatus(ActivityStatusCode.Ok);
}
```

### Propagating Context Across Services

OpenTelemetry automatically propagates context across services using HTTP headers (`traceparent`, `tracestate`). When Lanka.Gateway proxies a request to Lanka.Api, the trace context is propagated so the entire request flow appears as a single distributed trace in the Dashboard.

## Troubleshooting

* Check the **Aspire Dashboard** to see if traces are being collected (the URL is printed to the console when running the AppHost).
* Verify that the service appears in the Dashboard's resource list with a "Healthy" status.
* If traces are missing for a specific library, check that the corresponding `AddSource()` or `Add*Instrumentation()` call is present in ServiceDefaults.
* For the full observability architecture, see the [Orchestration & Observability Walkthrough](../../../walkthroughs/aspire-orchestration.md#observability-understanding-what-your-system-is-doing).
