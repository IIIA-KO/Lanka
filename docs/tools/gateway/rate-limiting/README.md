# Rate Limiting Configuration

Rate limiting is a technique used to control the rate of requests to an API, protecting it from abuse and ensuring that the system remains available and responsive.

## Usage in Lanka Project

In the Lanka project, rate limiting is used to:

* Protect the API from abuse.
* Ensure that the system remains available and responsive.
* Prevent denial-of-service attacks.

## Configuration

Rate limiting is configured in the `ConfigureRateLimiting` extension method in `ApplicationExtensions.cs`.

### Fixed Window Rate Limiter

The fixed window rate limiter is used to limit the number of requests from a client within a fixed time window.

## Code Examples

### Configuring Rate Limiting Policies

Rate limiting policies are configured in the `ConfigureRateLimiting` extension method in `ApplicationExtensions.cs`.

```csharp
builder
    .Services
    .AddRateLimiter(
        options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitingConfig.FixedByIp, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                  ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }
                )
            );
        }
    );
```

### Applying Rate Limiting Policies to YARP Routes

Rate limiting policies are applied to YARP routes using the `RequireRateLimiting` method.

```csharp
app.MapReverseProxy()
    .RequireRateLimiting(RateLimitingConfig.FixedByIp);
```
