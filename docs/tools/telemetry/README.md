# Telemetry in Lanka Project

Telemetry is the process of collecting and transmitting data from remote sources to a central location for monitoring and analysis. In the Lanka project, telemetry is crucial for gaining insights into the application's behavior, performance, and health. By collecting and analyzing telemetry data, we can identify issues, optimize performance, and ensure the reliability of the system.

This document provides an overview of the telemetry tools used in the Lanka project and how they work together to provide observability.

## Telemetry Tools

The Lanka project uses the following tools for telemetry:

* **OpenTelemetry:** A vendor-neutral, open-source observability framework for generating, collecting, and exporting telemetry data such as traces, metrics, and logs.
* **Serilog & Seq:** A structured logging library for .NET applications (Serilog) and a centralized log server for collecting and analyzing logs (Seq).
* **Jaeger:** An open-source, distributed tracing system used for monitoring and troubleshooting complex microservices-based applications.

## How They Work Together

These tools work together to provide a comprehensive telemetry solution:

1. The application is instrumented with OpenTelemetry to generate traces and metrics.
2. Serilog is used to capture structured logs from the application. The TraceId is added to the log context to correlate logs with traces.
3. OpenTelemetry exports telemetry data to Jaeger for trace visualization.
4. Serilog sends logs to Seq for centralized log management and analysis.

![CQRS](/docs/images/telemetry.jpg)

## Detailed Documentation

* [OpenTelemetry Integration](./open-telemetry/README.md)
* [Serilog and Seq for Structured Logging](./serilog-seq/README.md)
* [Jaeger for Trace Visualization](./jaeger/README.md)
