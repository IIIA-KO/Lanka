using System.Threading.RateLimiting;
using Lanka.Gateway.Authentication;
using Lanka.Gateway.Middleware;
using Lanka.Gateway.OpenTelemetry;
using Lanka.Gateway.RateLimiting;
using Lanka.Gateway.Resiliency;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;
using Yarp.ReverseProxy.Forwarder;

namespace Lanka.Gateway.Extensions;

internal static class ApplicationExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(context.Configuration)
        );

        return builder;
    }

    public static WebApplicationBuilder ConfigureRateLimiting(this WebApplicationBuilder builder)
    {
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

        return builder;
    }

    public static WebApplicationBuilder ConfigureReverserProxy(this WebApplicationBuilder builder)
    {
        ResiliencePipeline<HttpResponseMessage> pipeline = ResiliencePolicyBuilder.Build();
        
        builder.Services.AddSingleton<IForwarderHttpClientFactory>(
            _ => new ResilientHttpClientFactory(pipeline)
        );
        
        builder
            .Services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        return builder;
    }

    public static WebApplicationBuilder ConfigureTracing(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Yarp.ReverseProxy");

                tracing.AddOtlpExporter();
            });

        return builder;
    }

    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        builder.Services.AddAuthentication().AddJwtBearer();

        builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();

        return builder;
    }

    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseLogContextTraceLogging();
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapReverseProxy()
            .RequireRateLimiting(RateLimitingConfig.FixedByIp);

        return app;
    }
}
