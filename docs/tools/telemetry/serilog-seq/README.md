# Serilog and Seq for Structured Logging

Serilog is a structured logging library for .NET applications that makes it easy to capture and analyze logs. Seq is a centralized log server that provides a powerful and user-friendly interface for collecting, searching, and analyzing logs.

## Usage in Lanka Project

In the Lanka project, Serilog is used to capture structured logs from the application. The TraceId is added to the log context to correlate logs with traces. Seq is used to collect and analyze these logs.

## Configuration

Serilog is configured in the `appsettings.json` file of each API project (e.g., `Lanka.Api`, `Lanka.Gateway`).

### Sinks

The following sinks are configured in the Lanka project:

*   **Console:** Writes logs to the console.
*   **Seq:** Sends logs to the Seq server.

    *   The Seq server URL is configured using the `serverUrl` argument.

        *   **Example (`src/Api/Lanka.Api/appsettings.json`):**

            ```json
            {
              "Serilog": {
                "WriteTo": [
                  {
                    "Name": "Console"
                  },
                  {
                    "Name": "Seq",
                    "Args": {
                      "serverUrl": "http://lanka.seq:5341"
                    }
                  }
                ]
              }
            }
            ```

### Minimum Level

The minimum level of logs to be captured is configured using the `MinimumLevel` setting.

*   **Example (`src/Api/Lanka.Api/appsettings.json`):**

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

### Enrichers

The following enrichers are configured in the Lanka project:

*   **FromLogContext:** Adds properties from the log context to the log events.
*   **WithMachineName:** Adds the machine name to the log events.
*   **WithThreadId:** Adds the thread ID to the log events.

### Properties

The `Application` property is configured to identify the application in the logs.

*   **Example (`src/Api/Lanka.Api/appsettings.json`):**

    ```json
    {
      "Serilog": {
        "Properties": {
          "Application": "Lanka.Api"
        }
      }
    }
    ```

## Code Examples

### Logging Structured Data

You can log structured data by passing objects to the log methods.

```csharp
_logger.LogInformation("User registered: {UserId}, {Email}", userId, email);
```



## Setting Up Seq

1. Run the Seq Docker container:

    ```bash
    docker run --name lanka.seq -e ACCEPT_EULA=Y -p 5431:5341 -p 8081:80 datalust/seq:latest
    ```

2. Access the Seq UI at `http://localhost:8081`.
3. Configure Serilog to send logs to Seq by specifying the Seq server URL in the `appsettings.json` file.
