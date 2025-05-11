# OpenTelemetry Integration

OpenTelemetry is a vendor-neutral, open-source observability framework for generating, collecting, and exporting telemetry data such as traces, metrics, and logs. It provides a unified standard for instrumenting applications and collecting telemetry data, making it easier to monitor and analyze the behavior of distributed systems.

## Usage in Lanka Project

In the Lanka project, OpenTelemetry is used for tracing and metrics. Tracing helps to understand the flow of requests through the system and identify performance bottlenecks. Metrics provide insights into the overall health and performance of the system.

## Configuration

OpenTelemetry is configured in the `Lanka.Common.Infrastructure` project using the `AddTracing` extension method.

### Service Name

The service name is configured using the `DiagnosticsConfig.ServiceName` constant in each API project (e.g., `Lanka.Api`, `Lanka.Gateway`). This name is used to identify the service in the telemetry data.

* **Example (`src/Api/Lanka.Api/OpenTelemetry/DiagnosticsConfig.cs`):**

    ```csharp
    namespace Lanka.Api.OpenTelemetry;

    internal static class DiagnosticsConfig
    {
        public const string ServiceName = "Lanka.Api";
    }
    ```

### OTLP Exporter Endpoint

The OTLP (OpenTelemetry Protocol) exporter endpoint is configured using the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable. This endpoint specifies the address of the OpenTelemetry collector or backend where the telemetry data should be sent.

*   **Example (`src/Api/Lanka.Api/appsettings.json`):**

    ```json
    {
      "OTEL_EXPORTER_OTLP_ENDPOINT": "<your-otel-exporter-endpoint>"
    }
    ```

### Instrumentation

The following instrumentations are enabled in the Lanka project:

*   **AspNetCoreInstrumentation:** Collects telemetry data from ASP.NET Core applications.
*   **HttpClientInstrumentation:** Collects telemetry data from HTTP client requests.
*   **EntityFrameworkCoreInstrumentation:** Collects telemetry data from Entity Framework Core database operations.
*   **RedisInstrumentation:** Collects telemetry data from Redis operations.
*   **Npgsql:** Collects telemetry data from Npgsql database operations.
*   **MassTransit:** Collects telemetry data from MassTransit message bus operations.

## Code Examples

### Adding Custom Spans and Events

You can add custom spans and events to your code to provide more detailed information about the execution of your application.

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

OpenTelemetry automatically propagates context across services using HTTP headers. This allows you to trace requests as they flow through the system.

## Troubleshooting Tips

* Make sure the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable is configured correctly.
* Check the Jaeger UI to see if traces are being collected.
* Enable debug logging in OpenTelemetry to see more detailed information about the telemetry data being collected.