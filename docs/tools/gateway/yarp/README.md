# YARP Integration

YARP (Yet Another Reverse Proxy) is a reverse proxy toolkit for .NET that is used to route requests to backend services. It is highly customizable and can be configured to meet the specific needs of the Lanka project.

## Usage in Lanka Project

In the Lanka project, YARP is used to:

* Route requests to backend services based on predefined routes and clusters.
* Load balance requests across multiple instances of a backend service.
* Provide a single entry point for the API.

## Configuration

YARP is configured in the `appsettings.json` file of the `Lanka.Gateway` project.

### Routes Configuration

Routes are configured in the `ReverseProxy.Routes` section of the `appsettings.json` file. Each route defines a set of matchers and a cluster to route requests to.

*   **Example (`src/Api/Lanka.Gateway/appsettings.json`):**

    ```json
    {
      "ReverseProxy": {
        "Routes": {
          "route1": {
            "ClusterId": "cluster1",
            "Match": {
              "Path": "/api/{**catch-all}"
            }
          }
        }
      }
    }
    ```

### Clusters Configuration

Clusters are configured in the `ReverseProxy.Clusters` section of the `appsettings.json` file. Each cluster defines a set of destinations (backend services) to route requests to.

*   **Example (`src/Api/Lanka.Gateway/appsettings.json`):**

    ```json
    {
      "ReverseProxy": {
        "Clusters": {
          "cluster1": {
            "Destinations": {
              "destination1": {
                "Address": "https://localhost:4307/"
              }
            }
          }
        }
      }
    }
    ```

## Code Examples

### Configuring YARP Routes and Clusters

YARP routes and clusters are configured in the `ConfigureReverserProxy` extension method in `ApplicationExtensions.cs`.

```csharp
builder
    .Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
```

### Using YARP Middleware

YARP middleware is used to route requests to backend services.

```csharp
app.MapReverseProxy()
    .RequireRateLimiting(RateLimitingConfig.FixedByIp);
```
