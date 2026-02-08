# Serilog for Structured Logging

Serilog is a structured logging library for .NET applications that captures rich, queryable log data. In Lanka, Serilog output is bridged to OpenTelemetry and exported to the Aspire Dashboard alongside traces and metrics.

> **Note:** Lanka previously used Seq as a dedicated log server. After adopting .NET Aspire, Seq was replaced by the Aspire Dashboard, which provides structured log search, filtering, and trace correlation in a single UI. The Seq sink has been removed from the configuration.

## How It Works

Serilog is configured in `appsettings.json` of each API project. The `writeToProviders: true` flag bridges Serilog output into ASP.NET Core's `ILoggerProvider` pipeline, where ServiceDefaults registers an OpenTelemetry log provider that exports logs via OTLP:

```csharp
// Program.cs (both Lanka.Api and Lanka.Gateway)
builder.Host.UseSerilog(
    (context, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(context.Configuration),
    writeToProviders: true);
```

```csharp
// ServiceDefaults/Extensions.cs
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});
```

The result: Serilog handles log formatting and the console sink, while OpenTelemetry captures the same entries and exports them to the Aspire Dashboard. Logs are automatically correlated with their parent trace — clicking a log entry in the Dashboard reveals the full distributed trace.

## Configuration

### Sinks

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```

The console sink provides local output. All log aggregation and search is handled by the Aspire Dashboard via the OpenTelemetry bridge.

### Minimum Level

The minimum level controls which logs are captured:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information",
        "Lanka.Modules.Users.Infrastructure.Outbox": "Warning",
        "Lanka.Modules.Users.Infrastructure.Inbox": "Warning",
        "Lanka.Modules.Campaigns.Infrastructure.Outbox": "Warning",
        "Lanka.Modules.Campaigns.Infrastructure.Inbox": "Warning"
      }
    }
  }
}
```

Outbox/Inbox processors are set to `Warning` to reduce noise from their frequent polling.

### Enrichers

* **FromLogContext** — Adds properties from the log context to log events
* **WithMachineName** — Adds the machine name
* **WithThreadId** — Adds the thread ID

## Code Examples

### Logging Structured Data

```csharp
_logger.LogInformation("User registered: {UserId}, {Email}", userId, email);
```

Structured properties (`UserId`, `Email`) are preserved as queryable fields in the Aspire Dashboard, not flattened into a string.

### Trace Correlation via LogContext

The `LogContextTraceLoggingMiddleware` adds the current `TraceId` to the Serilog log context, so every log entry is linked to its distributed trace:

```csharp
using Serilog.Context;

public Task Invoke(HttpContext context)
{
    string traceId = Activity.Current?.TraceId.ToString();

    using (LogContext.PushProperty("TraceId", traceId))
    {
        return next.Invoke(context);
    }
}
```

In the Aspire Dashboard, this correlation is automatic — clicking a log entry navigates to the trace waterfall view.
