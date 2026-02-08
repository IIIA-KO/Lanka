using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace Lanka.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
            )
            .WithTracing(tracing =>
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql()
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource("Yarp.ReverseProxy")
            );

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        IHealthChecksBuilder healthChecks = builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        string? databaseCs = builder.Configuration.GetConnectionString("Database");

        if (!string.IsNullOrEmpty(databaseCs))
        {
            healthChecks.AddNpgSql(databaseCs);
        }

        string? cacheCs = builder.Configuration.GetConnectionString("Cache");

        if (!string.IsNullOrEmpty(cacheCs))
        {
            healthChecks.AddRedis(cacheCs);
        }

        string? queueCs = builder.Configuration.GetConnectionString("Queue");

        if (!string.IsNullOrEmpty(queueCs))
        {
            var queueUri = new Uri(queueCs);
            string[] userInfo = queueUri.UserInfo.Split(':', 2);
            string username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0])
                ? Uri.UnescapeDataString(userInfo[0])
                : "guest";
            string password = userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : "guest";

            builder.Services.TryAddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory()
                {
                    Uri = queueUri,
                    UserName = username,
                    Password = password,
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            healthChecks.AddRabbitMQ();
        }

        string? mongoCs = builder.Configuration.GetConnectionString("Mongo");

        if (!string.IsNullOrEmpty(mongoCs))
        {
            healthChecks.AddMongoDb();
        }

        string? elasticCs = builder.Configuration.GetConnectionString("lanka-search");

        if (!string.IsNullOrEmpty(elasticCs))
        {
            string elasticBaseUrl = new Uri(elasticCs).GetLeftPart(UriPartial.Authority);
            healthChecks.AddUrlGroup(new Uri(elasticBaseUrl), HttpMethod.Get, "elasticsearch");
        }

        string? keycloakHealthUrl = builder.Configuration.GetValue<string>("KeyCloak:HealthUrl");

        if (!string.IsNullOrEmpty(keycloakHealthUrl))
        {
            healthChecks.AddUrlGroup(new Uri(keycloakHealthUrl), HttpMethod.Get, "keycloak");
        }

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}
